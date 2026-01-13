using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.Abstractions.Repositories;

namespace Forto.Application.Abstractions.Services.Bookings.Closing
{

    public class BookingClosingService : IBookingClosingService
    {
        private readonly IUnitOfWork _uow;

        public BookingClosingService(IUnitOfWork uow)
        {
            _uow = uow;
        }






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


        public async Task TryAutoCloseBookingAsync(int bookingId, bool save = true)
        {
            var bookingRepo = _uow.Repository<Domain.Entities.Bookings.Booking>();
            var itemRepo = _uow.Repository<Domain.Entities.Bookings.BookingItem>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return;

            if (booking.Status == Domain.Enum.BookingStatus.Completed || booking.Status == Domain.Enum.BookingStatus.Cancelled)
                return;

            // لازم tracking عشان تشوفي statuses اللي اتعدلت في نفس request
            var items = await itemRepo.FindTrackingAsync(i => i.BookingId == bookingId);
            if (items.Count == 0) return;

            // لازم مفيش Pending/InProgress
            var stillOpen = items.Any(i => i.Status == Domain.Enum.BookingItemStatus.Pending || i.Status == Domain.Enum.BookingItemStatus.InProgress);
            if (stillOpen) return;

            // كله Cancelled => booking Cancelled
            var allCancelled = items.All(i => i.Status == Domain.Enum.BookingItemStatus.Cancelled);
            if (allCancelled)
            {
                booking.Status = Domain.Enum.BookingStatus.Cancelled;
                booking.CompletedAt = DateTime.UtcNow;
                booking.TotalPrice = 0;
                booking.EstimatedDurationMinutes = 0;
                bookingRepo.Update(booking);

                if (save) await _uow.SaveChangesAsync();
                return;
            }

            // Done + Cancelled => Completed (لازم فيه Done واحدة على الأقل)
            var hasDone = items.Any(i => i.Status == Domain.Enum.BookingItemStatus.Done);
            var allDoneOrCancelled = items.All(i => i.Status == Domain.Enum.BookingItemStatus.Done || i.Status == Domain.Enum.BookingItemStatus.Cancelled);

            if (hasDone && allDoneOrCancelled)
            {
                booking.Status = Domain.Enum.BookingStatus.Completed;
                booking.CompletedAt = DateTime.UtcNow;
                bookingRepo.Update(booking);

                if (save) await _uow.SaveChangesAsync();
            }
        }
    
    
    
    
    }

}
