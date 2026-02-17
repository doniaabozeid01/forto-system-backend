using System;

namespace Forto.Application.DTOs.Billings
{
    /// <summary>صف واحد في تقرير المنتجات المبيعة: منتج متحاسب عليه مع رقم الفاتورة والسعر والمجموع.</summary>
    public class SoldProductItemDto
    {
        /// <summary>وصف المنتج (الاسم).</summary>
        public string ProductDescription { get; set; } = "";

        /// <summary>سعر الوحدة (المنتج بكام).</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>الكمية.</summary>
        public int Qty { get; set; }

        /// <summary>مجموع الصف (UnitPrice × Qty).</summary>
        public decimal LineTotal { get; set; }

        /// <summary>رقم الفاتورة اللي المنتج كان فيها.</summary>
        public string InvoiceNumber { get; set; } = "";

        /// <summary>معرّف الفاتورة.</summary>
        public int InvoiceId { get; set; }

        /// <summary>تاريخ الدفع (لو مدفوعة).</summary>
        public DateTime? PaidAt { get; set; }
    }

    /// <summary>تقرير المنتجات المبيعة والمتحاسب عليها في فترة (from - to): كل صف منتج مع رقم الفاتورة والمجموع الكلي.</summary>
    public class SoldProductsReportResponse
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        /// <summary>قائمة المنتجات المبيعة والمتحاسب عليها (كل صف = بند منتج في فاتورة مدفوعة).</summary>
        public List<SoldProductItemDto> Items { get; set; } = new();

        /// <summary>المجموع الكلي من كل المنتجات في التقرير.</summary>
        public decimal GrandTotal { get; set; }
    }
}
