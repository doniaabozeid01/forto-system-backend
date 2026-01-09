using Forto.Application.Abstractions.Services.Bookings;
using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Bookings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/booking-items")]
    public class BookingItemsController : BaseApiController
    {
        private readonly IBookingService _service;
        public BookingItemsController(IBookingService service) => _service = service;

        [HttpPut("{itemId:int}/start")]
        public async Task<IActionResult> Start(int itemId, [FromBody] StartBookingItemRequest request)
        {
            var data = await _service.StartItemAsync(itemId, request.EmployeeId);
            return OkResponse(data, "Item started");
        }

        [HttpPut("{itemId:int}/complete")]
        public async Task<IActionResult> Complete(int itemId, [FromBody] StartBookingItemRequest request)
        {
            var data = await _service.CompleteItemAsync(itemId, request.EmployeeId);
            return OkResponse(data, "Item completed");
        }











        [HttpPost("{itemId:int}/cancel")]
        public async Task<IActionResult> CancelItem(int itemId, [FromBody] CashierActionRequest request)
        {
            await _service.CancelBookingItemAsync(itemId, request);
            return OkResponse(new { itemId }, "Item cancelled");
        }
    }

}

