using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Bookings.cashier.checkout;
using Forto.Application.DTOs.Bookings;
using Forto.Domain.Entities.Billings;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Clients;
using Forto.Domain.Entities.Employees;

namespace Forto.Application.Abstractions.Services.Bookings.Cashier.checkout
{
        public class CashierCheckoutService : ICashierCheckoutService
        {
            private readonly IUnitOfWork _uow;
            private readonly IBookingService _bookingService;
            private readonly IBookingLifecycleService _lifecycle;
            private readonly IInvoiceService _invoiceService;

            public CashierCheckoutService(
                IUnitOfWork uow,
                IBookingService bookingService,
                IBookingLifecycleService lifecycle,
                IInvoiceService invoiceService)
            {
                _uow = uow;
                _bookingService = bookingService;
                _lifecycle = lifecycle;
                _invoiceService = invoiceService;
            }






        //public async Task<InvoiceResponse> CheckoutNowAsync(CashierCheckoutRequest request)
        //{
        //    // 1) QuickCreate booking (convert ServiceAssignments types)
        //    var quickCreate = new QuickCreateBookingRequest
        //    {
        //        BranchId = request.BranchId,
        //        ScheduledStart = request.ScheduledStart,
        //        ServiceIds = request.ServiceIds,
        //        Notes = request.Notes,

        //        Client = new ClientInput
        //        {
        //            PhoneNumber = request.Client.PhoneNumber,
        //            FullName = request.Client.FullName,
        //            Email = request.Client.Email
        //        },

        //        Car = new CarInput
        //        {
        //            PlateNumber = request.Car.PlateNumber,
        //            BodyType = request.Car.BodyType,
        //            Brand = request.Car.Brand,
        //            Model = request.Car.Model,
        //            Color = request.Car.Color,
        //            Year = request.Car.Year,
        //            IsDefault = request.Car.IsDefault
        //        },

        //        CreatedByType = BookingCreatedByType.Employee,
        //        CreatedByEmployeeId = request.CashierId,

        //        // ✅ convert: cashier.checkout.ServiceAssignmentDto -> Bookings.ServiceAssignmentDto
        //        ServiceAssignments = request.ServiceAssignments?
        //            .Select(x => new Forto.Application.DTOs.Bookings.ServiceAssignmentDto
        //            {
        //                ServiceId = x.ServiceId,
        //                EmployeeId = x.EmployeeId
        //            })
        //            .ToList()
        //    };

        //    var booking = await _bookingService.QuickCreateAsync(quickCreate);

        //    // 2) Start booking (requires all items assigned)
        //    await _lifecycle.StartBookingAsync(booking.Id, request.CashierId);

        //    // 3) Complete booking
        //    await _lifecycle.CompleteBookingAsync(booking.Id, request.CashierId);

        //    // 4) Ensure invoice (Unpaid)
        //    var inv = await _invoiceService.EnsureInvoiceForBookingAsync(booking.Id);

        //    // 5) Add products to same invoice (optional)
        //    if (request.Products != null && request.Products.Count > 0)
        //    {
        //        var products = request.Products
        //            .Select(p => new PosProductItemDto { ProductId = p.ProductId, Qty = p.Qty })
        //            .ToList();

        //        await AddProductsToInvoiceAsync(inv.Id, request.CashierId, products);
        //    }

        //    // 6) Pay cash immediately
        //    await PayInvoiceCashAsync(inv.Id, request.CashierId, DateTime.UtcNow);

        //    // 7) Return final invoice (no IInvoiceService.GetByIdAsync -> fetch from repo)
        //    var invRepo = _uow.Repository<Invoice>();
        //    var lineRepo = _uow.Repository<InvoiceLine>();

        //    var fresh = await invRepo.GetByIdAsync(inv.Id);
        //    if (fresh == null) return inv;

        //    var lines = await lineRepo.FindAsync(l => l.InvoiceId == fresh.Id);
        //    fresh.Lines = lines.ToList();

        //    return MapInvoice(fresh);

        //}














        //// -------- helper DTO to avoid ambiguous PosInvoiceItemDto ----------
        //private class PosProductItemDto
        //{
        //    public int ProductId { get; set; }
        //    public decimal Qty { get; set; }
        //}

        //private async Task AddProductsToInvoiceAsync(int invoiceId, int cashierId, List<PosProductItemDto> products)
        //{
        //    var invRepo = _uow.Repository<Invoice>();
        //    var lineRepo = _uow.Repository<InvoiceLine>();
        //    var prodRepo = _uow.Repository<Product>();
        //    var stockRepo = _uow.Repository<BranchProductStock>();
        //    var moveRepo = _uow.Repository<ProductMovement>();

        //    var invoice = await invRepo.GetByIdAsync(invoiceId);
        //    if (invoice == null) throw new BusinessException("Invoice not found", 404);
        //    if (invoice.Status == InvoiceStatus.Paid) throw new BusinessException("Invoice already paid", 409);

        //    if (!invoice.BranchId.HasValue)
        //        throw new BusinessException("Invoice BranchId is missing", 409);

        //    var branchId = invoice.BranchId.Value;

        //    var productIds = products.Select(x => x.ProductId).Distinct().ToList();
        //    var dbProducts = await prodRepo.FindAsync(p => productIds.Contains(p.Id) && p.IsActive);
        //    var pMap = dbProducts.ToDictionary(p => p.Id, p => p);

        //    var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && productIds.Contains(s.ProductId));
        //    var sMap = stocks.ToDictionary(s => s.ProductId, s => s);

        //    foreach (var it in products)
        //    {
        //        if (!pMap.TryGetValue(it.ProductId, out var p))
        //            throw new BusinessException($"Product {it.ProductId} not found", 404);

        //        if (!sMap.TryGetValue(it.ProductId, out var st))
        //            throw new BusinessException("Product stock not found in this branch", 409);

        //        var available = st.OnHandQty - st.ReservedQty;
        //        if (available < 0) available = 0;

        //        if (available < it.Qty)
        //            throw new BusinessException($"Not enough stock for product {it.ProductId}", 409);

        //        // stock
        //        st.OnHandQty -= it.Qty;
        //        stockRepo.Update(st);

        //        var lineTotal = it.Qty * p.SalePrice;

        //        await lineRepo.AddAsync(new InvoiceLine
        //        {
        //            InvoiceId = invoice.Id,
        //            Description = $"Product: {p.Name}",
        //            Qty = 1,
        //            UnitPrice = p.SalePrice,
        //            Total = lineTotal
        //        });

        //        await moveRepo.AddAsync(new ProductMovement
        //        {
        //            BranchId = branchId,
        //            ProductId = p.Id,
        //            MovementType = ProductMovementType.Sell,
        //            Qty = it.Qty,
        //            UnitCostSnapshot = p.CostPerUnit,
        //            TotalCost = it.Qty * p.CostPerUnit,
        //            OccurredAt = DateTime.UtcNow,
        //            InvoiceId = invoice.Id,
        //            BookingId = invoice.BookingId,
        //            BookingItemId = null,
        //            RecordedByEmployeeId = cashierId,
        //            Notes = "Checkout add products"
        //        });
        //    }

        //    // recompute invoice totals from ALL lines
        //    var allLines = await lineRepo.FindAsync(l => l.InvoiceId == invoice.Id);
        //    var newSubTotal = allLines.Sum(l => l.Total);

        //    RecalcInvoiceTotals(invoice, newSubTotal);
        //    invRepo.Update(invoice);

        //    await _uow.SaveChangesAsync();
        //}

        //private async Task PayInvoiceCashAsync(int invoiceId, int cashierId, DateTime paidAt)
        //{
        //    var invRepo = _uow.Repository<Invoice>();
        //    var invoice = await invRepo.GetByIdAsync(invoiceId);
        //    if (invoice == null) throw new BusinessException("Invoice not found", 404);

        //    if (invoice.Status == InvoiceStatus.Paid) return;

        //    invoice.Status = InvoiceStatus.Paid;
        //    invoice.PaymentMethod = PaymentMethod.Cash;
        //    invoice.PaidByEmployeeId = cashierId;
        //    invoice.PaidAt = paidAt;

        //    invRepo.Update(invoice);
        //    await _uow.SaveChangesAsync();
        //}

        //private const decimal DefaultVatRate = 0.14m;

        //private static void RecalcInvoiceTotals(Invoice inv, decimal subTotal)
        //{
        //    inv.SubTotal = Math.Round(subTotal, 2);
        //    inv.TaxRate = DefaultVatRate;
        //    inv.TaxAmount = Math.Round(inv.SubTotal * inv.TaxRate, 2);
        //    inv.Total = inv.SubTotal + inv.TaxAmount - inv.Discount;
        //    if (inv.Total < 0) inv.Total = 0;
        //}

        //// Map invoice entity -> response (use your existing Map if you have)
        //private InvoiceResponse MapInvoice(Invoice inv)
        //{
        //    return new InvoiceResponse
        //    {
        //        Id = inv.Id,
        //        InvoiceNumber = inv.InvoiceNumber,
        //        BookingId = inv.BookingId??0,
        //        SubTotal = inv.SubTotal,
        //        Discount = inv.Discount,
        //        Total = inv.Total,
        //        Status = inv.Status,
        //        PaymentMethod = inv.PaymentMethod,
        //        PaidAt = inv.PaidAt,
        //        PaidByEmployeeId = inv.PaidByEmployeeId,
        //        Lines = inv.Lines?.Select(l => new InvoiceLineResponse
        //        {
        //            Id = l.Id,
        //            Description = l.Description,
        //            Qty = l.Qty,
        //            UnitPrice = l.UnitPrice,
        //            Total = l.Total
        //        }).ToList() ?? new List<InvoiceLineResponse>()
        //    };
        //}


















        // .. . .. //
        public async Task<InvoiceResponse> CheckoutNowAsync(CashierCheckoutRequest request)
        {
            await RequireCashierAsync(request.CashierId);
            if (request.SupervisorId.HasValue)
                await RequireSupervisorAsync(request.SupervisorId.Value);

            // 1) QuickCreate booking (convert ServiceAssignments types)
            var quickCreate = new QuickCreateBookingRequest
            {
                BranchId = request.BranchId,
                ScheduledStart = request.ScheduledStart,
                ServiceIds = request.ServiceIds,
                Notes = request.Notes,

                Client = new ClientInput
                {
                    PhoneNumber = request.Client.PhoneNumber,
                    FullName = request.Client.FullName,
                    Email = request.Client.Email
                },

                Car = new CarInput
                {
                    PlateNumber = request.Car.PlateNumber,
                    BodyType = request.Car.BodyType,
                    Brand = request.Car.Brand,
                    Model = request.Car.Model,
                    Color = request.Car.Color,
                    Year = request.Car.Year,
                    IsDefault = request.Car.IsDefault
                },

                CreatedByType = BookingCreatedByType.Employee,
                CreatedByEmployeeId = request.CashierId,

                ServiceAssignments = request.ServiceAssignments?
                    .Select(x => new Forto.Application.DTOs.Bookings.ServiceAssignmentDto
                    {
                        ServiceId = x.ServiceId,
                        EmployeeId = x.EmployeeId
                    })
                    .ToList()
            };

            var booking = await _bookingService.QuickCreateAsync(quickCreate, true);

            // 2) Start booking (requires all items assigned)
            await _lifecycle.StartBookingAsync(booking.Id, request.CashierId);

            // 3) Complete booking (يُنشئ الفاتورة داخلياً عبر EnsureInvoiceForBookingAsync)
            await _lifecycle.CompleteBookingAsync(booking.Id, request.CashierId);

            // 4) نكتفي بالفاتورة اللي اتعملت في Complete - نجيبها متتبعة عشان الـ Update ما يطلعش duplicate tracking
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();
            var bookingRepo = _uow.Repository<Booking>();
            var clientRepo = _uow.Repository<Client>();

            var existingInv = (await invRepo.FindAsync(x => x.BookingId == booking.Id)).FirstOrDefault();
            if (existingInv == null)
                throw new BusinessException("Invoice not found after completion", 404);

            // حمّل الفاتورة متتبعة (GetByIdAsync بدون AsNoTracking) عشان نقدّر نعدّلها ونعمل Update
            var invoice = await invRepo.GetByIdAsync(existingInv.Id);
            if (invoice == null)
                throw new BusinessException("Invoice not found after completion", 404);

            // ✅ Fix: store customer snapshot + المشرف على الفاتورة
            var bk = await bookingRepo.GetByIdAsync(booking.Id);
            if (bk != null)
            {
                var cl = await clientRepo.GetByIdAsync(bk.ClientId);
                if (cl != null)
                {
                    invoice.ClientId = cl.Id;
                    invoice.CustomerName = string.IsNullOrWhiteSpace(invoice.CustomerName) ? cl.FullName : invoice.CustomerName;
                    invoice.CustomerPhone = string.IsNullOrWhiteSpace(invoice.CustomerPhone) ? cl.PhoneNumber : invoice.CustomerPhone;
                }
            }
            if (request.SupervisorId.HasValue)
                invoice.SupervisorId = request.SupervisorId;
            invRepo.Update(invoice);

            // 5) Add products to same invoice (optional) + recalc totals from ALL lines
            if (request.Products != null && request.Products.Count > 0)
            {
                var products = request.Products
                    .Select(p => new PosProductItemDto { ProductId = p.ProductId, Qty = p.Qty })
                    .ToList();

                await AddProductsToInvoiceAsync(invoice, request.CashierId, products);
                // AddProductsToInvoiceAsync will SaveChanges and update invoice totals
            }
            else
            {
                // ✅ even لو مفيش منتجات: تأكد totals = sum(lines) + VAT
                await RecalcInvoiceFromAllLinesAsync(invoice.Id);
            }

            // 6) Pay cash immediately (Paid)
            await PayInvoiceCashAsync(invoice.Id, request.CashierId, DateTime.UtcNow);

            // 7) return fresh invoice + lines
            var fresh = await invRepo.GetByIdAsync(invoice.Id);
            if (fresh == null) throw new BusinessException("Invoice not found", 404);

            var lines = await lineRepo.FindAsync(l => l.InvoiceId == fresh.Id);
            fresh.Lines = lines.ToList();

            return MapInvoice(fresh);
        }


        


        // ----------------- helpers ----------------- //
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

        private async Task RequireSupervisorAsync(int supervisorId)
        {
            var empRepo = _uow.Repository<Employee>();
            var emp = await empRepo.GetByIdAsync(supervisorId);
            if (emp == null || !emp.IsActive)
                throw new BusinessException("Supervisor not found", 404);
            if (emp.Role != EmployeeRole.Supervisor)
                throw new BusinessException("Employee is not a supervisor", 403);
        }

        private class PosProductItemDto
        {
            public int ProductId { get; set; }
            public decimal Qty { get; set; }
        }

        private async Task AddProductsToInvoiceAsync(Invoice invoice, int cashierId, List<PosProductItemDto> products)
        {
            // invoice must be unpaid to add lines before payment
            if (invoice.Status == InvoiceStatus.Paid)
                throw new BusinessException("Invoice already paid", 409);

            var lineRepo = _uow.Repository<InvoiceLine>();
            var prodRepo = _uow.Repository<Product>();
            var stockRepo = _uow.Repository<BranchProductStock>();
            var moveRepo = _uow.Repository<ProductMovement>();
            var invRepo = _uow.Repository<Invoice>();

            var productIds = products.Select(x => x.ProductId).Distinct().ToList();
            var dbProducts = await prodRepo.FindAsync(p => productIds.Contains(p.Id) && p.IsActive);
            var pMap = dbProducts.ToDictionary(p => p.Id, p => p);

            var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == invoice.BranchId && productIds.Contains(s.ProductId));
            var sMap = stocks.ToDictionary(s => s.ProductId, s => s);

            foreach (var it in products)
            {
                if (!pMap.TryGetValue(it.ProductId, out var p))
                    throw new BusinessException($"Product {it.ProductId} not found", 404);

                if (!sMap.TryGetValue(it.ProductId, out var st))
                    throw new BusinessException("Product stock not found in this branch", 409);

                var available = st.OnHandQty - st.ReservedQty;
                if (available < 0) available = 0;

                if (available < it.Qty)
                    throw new BusinessException($"Not enough stock for product {it.ProductId}", 409);

                // stock
                st.OnHandQty -= it.Qty;
                stockRepo.Update(st);

                // invoice line
                var lineTotal = it.Qty * p.SalePrice;

                await lineRepo.AddAsync(new InvoiceLine
                {
                    InvoiceId = invoice.Id,
                    Description = $"Product: {p.Name}",
                    Qty = 1, // لو عايزة qty تظهر صح هنغير InvoiceLine.Qty لـ decimal بعدين
                    UnitPrice = p.SalePrice,
                    Total = lineTotal
                });

                // movement SELL
                await moveRepo.AddAsync(new ProductMovement
                {
                    BranchId = invoice.BranchId??0,
                    ProductId = p.Id,
                    MovementType = ProductMovementType.Sell,
                    Qty = it.Qty,
                    UnitCostSnapshot = p.CostPerUnit,
                    TotalCost = it.Qty * p.CostPerUnit,
                    OccurredAt = DateTime.UtcNow,
                    InvoiceId = invoice.Id,
                    BookingId = invoice.BookingId,
                    BookingItemId = null,
                    RecordedByEmployeeId = cashierId,
                    Notes = "Checkout add products"
                });
            }

            // ✅ totals MUST be recomputed from ALL invoice lines after adding products
            await _uow.SaveChangesAsync();

            await RecalcInvoiceFromAllLinesAsync(invoice.Id);
        }

        private async Task RecalcInvoiceFromAllLinesAsync(int invoiceId)
        {
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();

            var inv = await invRepo.GetByIdAsync(invoiceId);
            if (inv == null) return;

            if (inv.Status == InvoiceStatus.Paid)
                throw new BusinessException("Cannot change invoice after payment", 409);

            var allLines = await lineRepo.FindAsync(l => l.InvoiceId == invoiceId);
            var subTotal = allLines.Sum(l => l.Total);

            // VAT + totals
            RecalcInvoiceTotals(inv, subTotal);
            invRepo.Update(inv);

            await _uow.SaveChangesAsync();
        }

        private async Task PayInvoiceCashAsync(int invoiceId, int cashierId, DateTime paidAt)
        {
            var invRepo = _uow.Repository<Invoice>();
            var invoice = await invRepo.GetByIdAsync(invoiceId);
            if (invoice == null) throw new BusinessException("Invoice not found", 404);

            if (invoice.Status == InvoiceStatus.Paid) return;

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaymentMethod = PaymentMethod.Cash;
            invoice.PaidByEmployeeId = cashierId;
            invoice.PaidAt = paidAt;

            invRepo.Update(invoice);
            await _uow.SaveChangesAsync();
        }






        private const decimal DefaultVatRate = 0.14m;





        private static void RecalcInvoiceTotals(Invoice inv, decimal subTotal)
        {
            inv.SubTotal = Math.Round(subTotal, 2);
            inv.TaxRate = DefaultVatRate;
            inv.TaxAmount = Math.Round(inv.SubTotal * inv.TaxRate, 2);
            inv.Total = inv.SubTotal + inv.TaxAmount - inv.Discount;
            if (inv.Total < 0) inv.Total = 0;
        }

        // Map invoice entity -> response (use your existing Map if you have)
        private InvoiceResponse MapInvoice(Invoice inv)
        {
            return new InvoiceResponse
            {
                Id = inv.Id,
                InvoiceNumber = inv.InvoiceNumber,
                BookingId = inv.BookingId ?? 0,
                SubTotal = inv.SubTotal,
                Discount = inv.Discount,
                Total = inv.Total,
                Status = inv.Status,
                PaymentMethod = inv.PaymentMethod,
                PaidAt = inv.PaidAt,
                PaidByEmployeeId = inv.PaidByEmployeeId,
                SupervisorId = inv.SupervisorId,
                Lines = inv.Lines?.Select(l => new InvoiceLineResponse
                {
                    Id = l.Id,
                    Description = l.Description,
                    Qty = l.Qty,
                    UnitPrice = l.UnitPrice,
                    Total = l.Total
                }).ToList() ?? new List<InvoiceLineResponse>()
            };
        }





    }
}

