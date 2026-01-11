using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.DTOs.Billings;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace Forto.Application.Abstractions.Services.Bookings.Admin
{
    public class BookingAdminService : IBookingAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly IInvoiceService _invoiceService;

        public BookingAdminService(IUnitOfWork uow, IInvoiceService invoiceService)
        {
            _uow = uow;
            _invoiceService = invoiceService;
        }


        // Admin

        private async Task<Employee> RequireCashierAsync(int cashierId)
        {
            var empRepo = _uow.Repository<Employee>();
            var cashier = await empRepo.GetByIdAsync(cashierId);
            if (cashier == null || !cashier.IsActive)
                throw new BusinessException("Cashier not found", 404);

            if (!(cashier.Role == EmployeeRole.Cashier || cashier.Role == EmployeeRole.Supervisor || cashier.Role == EmployeeRole.Admin))
                throw new BusinessException("Not allowed", 403);

            return cashier;
        }

        //public async Task CancelBookingItemAsync(int itemId, CashierActionRequest request)
        //{
        //    await RequireCashierAsync(request.CashierId);

        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null)
        //        throw new BusinessException("Booking item not found", 404);

        //    if (item.Status == BookingItemStatus.Done)
        //        throw new BusinessException("Cannot cancel a completed service (refund flow needed)", 409);

        //    var inv = await _invoiceService.GetByBookingIdAsync(item.BookingId);
        //    if (inv != null && inv.Status == InvoiceStatus.Paid)
        //        throw new BusinessException("Cannot cancel after payment (refund flow needed)", 409);

        //    // 1) Cancel item
        //    item.Status = BookingItemStatus.Cancelled;
        //    itemRepo.Update(item);

        //    // 2) Update booking totals (NO SaveChanges)
        //    await RecalculateBookingTotalsAsync(item.BookingId, save: false);

        //    // 3) Auto complete booking if needed (NO SaveChanges)
        //    await TryAutoCompleteBookingAsync(item.BookingId, save: false);

        //    // 4) Recalculate invoice (NO SaveChanges)
        //    await _invoiceService.RecalculateForBookingAsync(item.BookingId);

        //    // ✅ ONE SAVE
        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException(
        //            "This booking was modified by another operation. Please retry.",
        //            409
        //        );
        //    }
        //}

        //public async Task CancelBookingAsync(int bookingId, CashierActionRequest request)
        //{
        //    await RequireCashierAsync(request.CashierId);

        //    var bookingRepo = _uow.Repository<Booking>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var booking = await bookingRepo.GetByIdAsync(bookingId);
        //    if (booking == null) throw new BusinessException("Booking not found", 404);

        //    var inv = await _invoiceService.GetByBookingIdAsync(bookingId);
        //    if (inv != null && inv.Status == InvoiceStatus.Paid)
        //        throw new BusinessException("Cannot cancel after payment (refund flow needed)", 409);

        //    booking.Status = BookingStatus.Cancelled;
        //    bookingRepo.Update(booking);

        //    var items = await itemRepo.FindAsync(i => i.BookingId == bookingId);

        //    foreach (var it in items)
        //    {
        //        if (it.Status != BookingItemStatus.Done) // done لا نمسّه في MVP؟ الأفضل تلغيه؟ نخليه كما هو
        //        {
        //            it.Status = BookingItemStatus.Cancelled;
        //            itemRepo.Update(it);
        //        }
        //    }

        //    // totals = 0 (كل الخدمات تعتبر ملغاة)
        //    booking.TotalPrice = 0;
        //    booking.EstimatedDurationMinutes = 0;
        //    bookingRepo.Update(booking);

        //    await _uow.SaveChangesAsync();

        //    // invoice موجودة وغير مدفوعة؟ نعيد حسابها (هتبقى 0 lines)
        //    await _invoiceService.RecalculateForBookingAsync(bookingId);
        //}





        //public async Task CancelBookingItemAsync(int itemId , CashierActionRequest request)
        //{
        //    await RequireCashierAsync(request.CashierId);

        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var bookingRepo = _uow.Repository<Booking>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null)
        //        throw new BusinessException("Booking item not found", 404);

        //    if (item.Status == BookingItemStatus.Done)
        //        throw new BusinessException("Cannot cancel a completed service (refund flow needed)", 409);

        //    var inv = await _invoiceService.GetByBookingIdAsync(item.BookingId);
        //    if (inv != null && inv.Status == InvoiceStatus.Paid)
        //        throw new BusinessException("Cannot cancel after payment (refund flow needed)", 409);

        //    // 1) Cancel item
        //    item.Status = BookingItemStatus.Cancelled;
        //    itemRepo.Update(item);

        //    // 2) Update booking totals (NO SaveChanges)
        //    await RecalculateBookingTotalsAsync(item.BookingId, save: false);

        //    // 3) Auto close booking if needed (NO SaveChanges)
        //    await TryAutoCloseBookingAsync(item.BookingId, save: false);

        //    // اقرأ حالة الحجز بعد الإغلاق
        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);

        //    // 4) لو booking اتكنسل => لا فاتورة ولا recalculation
        //    if (booking != null && booking.Status != BookingStatus.Cancelled)
        //    {
        //        // ✅ invoice recalculation (NO save)
        //        await _invoiceService.RecalculateForBookingAsync(item.BookingId);
        //    }

        //    // ✅ ONE SAVE
        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("This booking was modified by another operation. Please retry.", 409);
        //    }

        //    // ✅ لو booking بقى Completed (مش Cancelled) و invoice مش موجودة، اعملها
        //    // (اختياري حسب flow عندك)
        //    if (booking != null && booking.Status == BookingStatus.Completed)
        //    {
        //        await _invoiceService.EnsureInvoiceForBookingAsync(item.BookingId);
        //    }
        //}


























        public async Task CancelBookingItemAsync(int itemId, CashierActionRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var itemRepo = _uow.Repository<BookingItem>();
            var bookingRepo = _uow.Repository<Booking>();

            var item = await itemRepo.GetByIdAsync(itemId);
            if (item == null)
                throw new BusinessException("Booking item not found", 404);

            if (item.Status == BookingItemStatus.Done)
                throw new BusinessException("Cannot cancel a completed service (refund flow needed)", 409);

            var inv = await _invoiceService.GetByBookingIdAsync(item.BookingId);
            if (inv != null && inv.Status == InvoiceStatus.Paid)
                throw new BusinessException("Cannot cancel after payment (refund flow needed)", 409);

            // 1) Cancel item
            item.Status = BookingItemStatus.Cancelled;
            itemRepo.Update(item);


            // 2) Update booking totals (NO SaveChanges)
            await RecalculateBookingTotalsAsync(item.BookingId, save: false);

            // 3) Auto close booking (NO SaveChanges)
            await TryAutoCloseBookingAsync(item.BookingId, save: false);

            // اقرأ حالة البوكينج بعد التعديلات (من نفس context)
            var booking = await bookingRepo.GetByIdAsync(item.BookingId);
            if (booking == null)
                throw new BusinessException("Booking not found", 404);

            // 4) invoice logic based on booking status
            if (booking.Status == BookingStatus.Cancelled)
            {
                // لا recalculation ولا ensure invoice
            }
            else
            {
                // لو عندك invoice موجودة Unpaid لازم تتعدل
                // مهم: خليه save:false عشان ONE SAVE
                await _invoiceService.RecalculateForBookingAsync(item.BookingId, save: false);
            }

            // ✅ ONE SAVE
            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException("This booking was modified by another operation. Please retry.", 409);
            }

            // بعد ما حفظنا، لو البوكينج Completed اعمل invoice (لو مش موجودة)
            if (booking.Status == BookingStatus.Completed)
            {
                await _invoiceService.EnsureInvoiceForBookingAsync(item.BookingId);
            }
        }


        public async Task CancelBookingAsync(int bookingId, CashierActionRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                throw new BusinessException("Booking not found", 404);

            var inv = await _invoiceService.GetByBookingIdAsync(bookingId);
            if (inv != null && inv.Status == InvoiceStatus.Paid)
                throw new BusinessException("Cannot cancel after payment (refund flow needed)", 409);

            // cancel booking
            booking.Status = BookingStatus.Cancelled;
            booking.CompletedAt = DateTime.UtcNow;

            booking.TotalPrice = 0;
            booking.EstimatedDurationMinutes = 0;

            bookingRepo.Update(booking);

            // cancel all items (حتى لو Done؟ في MVP: الأفضل ما نلمس Done، بس لو cancel booking بالكامل عادةً معناها مفيش تنفيذ)
            // بما إنك قلتي "ممكن حاجات تتكنسل عادي" وده قرار كاشير:
            // هنلغي كل اللي مش Done، وDone نسيبها (لتجنب refund flow).
            var items = await itemRepo.FindAsync(i => i.BookingId == bookingId);

            foreach (var it in items)
            {
                if (it.Status != BookingItemStatus.Done)
                {
                    it.Status = BookingItemStatus.Cancelled;
                    itemRepo.Update(it);
                }
            }

            await _uow.SaveChangesAsync();

            // ✅ مهم: لا تعمل Recalculate invoice هنا
            // ولو حابة تلغي invoice غير مدفوعة (اختياري)، نعمل method CancelInvoiceForBooking
        }



        public async Task CompleteBookingAsync(int bookingId, CashierActionRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            if (booking.Status == BookingStatus.Completed)
            {
                // ensure invoice
                await _invoiceService.EnsureInvoiceForBookingAsync(bookingId);
                return;
            }

            var items = await itemRepo.FindAsync(i => i.BookingId == bookingId);

            // سياسة أمان: ماينفعش نقفل وهو فيه Pending/InProgress
            var stillOpen = items.Any(i => i.Status == BookingItemStatus.Pending || i.Status == BookingItemStatus.InProgress);
            if (stillOpen)
                throw new BusinessException("Cannot complete booking while there are pending/in-progress services. Complete or cancel them first.", 409);

            // لو كله Done/Cancelled نقفل
            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = DateTime.UtcNow;
            bookingRepo.Update(booking);

            await _uow.SaveChangesAsync();

            await _invoiceService.EnsureInvoiceForBookingAsync(bookingId);
        }


        private async Task RecalculateBookingTotalsAsync(int bookingId, bool save = true)
        {
            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return;

            var items = await itemRepo.FindAsync(i =>
                i.BookingId == bookingId &&
                i.Status != BookingItemStatus.Cancelled);

            booking.TotalPrice = items.Sum(i => i.UnitPrice);
            booking.EstimatedDurationMinutes = items.Sum(i => i.DurationMinutes);

            bookingRepo.Update(booking);

            if (save)
                await _uow.SaveChangesAsync();
        }

        //private async Task TryAutoCompleteBookingAsync(int bookingId, bool save = true)
        //{
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var booking = await bookingRepo.GetByIdAsync(bookingId);
        //    if (booking == null) return;

        //    if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
        //        return;

        //    var items = await itemRepo.FindAsync(i => i.BookingId == bookingId);

        //    var canceled = items.All(i =>
        //        i.Status == BookingItemStatus.Cancelled);

        //    if (canceled)
        //    {
        //        booking.Status = BookingStatus.Completed;
        //        booking.CompletedAt = DateTime.UtcNow;
        //        bookingRepo.Update(booking);

        //    }

        //    var doneOrCancelled = items.All(i =>
        //        i.Status == BookingItemStatus.Done ||
        //        i.Status == BookingItemStatus.Cancelled);

        //    if (!doneOrCancelled) return;

        //    booking.Status = BookingStatus.Completed;
        //    booking.CompletedAt = DateTime.UtcNow;
        //    bookingRepo.Update(booking);

        //    if (save)
        //        await _uow.SaveChangesAsync();
        //}


        private async Task TryAutoCloseBookingAsync(int bookingId, bool save = true)
        {
            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return;

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                return;

            //var items = await itemRepo.FindAsync(i => i.BookingId == bookingId);
            var items = await itemRepo.FindTrackingAsync(i => i.BookingId == bookingId);

            if (items.Count == 0) return;

            // لازم يكون مفيش Pending/InProgress
            var stillOpen = items.Any(i => i.Status == BookingItemStatus.Pending || i.Status == BookingItemStatus.InProgress);
            if (stillOpen) return;

            var allCancelled = items.All(i => i.Status == BookingItemStatus.Cancelled);
            if (allCancelled)
            {
                booking.Status = BookingStatus.Cancelled;
                booking.CompletedAt = DateTime.UtcNow; // أو خليها CancelledAt لو عندك
                bookingRepo.Update(booking);

                // totals = 0
                booking.TotalPrice = 0;
                booking.EstimatedDurationMinutes = 0;
                bookingRepo.Update(booking);

                if (save) await _uow.SaveChangesAsync();
                return;
            }

            // هنا معناها: فيه Done (والباقي Cancelled) => Completed
            var allDoneOrCancelled = items.All(i => i.Status == BookingItemStatus.Done || i.Status == BookingItemStatus.Cancelled);
            if (!allDoneOrCancelled) return;

            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = DateTime.UtcNow;
            bookingRepo.Update(booking);

            if (save) await _uow.SaveChangesAsync();
        }


    }

}
