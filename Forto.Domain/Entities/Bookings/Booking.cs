using Forto.Domain.Entities.Clients;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Bookings
{
    public class Booking : BaseEntity
    {
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public DateTime ScheduledStart { get; set; }

        // هنستخدمها للتحقق “الساعه عربيتين”
        public DateTime SlotHourStart { get; set; }

        public decimal TotalPrice { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }

        public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();
    }
}