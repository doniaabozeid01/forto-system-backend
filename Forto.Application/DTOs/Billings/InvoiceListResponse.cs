using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Billings
{
    public class InvoiceListResponse
    {
        public InvoiceListSummary Summary { get; set; } = new();
        public List<InvoiceListItemDto> Items { get; set; } = new();

        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class InvoiceListSummary
    {
        public int TotalCount { get; set; }
        public decimal TotalRevenue { get; set; }
        /// <summary>إجمالي مبالغ الدفع كاش.</summary>
        public decimal TotalCashAmount { get; set; }
        /// <summary>إجمالي مبالغ الدفع فيزا.</summary>
        public decimal TotalVisaAmount { get; set; }
        /// <summary>إجمالي التكلفة (فواتير مدفوعة فقط).</summary>
        public decimal TotalCost { get; set; }
        /// <summary>إجمالي الربح (إيراد − تكلفة).</summary>
        public decimal TotalProfit { get; set; }
    }

public class InvoiceListItemDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public DateTime Date { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    /// <summary>تكلفة الفاتورة (مواد + منتجات).</summary>
    public decimal TotalCost { get; set; }
    /// <summary>الربح = Total − TotalCost.</summary>
    public decimal Profit { get; set; }
    /// <summary>مبلغ الدفع كاش.</summary>
    public decimal? CashAmount { get; set; }
    /// <summary>مبلغ الدفع فيزا.</summary>
    public decimal? VisaAmount { get; set; }
    public InvoiceStatus? status { get; set; }
    /// <summary>تاريخ ووقت الدفع إن وُجد.</summary>
    public DateTime? PaidAt { get; set; }
    /// <summary>لو الأدمن رفض طلب الحذف — وقت الرفض (عشان تعرض "رفض الأدمن").</summary>
    public DateTime? DeletionRejectedAt { get; set; }
    /// <summary>وردية الكاشير اللي اتدفت فيها الفاتورة (لو اتدفت أثناء وردية مفتوحة).</summary>
    public int? CashierShiftId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    /// <summary>رقم لوحة العربية من الحجز.</summary>
    public string PlateNumber { get; set; } = "";
    public string ItemsText { get; set; } = "";
    public List<InvoiceLineListDto> Lines { get; set; } = new();

    // ✅ الجديد: تفاصيل محتوى الفاتورة

    //public string Services { get; set; } = ""; // نص للعرض

}

public class InvoiceLineListDto
{
    public int LineId { get; set; }
    public string Description { get; set; } = "";
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}


}
