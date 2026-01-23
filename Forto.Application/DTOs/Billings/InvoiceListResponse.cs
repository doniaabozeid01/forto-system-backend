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
    }

public class InvoiceListItemDto
{
    public int InvoiceId { get; set; }
    public DateTime Date { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }

    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";

    // نص سريع للعرض (اختياري تسيبيه)
    public string ItemsText { get; set; } = "";

        // ✅ الجديد: تفاصيل محتوى الفاتورة

        //public string Services { get; set; } = ""; // نص للعرض

        public List<InvoiceLineListDto> Lines { get; set; } = new();
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
