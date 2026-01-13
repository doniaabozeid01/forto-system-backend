using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Billings;
using Forto.Domain.Entities.Billings;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Enum;

namespace Forto.Application.Abstractions.Services.Invoices
{
    public class InvoiceService : IInvoiceService
    {
                
        private readonly IUnitOfWork _uow;


        public InvoiceService(IUnitOfWork uow) => _uow = uow;


        public async Task<InvoiceResponse?> GetByBookingIdAsync(int bookingId)
        {
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();

            var invList = await invRepo.FindAsync(x => x.BookingId == bookingId);
            var inv = invList.FirstOrDefault();
            if (inv == null) return null;

            var lines = await lineRepo.FindAsync(l => l.InvoiceId == inv.Id);
            inv.Lines = lines.ToList();

            return Map(inv);
        }


        //public async Task<InvoiceResponse> EnsureInvoiceForBookingAsync(int bookingId)
        //{
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var invRepo = _uow.Repository<Invoice>();
        //    var lineRepo = _uow.Repository<InvoiceLine>();
        //    var itemRepo = _uow.Repository<BookingItem>();


        //    var booking = await bookingRepo.GetByIdAsync(bookingId);
        //    if (booking == null)
        //        throw new BusinessException("Booking not found", 404);

        //    if (booking.Status == BookingStatus.Cancelled)
        //        throw new BusinessException("Cannot create invoice for a cancelled booking", 409);


        //    // لو الفاتورة موجودة خلاص
        //    var existing = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
        //    if (existing != null)
        //    {
        //        var exLines = await lineRepo.FindAsync(l => l.InvoiceId == existing.Id);
        //        existing.Lines = exLines.ToList();
        //        return Map(existing);
        //    }

        //    // هات items غير cancelled فقط (تشكل الفاتورة)
        //    var items = await itemRepo.FindAsync(i =>
        //        i.BookingId == bookingId &&
        //        i.Status != BookingItemStatus.Cancelled);

        //    var subTotal = items.Sum(i => i.UnitPrice);
        //    var invoice = new Invoice
        //    {
        //        BookingId = bookingId,
        //        SubTotal = subTotal,
        //        Discount = 0,
        //        Total = subTotal,
        //        Status = InvoiceStatus.Unpaid
        //    };

        //    await invRepo.AddAsync(invoice);
        //    await _uow.SaveChangesAsync(); // عشان ناخد invoice.Id

        //    // Lines: سطر لكل خدمة (أوضح من سطر واحد)
        //    // لو عايزة سطر واحد فقط، قولي وأنا أعدله
        //    foreach (var it in items)
        //    {
        //        await lineRepo.AddAsync(new InvoiceLine
        //        {
        //            InvoiceId = invoice.Id,
        //            Description = $"Service #{it.ServiceId}",
        //            Qty = 1,
        //            UnitPrice = it.UnitPrice,
        //            Total = it.UnitPrice
        //        });
        //    }

        //    await _uow.SaveChangesAsync();

        //    var linesAfter = await lineRepo.FindAsync(l => l.InvoiceId == invoice.Id);
        //    invoice.Lines = linesAfter.ToList();

        //    return Map(invoice);
        //}


        public async Task<InvoiceResponse> EnsureInvoiceForBookingAsync(int bookingId)
        {
            var bookingRepo = _uow.Repository<Booking>();
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();
            var itemRepo = _uow.Repository<BookingItem>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                throw new BusinessException("Booking not found", 404);

            if (booking.Status == BookingStatus.Cancelled)
                throw new BusinessException("Cannot create invoice for a cancelled booking", 409);

            // لو الفاتورة موجودة خلاص
            var existing = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
            if (existing != null)
            {
                var exLines = await lineRepo.FindAsync(l => l.InvoiceId == existing.Id);
                existing.Lines = exLines.ToList();
                return Map(existing);
            }

            // items غير cancelled فقط
            var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

            // ✅ totals based on (UnitPrice + MaterialAdjustment)
            var subTotal = items.Sum(it =>
            {
                var t = it.UnitPrice + it.MaterialAdjustment;
                return t < 0 ? 0 : t;
            });

            var invoice = new Invoice
            {
                BookingId = bookingId,
                SubTotal = subTotal,
                Discount = 0,
                Total = subTotal,
                Status = InvoiceStatus.Unpaid
            };

            await invRepo.AddAsync(invoice);
            await _uow.SaveChangesAsync(); // لازم عشان invoice.Id

            foreach (var it in items)
            {
                var lineTotal = it.UnitPrice + it.MaterialAdjustment;
                if (lineTotal < 0) lineTotal = 0;

                await lineRepo.AddAsync(new InvoiceLine
                {
                    InvoiceId = invoice.Id,
                    Description = $"Service #{it.ServiceId} #{it.Service?.Name}",
                    Qty = 1,
                    UnitPrice = it.UnitPrice,
                    Total = lineTotal
                });
            }

            await _uow.SaveChangesAsync();

            var linesAfter = await lineRepo.FindAsync(l => l.InvoiceId == invoice.Id);
            invoice.Lines = linesAfter.ToList();

            return Map(invoice);
        }


        //public async Task RecalculateForBookingAsync(int bookingId)
        //{
        //    var invRepo = _uow.Repository<Invoice>();
        //    var lineRepo = _uow.Repository<InvoiceLine>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var inv = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
        //    if (inv == null) return;

        //    if (inv.Status == InvoiceStatus.Paid)
        //        throw new BusinessException("Cannot change invoice after payment (refund flow needed)", 409);

        //    // rebuild lines based on items (not cancelled)
        //    var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

        //    // delete existing lines (soft delete عندك؟ هنا هنستخدم delete العادي عبر repo)
        //    var oldLines = await lineRepo.FindAsync(l => l.InvoiceId == inv.Id);
        //    foreach (var l in oldLines)
        //        lineRepo.Delete(l);

        //    await _uow.SaveChangesAsync();

        //    foreach (var it in items)
        //    {
        //        await lineRepo.AddAsync(new InvoiceLine
        //        {
        //            InvoiceId = inv.Id,
        //            Description = $"Service #{it.ServiceId}",
        //            Qty = 1,
        //            UnitPrice = it.UnitPrice,
        //            Total = it.UnitPrice
        //        });
        //    }

        //    inv.SubTotal = items.Sum(i => i.UnitPrice);
        //    inv.Total = inv.SubTotal - inv.Discount;

        //    invRepo.Update(inv);
        //    await _uow.SaveChangesAsync();
        //}



        //public async Task RecalculateForBookingAsync(int bookingId)
        //{
        //    var invRepo = _uow.Repository<Invoice>();
        //    var lineRepo = _uow.Repository<InvoiceLine>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var inv = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
        //    if (inv == null) return;

        //    if (inv.Status == InvoiceStatus.Paid)
        //        throw new BusinessException("Cannot change invoice after payment (refund flow needed)", 409);

        //    // rebuild lines based on items (not cancelled)
        //    var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

        //    // delete existing lines (soft delete عندك؟ هنا هنستخدم delete العادي عبر repo)
        //    var oldLines = await lineRepo.FindAsync(l => l.InvoiceId == inv.Id);
        //    foreach (var l in oldLines)
        //        lineRepo.Delete(l);

        //    await _uow.SaveChangesAsync();

        //    foreach (var it in items)
        //    {
        //        await lineRepo.AddAsync(new InvoiceLine
        //        {
        //            InvoiceId = inv.Id,
        //            Description = $"Service #{it.ServiceId}",
        //            Qty = 1,
        //            UnitPrice = it.UnitPrice,
        //            Total = it.UnitPrice
        //        });
        //    }

        //    inv.SubTotal = items.Sum(i => i.UnitPrice);
        //    inv.Total = inv.SubTotal - inv.Discount;

        //    invRepo.Update(inv);
        //    await _uow.SaveChangesAsync();
        //}






        //public async Task RecalculateForBookingAsync(int bookingId, bool save = true)
        //{
        //    var invRepo = _uow.Repository<Invoice>();
        //    var lineRepo = _uow.Repository<InvoiceLine>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var inv = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
        //    if (inv == null) return;

        //    if (inv.Status == InvoiceStatus.Paid)
        //        throw new BusinessException("Cannot change invoice after payment (refund flow needed)", 409);

        //    // items not cancelled
        //    var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

        //    // delete old lines (no SaveChanges here)
        //    var oldLines = await lineRepo.FindAsync(l => l.InvoiceId == inv.Id);
        //    foreach (var l in oldLines)
        //        lineRepo.Delete(l);

        //    // add new lines
        //    foreach (var it in items)
        //    {
        //        await lineRepo.AddAsync(new InvoiceLine
        //        {
        //            InvoiceId = inv.Id,
        //            Description = $"Service #{it.ServiceId}",
        //            Qty = 1,
        //            UnitPrice = it.UnitPrice,
        //            Total = it.UnitPrice
        //        });
        //    }

        //    // update invoice totals
        //    inv.SubTotal = items.Sum(i => i.UnitPrice);
        //    inv.Total = inv.SubTotal - inv.Discount;

        //    invRepo.Update(inv);

        //    // ✅ single save
        //    //try
        //    //{
        //    //    await _uow.SaveChangesAsync();
        //    //}
        //    //catch (DbUpdateConcurrencyException)
        //    //{
        //    //    // conflict -> يرجع 409 بدل 500
        //    //    throw new BusinessException("Invoice was updated by another operation. Please retry.", 409);
        //    //}


        //    if (!save) return;

        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("Invoice was updated by another operation. Please retry.", 409);
        //    }


        //}


        public async Task RecalculateForBookingAsync(int bookingId, bool save = true)
        {
            var invRepo = _uow.Repository<Invoice>();
            var lineRepo = _uow.Repository<InvoiceLine>();
            var itemRepo = _uow.Repository<BookingItem>();

            var inv = (await invRepo.FindAsync(x => x.BookingId == bookingId)).FirstOrDefault();
            if (inv == null) return;

            if (inv.Status == InvoiceStatus.Paid)
                throw new BusinessException("Cannot change invoice after payment (refund flow needed)", 409);

            // items not cancelled
            var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

            // delete old lines
            var oldLines = await lineRepo.FindAsync(l => l.InvoiceId == inv.Id);
            foreach (var l in oldLines)
                lineRepo.Delete(l);

            // rebuild lines using (UnitPrice + MaterialAdjustment)
            foreach (var it in items)
            {
                var lineTotal = it.UnitPrice + it.MaterialAdjustment;
                if (lineTotal < 0) lineTotal = 0;

                await lineRepo.AddAsync(new InvoiceLine
                {
                    InvoiceId = inv.Id,
                    Description = $"Service #{it.ServiceId}",
                    Qty = 1,
                    UnitPrice = it.UnitPrice,   // base price (optional keep)
                    Total = lineTotal           // final after materials
                });
            }

            // totals
            var subTotal = items.Sum(it =>
            {
                var t = it.UnitPrice + it.MaterialAdjustment;
                return t < 0 ? 0 : t;
            });

            inv.SubTotal = subTotal;
            inv.Total = inv.SubTotal - inv.Discount;

            invRepo.Update(inv);

            if (!save) return;

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new BusinessException("Invoice was updated by another operation. Please retry.", 409);
            }
        }


        public async Task<InvoiceResponse> PayCashAsync(int invoiceId, int cashierId)
        {
            // check cashier role
            var empRepo = _uow.Repository<Employee>();
            var cashier = await empRepo.GetByIdAsync(cashierId);
            if (cashier == null || !cashier.IsActive)
                throw new BusinessException("Cashier not found", 404);

            if (!(cashier.Role == EmployeeRole.Cashier || cashier.Role == EmployeeRole.Supervisor || cashier.Role == EmployeeRole.Admin))
                throw new BusinessException("Not allowed", 403);

            var invRepo = _uow.Repository<Invoice>();
            var inv = await invRepo.GetByIdAsync(invoiceId);
            if (inv == null)
                throw new BusinessException("Invoice not found", 404);

            if (inv.Status == InvoiceStatus.Paid)
                throw new BusinessException("Invoice already paid", 409);

            if (inv.Status == InvoiceStatus.Cancelled)
                throw new BusinessException("Invoice is cancelled", 409);

            inv.Status = InvoiceStatus.Paid;
            inv.PaymentMethod = PaymentMethod.Cash;
            inv.PaidByEmployeeId = cashierId;
            inv.PaidAt = DateTime.UtcNow;

            invRepo.Update(inv);
            await _uow.SaveChangesAsync();

            // load lines
            var lines = await _uow.Repository<InvoiceLine>().FindAsync(l => l.InvoiceId == inv.Id);
            inv.Lines = lines.ToList();

            return Map(inv);
        }


        private static InvoiceResponse Map(Invoice inv) => new()
        {
            Id = inv.Id,
            BookingId = inv.BookingId,
            SubTotal = inv.SubTotal,
            Discount = inv.Discount,
            Total = inv.Total,
            Status = inv.Status,
            PaidByEmployeeId = inv.PaidByEmployeeId,
            PaidAt = inv.PaidAt,
            PaymentMethod = inv.PaymentMethod,
            Lines = inv.Lines.Select(l => new InvoiceLineResponse
            {
                Id = l.Id,
                Description = l.Description,
                Qty = l.Qty,
                UnitPrice = l.UnitPrice,
                Total = l.Total
            }).ToList()
        };
    
        
    }
}
