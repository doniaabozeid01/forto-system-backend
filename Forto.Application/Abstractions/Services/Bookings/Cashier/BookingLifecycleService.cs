using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.DTOs.Bookings;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Bookings.Cashier
{
    public class BookingLifecycleService : IBookingLifecycleService
    {
        private readonly IUnitOfWork _uow;
        private readonly IInvoiceService _invoiceService; // for ensure invoice later
        private readonly IBookingService _bookingService; // for GetById response (or map yourself)

        public BookingLifecycleService(IUnitOfWork uow, IInvoiceService invoiceService, IBookingService bookingService)
        {
            _uow = uow;
            _invoiceService = invoiceService;
            _bookingService = bookingService;
        }

        //public async Task<BookingResponse> StartBookingAsync(int bookingId, int cashierId)
        //{
        //    await RequireCashierAsync(cashierId);

        //    var bookingRepo = _uow.Repository<Booking>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var booking = await bookingRepo.GetByIdAsync(bookingId);
        //    if (booking == null) throw new BusinessException("Booking not found", 404);

        //    if (booking.Status != BookingStatus.Pending)
        //        throw new BusinessException("Booking cannot be started in its current status", 409);

        //    var items = await itemRepo.FindTrackingAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);
        //    if (items.Count == 0) throw new BusinessException("No services to start", 409);

        //    // require each item assigned to employee (you wanted to know who works)
        //    if (items.Any(i => i.AssignedEmployeeId == null))
        //        throw new BusinessException("All services must be assigned to an employee before starting the booking", 409);

        //    // reserve materials for all items
        //    foreach (var item in items)
        //    {
        //        await ReserveMaterialsForItemAsync(booking, item, cashierId);
        //        item.Status = BookingItemStatus.InProgress;
        //        item.StartedAt = DateTime.UtcNow;
        //        itemRepo.Update(item);
        //    }

        //    booking.Status = BookingStatus.InProgress;
        //    bookingRepo.Update(booking);

        //    await _uow.SaveChangesAsync();

        //    // return fresh
        //    var refreshed = await _bookingService.GetByIdAsync(bookingId);
        //    return refreshed ?? throw new BusinessException("Booking not found", 404);
        //}



        public async Task<BookingResponse> StartBookingAsync(int bookingId, int cashierId)
        {
            await RequireCashierAsync(cashierId);

            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            // ✅ بدل ما نمنع تمامًا، نخليها safe:
            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                throw new BusinessException("Booking is closed and cannot be started", 409);

            var items = await itemRepo.FindTrackingAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);
            if (items.Count == 0) throw new BusinessException("No services to start", 409);

            if (items.Any(i => i.AssignedEmployeeId == null))
                throw new BusinessException("All services must be assigned to an employee before starting the booking", 409);

            // ✅ start only pending items
            var toStart = items.Where(i => i.Status == BookingItemStatus.Pending).ToList();

            // لو مفيش pending items:
            // - لو booking already InProgress يبقى OK
            // - لو booking Pending لكن items مش pending (غريب) نرجع خطأ
            if (toStart.Count == 0)
            {
                if (booking.Status == BookingStatus.InProgress)
                {
                    var already = await _bookingService.GetByIdAsync(bookingId);
                    return already ?? throw new BusinessException("Booking not found", 404);
                }

                // booking pending but nothing to start -> data inconsistency
                throw new BusinessException("No pending services to start", 409);
            }

            foreach (var item in toStart)
            {
                await ReserveMaterialsForItemAsync(booking, item, cashierId);

                item.Status = BookingItemStatus.InProgress;
                item.StartedAt = DateTime.UtcNow;
                itemRepo.Update(item);
            }

            // ✅ set booking InProgress if it was Pending
            if (booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.InProgress;
                bookingRepo.Update(booking);
            }

            await _uow.SaveChangesAsync();

            var refreshed = await _bookingService.GetByIdAsync(bookingId);
            return refreshed ?? throw new BusinessException("Booking not found", 404);
        }


        public async Task<BookingResponse> CompleteBookingAsync(int bookingId, int cashierId)
        {
            await RequireCashierAsync(cashierId);

            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();
            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var movementRepo = _uow.Repository<MaterialMovement>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            if (booking.Status != BookingStatus.InProgress)
                throw new BusinessException("Booking cannot be completed in its current status", 409);

            var items = await itemRepo.FindTrackingAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);
            if (items.Count == 0) throw new BusinessException("No services to complete", 409);

            var now = DateTime.UtcNow;

            // consume per item
            foreach (var item in items)
            {
                if (item.Status != BookingItemStatus.InProgress)
                    throw new BusinessException("All services must be InProgress before completing booking", 409);

                var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == item.Id);
                if (usages.Count == 0)
                    throw new BusinessException("Missing reserved materials for a service (start booking first)", 409);

                var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();
                var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == booking.BranchId && materialIds.Contains(s.MaterialId));
                var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

                // idempotency on movements
                var existingConsume = await movementRepo.FindAsync(m =>
                    m.BookingItemId == item.Id &&
                    m.MovementType == MaterialMovementType.Consume);

                var alreadyLogged = existingConsume.Select(m => m.MaterialId).ToHashSet();

                foreach (var u in usages)
                {
                    if (!stockMap.TryGetValue(u.MaterialId, out var stock))
                        throw new BusinessException("Stock row missing for a material in this branch", 409);

                    var availableNow = stock.OnHandQty - stock.ReservedQty + u.ReservedQty;
                    if (availableNow < 0) availableNow = 0;

                    if (availableNow < u.ActualQty)
                        throw new BusinessException("Not enough stock to complete booking", 409);

                    // release + consume
                    stock.ReservedQty -= u.ReservedQty;
                    if (stock.ReservedQty < 0) stock.ReservedQty = 0;

                    stock.OnHandQty -= u.ActualQty;
                    if (stock.OnHandQty < 0) stock.OnHandQty = 0;

                    stockRepo.Update(stock);

                    // release usage reserved
                    u.ReservedQty = 0;
                    usageRepo.Update(u);

                    // movement consume
                    if (!alreadyLogged.Contains(u.MaterialId) && u.ActualQty > 0)
                    {
                        await movementRepo.AddAsync(new MaterialMovement
                        {
                            BranchId = booking.BranchId,
                            MaterialId = u.MaterialId,
                            MovementType = MaterialMovementType.Consume,
                            Qty = u.ActualQty,
                            UnitCostSnapshot = u.UnitCost,
                            TotalCost = u.ActualQty * u.UnitCost,
                            OccurredAt = now,
                            BookingId = booking.Id,
                            BookingItemId = item.Id,
                            RecordedByEmployeeId = item.AssignedEmployeeId ?? cashierId,
                            Notes = $"Completed by cashier {cashierId}"
                        });
                    }
                }

                item.MaterialAdjustment = usages.Sum(u => u.ExtraCharge);
                item.Status = BookingItemStatus.Done;
                item.CompletedAt = now;
                itemRepo.Update(item);
            }

            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = now;
            bookingRepo.Update(booking);

            await _uow.SaveChangesAsync();

            // ensure invoice + rebuild service lines only
            var invoice = await _invoiceService.EnsureInvoiceForBookingAsync(bookingId);
            // Ensure already returns Map, but we still want to rebuild service lines if items changed.
            // (If your Ensure already rebuilds lines, ignore this)
            // await RebuildServiceLinesForInvoiceAsync(invoice.Id, bookingId);

            var refreshed = await _bookingService.GetByIdAsync(bookingId);
            return refreshed ?? throw new BusinessException("Booking not found", 404);
        }



        // --------- Reservation helper (same logic as StartItem but reused) ----------
        //private async Task ReserveMaterialsForItemAsync(Booking booking, BookingItem item, int cashierId)
        //{
        //    var recipeRepo = _uow.Repository<ServiceMaterialRecipe>();
        //    var materialRepo = _uow.Repository<Material>();
        //    var stockRepo = _uow.Repository<BranchMaterialStock>();
        //    var usageRepo = _uow.Repository<BookingItemMaterialUsage>();

        //    var recipeRows = await recipeRepo.FindAsync(r =>
        //        r.IsActive && r.ServiceId == item.ServiceId && r.BodyType == item.BodyType);

        //    if (recipeRows.Count == 0)
        //        throw new BusinessException("Missing recipe for this service and car type", 409);

        //    var materialIds = recipeRows.Select(r => r.MaterialId).Distinct().ToList();
        //    var mats = await materialRepo.FindAsync(m => materialIds.Contains(m.Id) && m.IsActive);
        //    var matMap = mats.ToDictionary(m => m.Id, m => m);

        //    var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == booking.BranchId && materialIds.Contains(s.MaterialId));
        //    var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

        //    // check availability first
        //    foreach (var row in recipeRows)
        //    {
        //        if (!stockMap.TryGetValue(row.MaterialId, out var stock))
        //            throw new BusinessException("Stock row missing for material in this branch", 409);

        //        var available = stock.OnHandQty - stock.ReservedQty;
        //        if (available < 0) available = 0;

        //        if (available < row.DefaultQty)
        //            throw new BusinessException("Not enough stock to start booking", 409);
        //    }

        //    // reserve + usage
        //    foreach (var row in recipeRows)
        //    {
        //        var stock = stockMap[row.MaterialId];
        //        stock.ReservedQty += row.DefaultQty;
        //        stockRepo.Update(stock);

        //        var mat = matMap[row.MaterialId];

        //        await usageRepo.AddAsync(new BookingItemMaterialUsage
        //        {
        //            BookingItemId = item.Id,
        //            MaterialId = row.MaterialId,
        //            DefaultQty = row.DefaultQty,
        //            ReservedQty = row.DefaultQty,
        //            ActualQty = row.DefaultQty,
        //            UnitCost = mat.CostPerUnit,
        //            UnitCharge = mat.ChargePerUnit,
        //            ExtraCharge = 0,
        //            RecordedByEmployeeId = cashierId,
        //            RecordedAt = DateTime.UtcNow
        //        });
        //    }
        //}



        private async Task ReserveMaterialsForItemAsync(Booking booking, BookingItem item, int cashierId)
        {
            var recipeRepo = _uow.Repository<ServiceMaterialRecipe>();
            var materialRepo = _uow.Repository<Material>();
            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();

            // ✅ IMPORTANT: idempotency guard
            // لو usages موجودة بالفعل لنفس bookingItem -> متعملش insert تاني
            var existingUsages = await usageRepo.FindAsync(u => u.BookingItemId == item.Id);
            if (existingUsages.Count > 0)
            {
                // already reserved/created before
                return;
            }

            var recipeRows = await recipeRepo.FindAsync(r =>
                r.IsActive && r.ServiceId == item.ServiceId && r.BodyType == item.BodyType);

            if (recipeRows.Count == 0)
                throw new BusinessException("Missing recipe for this service and car type", 409);

            var materialIds = recipeRows.Select(r => r.MaterialId).Distinct().ToList();
            var mats = await materialRepo.FindAsync(m => materialIds.Contains(m.Id) && m.IsActive);
            var matMap = mats.ToDictionary(m => m.Id, m => m);

            var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == booking.BranchId && materialIds.Contains(s.MaterialId));
            var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

            // check availability first
            foreach (var row in recipeRows)
            {
                if (!stockMap.TryGetValue(row.MaterialId, out var stock))
                    throw new BusinessException("Stock row missing for material in this branch", 409);

                var available = stock.OnHandQty - stock.ReservedQty;
                if (available < 0) available = 0;

                if (available < row.DefaultQty)
                    throw new BusinessException("Not enough stock to start booking", 409);
            }

            // reserve + usage
            foreach (var row in recipeRows)
            {
                if (!stockMap.TryGetValue(row.MaterialId, out var stock))
                    throw new BusinessException("Stock row missing for material in this branch", 409);

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
            var empRepo = _uow.Repository<Employee>();
            var emp = await empRepo.GetByIdAsync(cashierId);

            if (emp == null || !emp.IsActive)
                throw new BusinessException("Cashier not found", 404);

            if (emp.Role != EmployeeRole.Cashier &&
                emp.Role != EmployeeRole.Supervisor &&
                emp.Role != EmployeeRole.Admin)
                throw new BusinessException("Not allowed", 403);
        }


    }

}
