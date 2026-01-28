using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Ops.Usage;

namespace Forto.Application.Abstractions.Services.Ops.Usage
{
    public interface IBookingItemMaterialsService
    {
        //Task<BookingItemMaterialsResponse> GetAsync(int bookingItemId, int employeeId);
        Task<BookingItemMaterialsResponse> GetAsync(int bookingItemId);
        Task<BookingItemMaterialsResponse> UpdateActualAsync(int bookingItemId, UpdateBookingItemMaterialsRequest request);
    }
}
