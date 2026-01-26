using Forto.Application.Abstractions.Services.Bookings.Cashier.checkout;
using Forto.Application.DTOs.Bookings.cashier.checkout;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [ApiController]
    [Route("api/cashier")]
    public class CashierCheckoutController : BaseApiController
    {
        private readonly ICashierCheckoutService _service;

        public CashierCheckoutController(ICashierCheckoutService service)
        {
            _service = service;
        }

        //[HttpPost("checkout")]
        //public async Task<IActionResult> Checkout([FromBody] CashierCheckoutRequest request)
        //{
        //    var data = await _service.CheckoutNowAsync(request);
        //    return OkResponse(data, "Checkout completed");
        //}
    }

}
