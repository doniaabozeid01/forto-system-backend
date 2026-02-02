using Forto.Application.DTOs.Bookings.cashier;
using Forto.Application.DTOs.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Bookings.Cashier
{
    public interface IBookingItemOpsService
    {
        Task<BookingResponse> AddServiceAsync(int bookingId, AddServiceToBookingRequest request);
        Task<BookingResponse> AddServicesAsync(int bookingId, AddServicesToBookingRequest request);
        Task<BookingResponse> CancelServiceAsync(int bookingItemId, CancelBookingItemByCashierRequest request);
    }
}
