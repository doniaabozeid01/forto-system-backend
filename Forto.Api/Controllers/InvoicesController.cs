using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Billings.cashier;
using Forto.Application.DTOs.Billings.Gifts;
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





        [HttpPost("{invoiceId:int}/products")]
        public async Task<IActionResult> SellProduct(int invoiceId, [FromBody] SellProductOnInvoiceRequest request)
        {
            var data = await _service.SellProductAsync(invoiceId, request);
            return OkResponse(data, "Product added to invoice");
        }










        [HttpGet("{invoiceId:int}/gift-options")]
        public async Task<IActionResult> GetGiftOptions(int invoiceId)
        {
            var data = await _service.GetGiftOptionsAsync(invoiceId);
            return OkResponse(data, "OK");
        }






        /// <summary>إضافة هدية على فاتورة (بعد الـ Complete/Checkout) — Body: cashierId, productId.</summary>
        [HttpPost("{invoiceId:int}/gift/select")]
        public async Task<IActionResult> SelectGift(int invoiceId, [FromBody] SelectInvoiceGiftRequest request)
        {
            var data = await _service.SelectGiftAsync(invoiceId, request);
            return OkResponse(data, "Gift selected");
        }

        /// <summary>نفس SelectGift — بس invoiceId جوا الـ body مع productId و cashierId (لو الفرونت حابب يبعت كل حاجة في body واحد).</summary>
        [HttpPost("gift/add")]
        public async Task<IActionResult> AddGiftToInvoice([FromBody] AddGiftToInvoiceRequest request)
        {
            var req = new SelectInvoiceGiftRequest
            {
                CashierId = request.CashierId,
                ProductId = request.ProductId,
                OccurredAt = request.OccurredAt,
                Notes = request.Notes
            };
            var data = await _service.SelectGiftAsync(request.InvoiceId, req);
            return OkResponse(data, "Gift added");
        }






        [HttpPost("pos")]
        public async Task<IActionResult> CreatePosInvoice([FromBody] CreatePosInvoiceRequest request)
        {
            var data = await _service.CreatePosInvoicePaidCashAsync(request);
            return CreatedResponse(data, "POS invoice created");
        }





        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] InvoiceListQuery query)
        {
            var data = await _service.ListAsync(query);
            return OkResponse(data, "OK");
        }




    }

}
