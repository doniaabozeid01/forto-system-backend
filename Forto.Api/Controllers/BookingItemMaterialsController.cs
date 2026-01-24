using Forto.Application.Abstractions.Services.Ops.Usage;
using Forto.Application.DTOs.Ops.Usage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/booking-items/{bookingItemId:int}/materials")]
    public class BookingItemMaterialsController : BaseApiController
    {
        private readonly IBookingItemMaterialsService _service;

        public BookingItemMaterialsController(IBookingItemMaterialsService service)
            => _service = service;

        [HttpGet("Get")]
        public async Task<IActionResult> Get(int bookingItemId, [FromQuery] int employeeId)
        {
            var data = await _service.GetAsync(bookingItemId, employeeId);
            return OkResponse(data, "OK");
        }



        //[HttpPut("UpdateBookingItemMaterials")]
        //public async Task<IActionResult> Update(int bookingItemId, [FromBody] UpdateBookingItemMaterialsRequest request)
        //{
        //    var data = await _service.UpdateActualAsync(bookingItemId, request);
        //    return OkResponse(data, "Materials updated");
        //}
    
    }

}
