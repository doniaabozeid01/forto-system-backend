using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings
{
    public class InvoiceResponse
    {
        public int Id { get; set; }
        public int BookingId { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public InvoiceStatus Status { get; set; }

        public int? PaidByEmployeeId { get; set; }
        public DateTime? PaidAt { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        public List<InvoiceLineResponse> Lines { get; set; } = new();
    }
}
