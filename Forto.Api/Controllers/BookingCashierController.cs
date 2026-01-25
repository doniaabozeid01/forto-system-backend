using Forto.Application.Abstractions.Services.Bookings.Cashier;
using Forto.Application.DTOs.Bookings.cashier;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/")]
    public class BookingCashierController : BaseApiController
    {

        private readonly IBookingLifecycleService _lifecycle;
        private readonly IBookingItemOpsService _itemOps;

        public BookingCashierController(IBookingLifecycleService lifecycle, IBookingItemOpsService itemOps)
        {
            _lifecycle = lifecycle;
            _itemOps = itemOps;
        }





        [HttpPost("bookings-cashier/{bookingId:int}/start")]
        public async Task<IActionResult> Start(int bookingId, [FromBody] CashierActionDto dto)
            => OkResponse(await _lifecycle.StartBookingAsync(bookingId, dto.CashierId), "Booking started");






        [HttpPost("bookings-cashier/{bookingId:int}/complete")]
        public async Task<IActionResult> Complete(int bookingId, [FromBody] CashierActionDto dto)
            => OkResponse(await _lifecycle.CompleteBookingAsync(bookingId, dto.CashierId), "Booking completed");





        [HttpPost("bookings-cashier/{bookingId:int}/services")]
        public async Task<IActionResult> AddService(int bookingId, [FromBody] AddServiceToBookingRequest dto)
            => OkResponse(await _itemOps.AddServiceAsync(bookingId, dto), "Service added");




        //[HttpPost("CancelService/{bookingItemId:int}")]
        //public async Task<IActionResult> CancelService(int bookingItemId, [FromBody] CancelBookingItemByCashierRequest dto)
        //    => OkResponse(await _itemOps.CancelServiceAsync(bookingItemId, dto), "Service cancelled");



    }
}
