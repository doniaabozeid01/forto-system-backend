using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.DTOs.Bookings.cashier;
using Forto.Application.DTOs.Bookings;
using Forto.Domain.Entities.Billings;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Clients;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities.Employees;

namespace Forto.Application.Abstractions.Services.Bookings.Cashier
{
    public class BookingItemOpsService : IBookingItemOpsService
    {
        private readonly IUnitOfWork _uow;
        private readonly IInvoiceService _invoiceService;
        private readonly IBookingService _bookingService;

        public BookingItemOpsService(IUnitOfWork uow, IInvoiceService invoiceService, IBookingService bookingService)
        {
            _uow = uow;
            _invoiceService = invoiceService;
            _bookingService = bookingService;
        }

        public async Task<BookingResponse> AddServiceAsync(int bookingId, AddServiceToBookingRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();
            var carRepo = _uow.Repository<Car>();
            var rateRepo = _uow.Repository<ServiceRate>();
            var empServiceRepo = _uow.Repository<EmployeeService>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);
            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                throw new BusinessException("Cannot modify a closed booking", 409);

            if (request.AssignedEmployeeId <= 0)
                throw new BusinessException("AssignedEmployeeId is required", 400);




            var car = await carRepo.GetByIdAsync(booking.CarId);
            if (car == null) throw new BusinessException("Car not found", 404);

            // employee qualification
            var qualified = await empServiceRepo.AnyAsync(x =>
                x.EmployeeId == request.AssignedEmployeeId &&
                x.ServiceId == request.ServiceId &&
                x.IsActive);
            if (!qualified)
                throw new BusinessException("Employee is not qualified for this service", 409);

            // get rate snapshot
            var rate = (await rateRepo.FindAsync(r =>
                r.IsActive &&
                r.ServiceId == request.ServiceId &&
                r.BodyType == car.BodyType)).FirstOrDefault();

            if (rate == null)
                throw new BusinessException("No rate for this service/body type", 409);

            var item = new BookingItem
            {
                BookingId = booking.Id,
                ServiceId = request.ServiceId,
                BodyType = car.BodyType,
                UnitPrice = rate.Price,
                DurationMinutes = rate.DurationMinutes,
                AssignedEmployeeId = request.AssignedEmployeeId,
                Status = booking.Status == BookingStatus.InProgress ? BookingItemStatus.InProgress : BookingItemStatus.Pending,
                StartedAt = booking.Status == BookingStatus.InProgress ? DateTime.UtcNow : null
            };

            await itemRepo.AddAsync(item);
            await _uow.SaveChangesAsync(); // get item.Id

            // if booking already started => reserve materials now
            if (booking.Status == BookingStatus.InProgress)
            {
                await ReserveMaterialsForItemAsync(booking, item, request.CashierId);
            }

            await _uow.SaveChangesAsync();

            // if invoice exists and unpaid -> rebuild service lines (avoid deleting products/gifts)
            var invRepo = _uow.Repository<Invoice>();
            var existingInv = (await invRepo.FindAsync(x => x.BookingId == booking.Id)).FirstOrDefault();
            if (existingInv != null && existingInv.Status != InvoiceStatus.Paid)
            {
                await RebuildServiceLinesForInvoiceAsync(existingInv.Id, booking.Id);
                await _uow.SaveChangesAsync();
            }

            var refreshed = await _bookingService.GetByIdAsync(bookingId);
            return refreshed ?? throw new BusinessException("Booking not found", 404);
        }

        public async Task<BookingResponse> CancelServiceAsync(int bookingItemId, CancelBookingItemByCashierRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var itemRepo = _uow.Repository<BookingItem>();
            var bookingRepo = _uow.Repository<Booking>();
            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var movementRepo = _uow.Repository<MaterialMovement>();
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();
            var serviceRepo = _uow.Repository<Service>();

            var item = await itemRepo.GetByIdAsync(bookingItemId);
            if (item == null) throw new BusinessException("Booking item not found", 404);

            if (item.Status == BookingItemStatus.Done)
                throw new BusinessException("Cannot cancel a completed service (refund flow needed)", 409);

            var booking = await bookingRepo.GetByIdAsync(item.BookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            // Ensure invoice exists (so we can charge materials used if needed)
            var invoice = (await invRepo.FindAsync(x => x.BookingId == booking.Id)).FirstOrDefault();
            if (invoice == null)
            {
                var invResp = await _invoiceService.EnsureInvoiceForBookingAsync(booking.Id);
                // reload invoice entity
                invoice = await invRepo.GetByIdAsync(invResp.Id);
            }

            if (invoice != null && invoice.Status == InvoiceStatus.Paid)
                throw new BusinessException("Cannot cancel after payment (refund flow needed)", 409);

            // Pending: just cancel + rebuild service lines
            if (item.Status == BookingItemStatus.Pending)
            {
                item.Status = BookingItemStatus.Cancelled;
                itemRepo.Update(item);

                if (invoice != null)
                {
                    await RebuildServiceLinesForInvoiceAsync(invoice.Id, booking.Id);
                }

                await _uow.SaveChangesAsync();

                var refreshed = await _bookingService.GetByIdAsync(booking.Id);
                return refreshed ?? throw new BusinessException("Booking not found", 404);
            }

            // InProgress: charge materials used to invoice (UnitCharge), consume stock, release reserved
            var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == item.Id);
            if (usages.Count == 0)
                throw new BusinessException("No materials were reserved for this item (start first)", 409);

            // apply override if provided
            var overrideMap = request.UsedOverride?.ToDictionary(x => x.MaterialId, x => x.ActualQty) ?? new Dictionary<int, decimal>();

            var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();
            var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == booking.BranchId && materialIds.Contains(s.MaterialId));
            var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

            decimal materialsCharge = 0m;

            foreach (var u in usages)
            {
                if (!stockMap.TryGetValue(u.MaterialId, out var stock))
                    throw new BusinessException("Stock row missing for a material in this branch", 409);

                var actualUsed = overrideMap.TryGetValue(u.MaterialId, out var ov) ? Math.Max(0, ov) : u.ActualQty;

                // charge customer for used materials: ActualQty * UnitCharge
                materialsCharge += actualUsed * u.UnitCharge;

                // consume onhand (used)
                stock.OnHandQty -= actualUsed;
                if (stock.OnHandQty < 0) stock.OnHandQty = 0;

                // release reserved
                stock.ReservedQty -= u.ReservedQty;
                if (stock.ReservedQty < 0) stock.ReservedQty = 0;

                stockRepo.Update(stock);

                // update usage to freeze
                u.ActualQty = actualUsed;
                u.ReservedQty = 0;
                u.ExtraCharge = 0; // service canceled, extra adjustment not used
                usageRepo.Update(u);

                // record consume movement (it WAS consumed)
                await movementRepo.AddAsync(new MaterialMovement
                {
                    BranchId = booking.BranchId,
                    MaterialId = u.MaterialId,
                    MovementType = MaterialMovementType.Consume,
                    Qty = actualUsed,
                    UnitCostSnapshot = u.UnitCost,
                    TotalCost = actualUsed * u.UnitCost,
                    OccurredAt = DateTime.UtcNow,
                    BookingId = booking.Id,
                    BookingItemId = item.Id,
                    RecordedByEmployeeId = request.CashierId,
                    Notes = "Consumed on cancel"
                });
            }

            // add invoice line for materials used
            var svc = await serviceRepo.GetByIdAsync(item.ServiceId);
            var serviceName = svc?.Name ?? "Service";

            if (invoice != null && materialsCharge > 0)
            {
                await lineRepo.AddAsync(new InvoiceLine
                {
                    InvoiceId = invoice.Id,
                    Description = $"Materials used (cancelled) - {serviceName}",
                    Qty = 1,
                    UnitPrice = materialsCharge,
                    Total = materialsCharge
                });

                // rebuild service lines (this will remove canceled service line)
                await RebuildServiceLinesForInvoiceAsync(invoice.Id, booking.Id);
            }

            // cancel item
            item.Status = BookingItemStatus.Cancelled;
            item.MaterialAdjustment = 0;
            itemRepo.Update(item);

            await _uow.SaveChangesAsync();

            var refreshed2 = await _bookingService.GetByIdAsync(booking.Id);
            return refreshed2 ?? throw new BusinessException("Booking not found", 404);
        }

        // same helper from lifecycle (reuse or move to shared)
        private async Task ReserveMaterialsForItemAsync(Booking booking, BookingItem item, int cashierId)
        {
            var recipeRepo = _uow.Repository<ServiceMaterialRecipe>();
            var materialRepo = _uow.Repository<Material>();
            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();

            var recipeRows = await recipeRepo.FindAsync(r => r.IsActive && r.ServiceId == item.ServiceId && r.BodyType == item.BodyType);
            if (recipeRows.Count == 0) throw new BusinessException("Missing recipe for this service and car type", 409);

            var materialIds = recipeRows.Select(r => r.MaterialId).Distinct().ToList();
            var mats = await materialRepo.FindAsync(m => materialIds.Contains(m.Id) && m.IsActive);
            var matMap = mats.ToDictionary(m => m.Id, m => m);

            var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == booking.BranchId && materialIds.Contains(s.MaterialId));
            var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

            foreach (var row in recipeRows)
            {
                if (!stockMap.TryGetValue(row.MaterialId, out var stock))
                    throw new BusinessException("Stock row missing for material in this branch", 409);

                var available = stock.OnHandQty - stock.ReservedQty;
                if (available < 0) available = 0;
                if (available < row.DefaultQty) throw new BusinessException("Not enough stock", 409);
            }

            foreach (var row in recipeRows)
            {
                var stock = stockMap[row.MaterialId];
                stock.ReservedQty += row.DefaultQty;
                stockRepo.Update(stock);

                var mat = matMap[row.MaterialId];

                await usageRepo.AddAsync(new BookingItemMaterialUsage
                {
                    BookingItemId = item.Id,
                    MaterialId = row.MaterialId,
                    DefaultQty = row.DefaultQty,
                    ReservedQty = row.DefaultQty,
                    ActualQty = row.DefaultQty,
                    UnitCost = mat.CostPerUnit,
                    UnitCharge = mat.ChargePerUnit,
                    ExtraCharge = 0,
                    RecordedByEmployeeId = cashierId,
                    RecordedAt = DateTime.UtcNow
                });
            }
        }











        private async Task RequireCashierAsync(int cashierId)
        {
            var emp = await _uow.Repository<Employee>().GetByIdAsync(cashierId);
            if (emp == null || !emp.IsActive)
                throw new BusinessException("Cashier not found", 404);

            if (emp.Role != EmployeeRole.Cashier &&
                emp.Role != EmployeeRole.Supervisor &&
                emp.Role != EmployeeRole.Admin)
                throw new BusinessException("Not allowed", 403);
        }

        // Important: rebuild ONLY service lines, keep Product/Gift/Materials used lines
        private async Task RebuildServiceLinesForInvoiceAsync(int invoiceId, int bookingId)
        {
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();
            var itemRepo = _uow.Repository<BookingItem>();
            var serviceRepo = _uow.Repository<Service>();

            var inv = await invRepo.GetByIdAsync(invoiceId);
            if (inv == null) return;

            if (inv.Status == InvoiceStatus.Paid)
                throw new BusinessException("Cannot change invoice after payment", 409);

            // delete only service lines
            var existingLines = await lineRepo.FindAsync(l => l.InvoiceId == invoiceId);
            foreach (var l in existingLines.Where(x => (x.Description ?? "").Trim().StartsWith("Service:", StringComparison.OrdinalIgnoreCase)))
                lineRepo.Delete(l);

            // rebuild service lines from booking items (not cancelled)
            var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

            var serviceIds = items.Select(i => i.ServiceId).Distinct().ToList();
            var services = serviceIds.Count == 0 ? new List<Service>() : await serviceRepo.FindAsync(s => serviceIds.Contains(s.Id));
            var map = services.ToDictionary(s => s.Id, s => s.Name);

            foreach (var it in items)
            {
                map.TryGetValue(it.ServiceId, out var name);

                var lineTotal = it.UnitPrice + it.MaterialAdjustment;
                if (lineTotal < 0) lineTotal = 0;

                await lineRepo.AddAsync(new InvoiceLine
                {
                    InvoiceId = invoiceId,
                    Description = $"Service: {(string.IsNullOrWhiteSpace(name) ? "Service" : name)}",
                    Qty = 1,
                    UnitPrice = it.UnitPrice,
                    Total = lineTotal
                });
            }

            // recompute invoice totals from ALL lines (services + products + gifts + materials used)
            var allLinesAfter = await lineRepo.FindAsync(l => l.InvoiceId == invoiceId);
            var subTotal = allLinesAfter.Sum(l => l.Total);

            RecalcInvoiceTotals(inv, subTotal);
            invRepo.Update(inv);
        }


        private const decimal DefaultVatRate = 0.14m;

        private static void RecalcInvoiceTotals(Invoice inv, decimal subTotal)
        {
            inv.SubTotal = subTotal;
            inv.TaxRate = DefaultVatRate;
            inv.TaxAmount = Math.Round(inv.SubTotal * inv.TaxRate, 2);
            inv.Total = inv.SubTotal + inv.TaxAmount - inv.Discount;
            if (inv.Total < 0) inv.Total = 0;
        }

    }

}
