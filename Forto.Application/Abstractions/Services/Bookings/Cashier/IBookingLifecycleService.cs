using Forto.Application.DTOs.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Bookings.Cashier
{
    public interface IBookingLifecycleService
    {
        Task<BookingResponse> StartBookingAsync(int bookingId, int cashierId);
        Task<BookingResponse> CompleteBookingAsync(int bookingId, int cashierId);
    }
}
