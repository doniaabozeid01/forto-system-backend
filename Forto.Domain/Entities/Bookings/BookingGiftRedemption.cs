using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities.Inventory;

namespace Forto.Domain.Entities.Bookings
{
    public class BookingGiftRedemption : BaseEntity
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int InvoiceId { get; set; }
        public int InvoiceLineId { get; set; }

        public int SelectedByCashierId { get; set; }
        public DateTime OccurredAt { get; set; }

        public string? Notes { get; set; }
    }

}
