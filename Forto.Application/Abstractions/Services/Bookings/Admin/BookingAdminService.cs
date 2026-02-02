using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.Abstractions.Services.Bookings.Closing;
using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.Abstractions.Services.Schedule;
using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Bookings;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace Forto.Application.Abstractions.Services.Bookings.Admin
{
    public class BookingAdminService : IBookingAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly IInvoiceService _invoiceService;
        private readonly IBookingService _bookingService;
        private readonly IBookingClosingService _closingService;
        private readonly IEmployeeScheduleService _scheduleService;

        public BookingAdminService(IUnitOfWork uow, IInvoiceService invoiceService,IBookingService bookingService , IBookingClosingService closingService, IEmployeeScheduleService scheduleService)
        {
            _uow = uow;
            _invoiceService = invoiceService;
            _bookingService = bookingService;
            _closingService = closingService;
            _scheduleService = scheduleService;
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





        // Before Material
        //public async Task CancelBookingItemAsync(int itemId, CashierActionRequest request)
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

        //    // 3) Auto close booking (NO SaveChanges)
        //    await _closingService.TryAutoCloseBookingAsync(item.BookingId, save: false);

        //    // اقرأ حالة البوكينج بعد التعديلات (من نفس context)
        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (booking == null)
        //        throw new BusinessException("Booking not found", 404);

        //    // 4) invoice logic based on booking status
        //    if (booking.Status == BookingStatus.Cancelled)
        //    {
        //        // لا recalculation ولا ensure invoice
        //    }
        //    else
        //    {
        //        // لو عندك invoice موجودة Unpaid لازم تتعدل
        //        // مهم: خليه save:false عشان ONE SAVE
        //        await _invoiceService.RecalculateForBookingAsync(item.BookingId, save: false);
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

        //    // بعد ما حفظنا، لو البوكينج Completed اعمل invoice (لو مش موجودة)
        //    if (booking.Status == BookingStatus.Completed)
        //    {
        //        await _invoiceService.EnsureInvoiceForBookingAsync(item.BookingId);
        //    }
        //}












        // before material movements
        //public async Task CancelBookingItemAsync(int itemId, CashierActionRequest request)
        //{
        //    await RequireCashierAsync(request.CashierId);

        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
        //    var stockRepo = _uow.Repository<BranchMaterialStock>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null)
        //        throw new BusinessException("Booking item not found", 404);

        //    if (item.Status == BookingItemStatus.Done)
        //        throw new BusinessException("Cannot cancel a completed service (refund flow needed)", 409);

        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (booking == null)
        //        throw new BusinessException("Booking not found", 404);

        //    // لو الفاتورة مدفوعة نمنع الإلغاء
        //    var inv = await _invoiceService.GetByBookingIdAsync(item.BookingId);
        //    if (inv != null && inv.Status == InvoiceStatus.Paid)
        //        throw new BusinessException("Cannot cancel after payment (refund flow needed)", 409);

        //    // ========= CASE 1: Pending =========
        //    if (item.Status == BookingItemStatus.Pending)
        //    {
        //        item.Status = BookingItemStatus.Cancelled;
        //        itemRepo.Update(item);

        //        await _closingService.TryAutoCloseBookingAsync(item.BookingId, save: false);

        //        await _uow.SaveChangesAsync();
        //        return;
        //    }

        //    // ========= CASE 2: InProgress =========

        //    // load usages tracking
        //    var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == item.Id);
        //    if (usages.Count == 0)
        //        throw new BusinessException("No material usage found for this item", 409);

        //    // load branch stocks tracking
        //    var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();
        //    var stocks = await stockRepo.FindTrackingAsync(s =>
        //        s.BranchId == booking.BranchId && materialIds.Contains(s.MaterialId));

        //    var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

        //    // map overrides (لو الكاشير دخل أرقام)
        //    var overrideMap = request.UsedOverride?
        //        .ToDictionary(x => x.MaterialId, x => x.ActualQty)
        //        ?? new Dictionary<int, decimal>();

        //    foreach (var usage in usages)
        //    {
        //        if (!stockMap.TryGetValue(usage.MaterialId, out var stock))
        //            throw new BusinessException("Stock row missing for a material in this branch", 409);

        //        // actual used = override OR current actual
        //        var actualUsed = overrideMap.TryGetValue(usage.MaterialId, out var ov)
        //            ? Math.Max(0, ov)
        //            : usage.ActualQty;

        //        // ==== خصم OnHand (Waste) ====
        //        stock.OnHandQty -= actualUsed;
        //        if (stock.OnHandQty < 0) stock.OnHandQty = 0;

        //        // ==== فك الـ Reservation بالكامل ====
        //        stock.ReservedQty -= usage.ReservedQty;
        //        if (stock.ReservedQty < 0) stock.ReservedQty = 0;

        //        stockRepo.Update(stock);

        //        // update usage
        //        usage.ActualQty = actualUsed;
        //        usage.ReservedQty = 0;
        //        usage.ExtraCharge = 0; // لا نحاسب العميل
        //        usage.RecordedByEmployeeId = request.CashierId;
        //        usage.RecordedAt = DateTime.UtcNow;

        //        usageRepo.Update(usage);
        //    }

        //    // mark item cancelled
        //    item.Status = BookingItemStatus.Cancelled;
        //    item.MaterialAdjustment = 0; // ما فيش تحميل على العميل
        //    itemRepo.Update(item);

        //    // auto close booking (cancel / complete)
        //    await _closingService.TryAutoCloseBookingAsync(item.BookingId, save: false);

        //    // ========= ONE SAVE =========
        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("This booking was modified by another operation. Please retry.", 409);
        //    }
        //}

        public async Task CancelBookingItemAsync(int itemId, CashierActionRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var itemRepo = _uow.Repository<BookingItem>();
            var bookingRepo = _uow.Repository<Booking>();
            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var movementRepo = _uow.Repository<MaterialMovement>();

            var item = await itemRepo.GetByIdAsync(itemId);
            if (item == null)
                throw new BusinessException("Booking item not found", 404);

            if (item.Status == BookingItemStatus.Done)
                throw new BusinessException("Cannot cancel a completed service (refund flow needed)", 409);

            var booking = await bookingRepo.GetByIdAsync(item.BookingId);
            if (booking == null)
                throw new BusinessException("Booking not found", 404);

            var inv = await _invoiceService.GetByBookingIdAsync(item.BookingId);
            if (inv != null && inv.Status == InvoiceStatus.Paid)
                throw new BusinessException("Cannot cancel after payment (refund flow needed)", 409);

            // ========= CASE 1: Pending =========
            if (item.Status == BookingItemStatus.Pending)
            {
                item.Status = BookingItemStatus.Cancelled;
                itemRepo.Update(item);

                await _closingService.TryAutoCloseBookingAsync(item.BookingId, save: false);

                await _uow.SaveChangesAsync();
                return;
            }

            // ========= CASE 2: InProgress =========
            var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == item.Id);
            if (usages.Count == 0)
                throw new BusinessException("No material usage found for this item", 409);

            var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();

            var stocks = await stockRepo.FindTrackingAsync(s =>
                s.BranchId == booking.BranchId && materialIds.Contains(s.MaterialId));

            var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

            // ✅ Idempotency: لو waste اتسجل قبل كده لنفس item/material متسجلش تاني
            var existingWaste = await movementRepo.FindAsync(m =>
                m.BookingItemId == item.Id &&
                m.MovementType == MaterialMovementType.Waste);

            var alreadyLogged = existingWaste.Select(m => m.MaterialId).ToHashSet();

            // overrides
            var overrideMap = request.UsedOverride?
                .ToDictionary(x => x.MaterialId, x => x.ActualQty)
                ?? new Dictionary<int, decimal>();

            var occurredAt = DateTime.UtcNow;

            foreach (var usage in usages)
            {
                if (!stockMap.TryGetValue(usage.MaterialId, out var stock))
                    throw new BusinessException("Stock row missing for a material in this branch", 409);

                var actualUsed = overrideMap.TryGetValue(usage.MaterialId, out var ov)
                    ? Math.Max(0, ov)
                    : usage.ActualQty;

                // ==== خصم OnHand (Waste) ====
                stock.OnHandQty -= actualUsed;
                if (stock.OnHandQty < 0) stock.OnHandQty = 0;

                // ==== فك الـ Reservation بالكامل ====
                stock.ReservedQty -= usage.ReservedQty;
                if (stock.ReservedQty < 0) stock.ReservedQty = 0;

                stockRepo.Update(stock);

                // update usage (freeze it)
                usage.ActualQty = actualUsed;
                usage.ReservedQty = 0;
                usage.ExtraCharge = 0; // لا نحاسب العميل
                usage.RecordedByEmployeeId = request.CashierId;
                usage.RecordedAt = occurredAt;
                usageRepo.Update(usage);

                // ✅ سجل Waste movement (مرة واحدة فقط)
                if (!alreadyLogged.Contains(usage.MaterialId) && actualUsed > 0)
                {
                    await movementRepo.AddAsync(new MaterialMovement
                    {
                        BranchId = booking.BranchId,
                        MaterialId = usage.MaterialId,
                        MovementType = MaterialMovementType.Waste,
                        Qty = actualUsed,
                        UnitCostSnapshot = usage.UnitCost,
                        TotalCost = actualUsed * usage.UnitCost,
                        OccurredAt = occurredAt,
                        BookingId = booking.Id,
                        BookingItemId = item.Id,
                        //RecordedByEmployeeId = request.CashierId,
                        Notes = request.Reason
                    });
                }
            }

            // mark item cancelled
            item.Status = BookingItemStatus.Cancelled;
            item.MaterialAdjustment = 0;
            itemRepo.Update(item);

            await _closingService.TryAutoCloseBookingAsync(item.BookingId, save: false);

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException("This booking was modified by another operation. Please retry.", 409);
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
                // already completed, nothing to cancel (invoice was created on complete)
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

        //private async Task RecalculateBookingTotalsAsync(int bookingId, bool save = true)
        //{
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var booking = await bookingRepo.GetByIdAsync(bookingId);
        //    if (booking == null) return;

        //    var items = await itemRepo.FindAsync(i =>
        //        i.BookingId == bookingId &&
        //        i.Status != BookingItemStatus.Cancelled);

        //    booking.TotalPrice = items.Sum(i => i.UnitPrice);
        //    booking.EstimatedDurationMinutes = items.Sum(i => i.DurationMinutes);

        //    bookingRepo.Update(booking);

        //    if (save)
        //        await _uow.SaveChangesAsync();
        //}

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


        //private async Task TryAutoCloseBookingAsync(int bookingId, bool save = true)
        //{
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var booking = await bookingRepo.GetByIdAsync(bookingId);
        //    if (booking == null) return;

        //    if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
        //        return;

        //    //var items = await itemRepo.FindAsync(i => i.BookingId == bookingId);
        //    var items = await itemRepo.FindTrackingAsync(i => i.BookingId == bookingId);

        //    if (items.Count == 0) return;

        //    // لازم يكون مفيش Pending/InProgress
        //    var stillOpen = items.Any(i => i.Status == BookingItemStatus.Pending || i.Status == BookingItemStatus.InProgress);
        //    if (stillOpen) return;

        //    var allCancelled = items.All(i => i.Status == BookingItemStatus.Cancelled);
        //    if (allCancelled)
        //    {
        //        booking.Status = BookingStatus.Cancelled;
        //        booking.CompletedAt = DateTime.UtcNow; // أو خليها CancelledAt لو عندك
        //        bookingRepo.Update(booking);

        //        // totals = 0
        //        booking.TotalPrice = 0;
        //        booking.EstimatedDurationMinutes = 0;
        //        bookingRepo.Update(booking);

        //        if (save) await _uow.SaveChangesAsync();
        //        return;
        //    }

        //    // هنا معناها: فيه Done (والباقي Cancelled) => Completed
        //    var allDoneOrCancelled = items.All(i => i.Status == BookingItemStatus.Done || i.Status == BookingItemStatus.Cancelled);
        //    if (!allDoneOrCancelled) return;

        //    booking.Status = BookingStatus.Completed;
        //    booking.CompletedAt = DateTime.UtcNow;
        //    bookingRepo.Update(booking);

        //    if (save) await _uow.SaveChangesAsync();
        //}

        public async Task<BookingResponse> AssignEmployeesAsync(int bookingId, AssignBookingEmployeesRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();
            var empRepo = _uow.Repository<Employee>();
            var empServiceRepo = _uow.Repository<EmployeeService>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                throw new BusinessException("Cannot assign employees for a closed booking", 409);

            if (request.Assignments == null || request.Assignments.Count == 0)
                throw new BusinessException("Assignments is required", 400);

            // load booking items tracking
            var items = await itemRepo.FindTrackingAsync(i => i.BookingId == bookingId);
            var itemMap = items.ToDictionary(i => i.Id, i => i);

            // validate bookingItemIds
            var badItemIds = request.Assignments
                .Select(a => a.BookingItemId)
                .Distinct()
                .Where(id => !itemMap.ContainsKey(id))
                .ToList();

            if (badItemIds.Any())
                throw new BusinessException("Some booking items do not belong to this booking", 400,
                    new Dictionary<string, string[]>
                    {
                        ["bookingItemId"] = badItemIds.Select(x => x.ToString()).ToArray()
                    });

            foreach (var a in request.Assignments)
            {
                var item = itemMap[a.BookingItemId];

                // do not assign done/cancelled
                if (item.Status == BookingItemStatus.Done || item.Status == BookingItemStatus.Cancelled)
                    throw new BusinessException($"Cannot assign for item {item.Id} in status {item.Status}", 409);

                var emp = await empRepo.GetByIdAsync(a.EmployeeId);
                if (emp == null || !emp.IsActive)
                    throw new BusinessException($"Employee {a.EmployeeId} not found", 404);

                // working check at booking time
                var isWorking = await _scheduleService.IsEmployeeWorkingAsync(a.EmployeeId, booking.ScheduledStart);
                if (!isWorking)
                    throw new BusinessException($"Employee {a.EmployeeId} is not working at this time", 409);

                // qualification check
                var qualified = await empServiceRepo.AnyAsync(es =>
                    es.EmployeeId == a.EmployeeId &&
                    es.ServiceId == item.ServiceId &&
                    es.IsActive);

                if (!qualified)
                    throw new BusinessException($"Employee {a.EmployeeId} is not qualified for service {item.ServiceId}", 409);

                // assign
                item.AssignedEmployeeId = a.EmployeeId;
                itemRepo.Update(item);
            }

            await _uow.SaveChangesAsync();

            // return fresh booking with items (recommended)
            var refreshed = await _bookingService.GetByIdAsync(bookingId);
            if (refreshed == null) throw new BusinessException("Booking not found", 404);
            return refreshed;

        }

    }

}
