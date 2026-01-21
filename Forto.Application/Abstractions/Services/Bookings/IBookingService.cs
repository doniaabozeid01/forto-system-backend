using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Bookings
{
    public interface IBookingService
    {
        Task<AvailableSlotsResponse> GetAvailableSlotsAsync(int branchId, DateOnly date, int carId, List<int> serviceIds);

        Task<BookingResponse> CreateAsync(CreateBookingRequest request);
        Task<BookingResponse?> GetByIdAsync(int bookingId);

        Task<BookingItemResponse> StartItemAsync(int itemId, int employeeId);
        Task<BookingItemResponse> CompleteItemAsync(int itemId, int employeeId);

        Task<BookingResponse> QuickCreateAsync(QuickCreateBookingRequest request);

        // Admin
        //Task CancelBookingItemAsync(int itemId, CashierActionRequest request);
        //Task CancelBookingAsync(int bookingId, CashierActionRequest request);
        //Task CompleteBookingAsync(int bookingId, CashierActionRequest request); // manual complete
    }
}
