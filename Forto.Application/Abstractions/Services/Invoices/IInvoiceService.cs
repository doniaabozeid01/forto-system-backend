using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Billings.cashier;
using Forto.Application.DTOs.Billings.Gifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Invoices
{
    public interface IInvoiceService
    {
        Task<InvoiceResponse?> GetByBookingIdAsync(int bookingId);
        Task<InvoiceResponse> EnsureInvoiceForBookingAsync(int bookingId); // create if not exists
        Task<InvoiceResponse> PayCashAsync(int invoiceId, PayCashRequest request);

        /// <summary>تعيين المجموع قبل الضريبة (AdjustedTotal) على الفاتورة قبل الدفع — مثلاً قبل إنهاء الخدمة. الـ Total يُحسب منه + ضريبة 14% - الخصم.</summary>
        Task<InvoiceResponse> SetAdjustedTotalAsync(int invoiceId, decimal adjustedTotal);

        Task RecalculateForBookingAsync(int bookingId, bool save = true); // لو اتلغت خدمات قبل الدفع

        Task<InvoiceResponse> SellProductAsync(int invoiceId, SellProductOnInvoiceRequest request);

        Task<InvoiceGiftOptionsResponse> GetGiftOptionsAsync(int invoiceId);
        Task<InvoiceResponse> SelectGiftAsync(int invoiceId, SelectInvoiceGiftRequest request);





        Task<InvoiceResponse> CreatePosInvoicePaidCashAsync(CreatePosInvoiceRequest request);

        Task<InvoiceListResponse> ListAsync(InvoiceListQuery query);

        /// <summary>تقرير المنتجات المبيعة والمتحاسب عليها من تاريخ لتاريخ: كل منتج مع سعره ورقم الفاتورة والمجموع الكلي.</summary>
        Task<SoldProductsReportResponse> GetSoldProductsReportAsync(DateTime fromDate, DateTime toDate);

        /// <summary>الكاشير يطلب حذف الفاتورة (سبب إجباري) — الفاتورة تبقى PendingDeletion ويتبعت إيميل للأدمن.</summary>
        Task<InvoiceResponse> RequestDeletionAsync(int invoiceId, RequestInvoiceDeletionRequest request);
        /// <summary>الأدمن يوافق على الحذف — الفاتورة تبقى Deleted.</summary>
        Task<InvoiceResponse> ApproveDeletionAsync(int invoiceId);
        /// <summary>الأدمن يرفض الحذف — الفاتورة ترجع لحالتها السابقة والكاشير يشوف "رفض الأدمن".</summary>
        Task<InvoiceResponse> RejectDeletionAsync(int invoiceId);
    }
}
