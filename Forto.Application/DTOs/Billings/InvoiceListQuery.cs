using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings
{
    public class InvoiceListQuery
    {
        public int BranchId { get; set; }

        public string? From { get; set; }
        public string? To { get; set; }

        public string? PaymentMethod { get; set; } // "all" | "cash"
        public string? Q { get; set; } // invoiceId OR phone OR name

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}
