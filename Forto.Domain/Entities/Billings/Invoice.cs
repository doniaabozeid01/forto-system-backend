using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Billings
{
    public class Invoice : BaseEntity
    {
        public int? BookingId { get; set; }
        public Booking? Booking { get; set; } = null!;

        public int? BranchId { get; set; }            // ✅ required
        public Branch? Branch { get; set; } = null!;

        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal Total { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;

        public int? PaidByEmployeeId { get; set; }   // cashier employee id
        public DateTime? PaidAt { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    }

}
