using Forto.Application.Abstractions.Services.Bookings;
using Forto.Application.Abstractions.Services.Bookings.Admin;
using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Bookings;
using Forto.Application.DTOs.Inventory.Materials;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/booking-items")]
    public class BookingItemsController : BaseApiController
    {
        private readonly IBookingService _service;
        private readonly IBookingAdminService _adminService;

        public BookingItemsController(IBookingService service, IBookingAdminService adminService)
        {
            _service = service;
            _adminService = adminService;
        }

        //[HttpPut("{itemId:int}/start")]
        //public async Task<IActionResult> Start(int itemId, [FromBody] StartBookingItemRequest request)
        //{
        //    var data = await _service.StartItemByCashierAsync(itemId, request.CashierId);
        //    return OkResponse(data, "Item started");
        //}





        //[HttpPut("{itemId:int}/complete")]
        //public async Task<IActionResult> Complete(int itemId, [FromBody] StartBookingItemRequest request)
        //{
        //    var data = await _service.CompleteItemByCashierAsync(itemId, request.CashierId);
        //    return OkResponse(data, "Item completed");
        //}





        [HttpPut("{bookingItemId:int}/materials/by-cashier")]
        public async Task<IActionResult> UpdateMaterialsByCashier(
    int bookingItemId,
    [FromBody] UpdateBookingItemMaterialsByCashierRequest request)
        {
            var data = await _service.UpdateActualByCashierAsync(bookingItemId, request);
            return OkResponse(data, "Materials updated");
        }





        [HttpPost("{itemId:int}/cancel")]
        public async Task<IActionResult> CancelItem(int itemId, [FromBody] CashierActionRequest request)
        {
            await _adminService.CancelBookingItemAsync(itemId, request);
            return OkResponse(new { itemId }, "Item cancelled");
        }
    }

}

