using Forto.Api.Hubs;
using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.Common;
using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Billings.cashier;
using Forto.Application.DTOs.Billings.Gifts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Forto.Api.Controllers
{
    [Route("api/invoices")]
    public class InvoicesController : BaseApiController
    {
        private readonly IInvoiceService _service;
        private readonly InvoiceDeletionLinkSettings _deletionLinkSettings;
        private readonly IHubContext<InvoiceDeletionHub> _deletionHub;

        public InvoicesController(
            IInvoiceService service,
            IOptions<InvoiceDeletionLinkSettings> deletionLinkOptions,
            IHubContext<InvoiceDeletionHub> deletionHub)
        {
            _service = service;
            _deletionLinkSettings = deletionLinkOptions?.Value ?? new InvoiceDeletionLinkSettings();
            _deletionHub = deletionHub;
        }


        /// <summary>يجيب الفاتورة بالـ bookingId. لو مفيش فاتورة تُنشأ تلقائياً ثم تُرجَع (ما عدا لو الحجز ملغى أو غير موجود).</summary>
        [HttpGet("by-booking/{bookingId:int}")]
        public async Task<IActionResult> GetByBooking(int bookingId)
        {
            var data = await _service.GetByBookingIdAsync(bookingId);
            if (data == null) return FailResponse("Booking not found or cancelled", 404);
            return OkResponse(data, "OK");
        }




        [HttpPost("{invoiceId:int}/pay-cash")]
        public async Task<IActionResult> PayCash(int invoiceId, [FromBody] PayCashRequest request)
        {
            var data = await _service.PayCashAsync(invoiceId, request);
            return OkResponse(data, "Paid");
        }

        /// <summary>تعيين المجموع قبل الضريبة (AdjustedTotal) على الفاتورة قبل الدفع — مثلاً قبل إنهاء الخدمة. الـ Total يُحسب منه + ضريبة 14% - الخصم. الفاتورة لازم تكون Unpaid.</summary>
        [HttpPatch("{invoiceId:int}/adjusted-total")]
        public async Task<IActionResult> SetAdjustedTotal(int invoiceId, [FromBody] SetAdjustedTotalRequest request)
        {
            if (request == null)
                return FailResponse("Request body required", 400);
            var data = await _service.SetAdjustedTotalAsync(invoiceId, request.AdjustedTotal);
            return OkResponse(data, "Adjusted total set");
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

        /// <summary>تقرير المنتجات المبيعة والمتحاسب عليها: ادّي from و to (تاريخ) يرجع المنتجات المتباعة، كل منتج بكام، وفي فاتورة رقم كام، والمجموع الكلي.</summary>
        [HttpGet("reports/sold-products")]
        public async Task<IActionResult> GetSoldProductsReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from > to)
                return FailResponse("From date must be before or equal to To date", 400);
            var data = await _service.GetSoldProductsReportAsync(from, to);
            return OkResponse(data, "OK");
        }

        /// <summary>الكاشير يطلب حذف الفاتورة — سبب إجباري. الفاتورة تبقى PendingDeletion ويُرسل إيميل للأدمن.</summary>
        [HttpPost("{invoiceId:int}/request-deletion")]
        public async Task<IActionResult> RequestDeletion(int invoiceId, [FromBody] RequestInvoiceDeletionRequest request)
        {
            if (request == null)
                return FailResponse("Request body required", 400);
            var data = await _service.RequestDeletionAsync(invoiceId, request);
            return OkResponse(data, "Deletion requested; admin will be notified.");
        }

        /// <summary>الأدمن يوافق على حذف الفاتورة — تصبح Deleted.</summary>
        [HttpPost("{invoiceId:int}/deletion/approve")]
        public async Task<IActionResult> ApproveDeletion(int invoiceId)
        {
            var data = await _service.ApproveDeletionAsync(invoiceId);
            await _deletionHub.Clients.All.SendAsync(InvoiceDeletionHub.EventName, invoiceId, "approved");
            return OkResponse(data, "Invoice deleted.");
        }

        /// <summary>الأدمن يرفض طلب الحذف — الفاتورة ترجع لحالتها والكاشير يشوف "رفض الأدمن".</summary>
        [HttpPost("{invoiceId:int}/deletion/reject")]
        public async Task<IActionResult> RejectDeletion(int invoiceId)
        {
            var data = await _service.RejectDeletionAsync(invoiceId);
            await _deletionHub.Clients.All.SendAsync(InvoiceDeletionHub.EventName, invoiceId, "rejected");
            return OkResponse(data, "Deletion rejected.");
        }

        /// <summary>رابط من الإيميل — الموافقة أو الرفض بالضغط على ACCEPT/REJECT. لا يتطلب تسجيل دخول.</summary>
        [HttpGet("deletion/confirm")]
        public async Task<IActionResult> ConfirmDeletion([FromQuery] int invoiceId, [FromQuery] string? action, [FromQuery] string? token)
        {
            if (invoiceId <= 0 || string.IsNullOrEmpty(action) || string.IsNullOrEmpty(token))
            {
                return Content(
                    "<html><body style='font-family: Arial; padding: 20px;'><h2>FORTO CAR CLEAN CENTER</h2><p>رابط غير صالح أو منتهي الصلاحية.</p></body></html>",
                    "text/html; charset=utf-8");
            }
            var secret = _deletionLinkSettings.Secret ?? "";
            if (!DeletionLinkToken.Validate(invoiceId, action, token, secret))
            {
                return Content(
                    "<html><body style='font-family: Arial; padding: 20px;'><h2>FORTO CAR CLEAN CENTER</h2><p>رابط غير صالح أو منتهي الصلاحية.</p></body></html>",
                    "text/html; charset=utf-8");
            }
            try
            {
                if (string.Equals(action, "approve", StringComparison.OrdinalIgnoreCase))
                {
                    await _service.ApproveDeletionAsync(invoiceId);
                    await _deletionHub.Clients.All.SendAsync(InvoiceDeletionHub.EventName, invoiceId, "approved");
                    return Content(
                        "<html><body style='font-family: Arial; padding: 20px;'><h2>FORTO CAR CLEAN CENTER</h2><p style='color: green; font-size: 18px;'>تمت الموافقة على حذف الفاتورة.</p></body></html>",
                        "text/html; charset=utf-8");
                }
                if (string.Equals(action, "reject", StringComparison.OrdinalIgnoreCase))
                {
                    await _service.RejectDeletionAsync(invoiceId);
                    await _deletionHub.Clients.All.SendAsync(InvoiceDeletionHub.EventName, invoiceId, "rejected");
                    return Content(
                        "<html><body style='font-family: Arial; padding: 20px;'><h2>FORTO CAR CLEAN CENTER</h2><p style='color: #b45309; font-size: 18px;'>تم رفض طلب الحذف.</p></body></html>",
                        "text/html; charset=utf-8");
                }
            }
            catch (Exception)
            {
                return Content(
                    "<html><body style='font-family: Arial; padding: 20px;'><h2>FORTO CAR CLEAN CENTER</h2><p style='color: red;'>حدث خطأ. قد تكون الفاتورة تمت معالجتها مسبقاً.</p></body></html>",
                    "text/html; charset=utf-8");
            }
            return Content(
                "<html><body style='font-family: Arial; padding: 20px;'><h2>FORTO CAR CLEAN CENTER</h2><p>رابط غير صالح.</p></body></html>",
                "text/html; charset=utf-8");
        }
    }
}
