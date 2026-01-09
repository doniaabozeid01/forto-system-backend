using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.DTOs.Billings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/invoices")]
    public class InvoicesController : BaseApiController
    {
        private readonly IInvoiceService _service;

        public InvoicesController(IInvoiceService service) => _service = service;

        [HttpGet("by-booking/{bookingId:int}")]
        public async Task<IActionResult> GetByBooking(int bookingId)
        {
            var data = await _service.GetByBookingIdAsync(bookingId);
            if (data == null) return FailResponse("Invoice not found", 404);
            return OkResponse(data, "OK");
        }

        [HttpPost("{invoiceId:int}/pay-cash")]
        public async Task<IActionResult> PayCash(int invoiceId, [FromBody] PayCashRequest request)
        {
            var data = await _service.PayCashAsync(invoiceId, request.CashierId);
            return OkResponse(data, "Paid");
        }
    }

}
