using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Billings
{
    public class InvoiceLine : BaseEntity
    {
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public string Description { get; set; } = "";
        public int Qty { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }

        public int? BookingItemId { get; set; }

        public InvoiceLineType LineType { get; set; } = InvoiceLineType.Service; // enum

        // للمرحلة الجاية (Products/Gifts) نضيف:
        // public string LineType {get;set;}
        // public int? RefId {get;set;}
    }

}
