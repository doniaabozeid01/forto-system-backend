using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Bookings;

namespace Forto.Application.Abstractions.Services.Bookings.Admin
{
    public interface IBookingAdminService
    {
        Task CancelBookingItemAsync(int itemId, CashierActionRequest request);
        Task CancelBookingAsync(int bookingId, CashierActionRequest request);
        Task CompleteBookingAsync(int bookingId, CashierActionRequest request); // manual complete

        Task<BookingResponse> AssignEmployeesAsync(int bookingId, AssignBookingEmployeesRequest request);

    }

}
