using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Bookings
{
    public class BookingItem : BaseEntity
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        // snapshot من عربية العميل وقت الحجز
        public CarBodyType BodyType { get; set; }

        // snapshot من ServiceRate
        public decimal UnitPrice { get; set; }
        public int DurationMinutes { get; set; }

        public BookingItemStatus Status { get; set; } = BookingItemStatus.Pending;

        // Option B: يتحدد عند Start
        public int? AssignedEmployeeId { get; set; }
        public Employee? AssignedEmployee { get; set; }

        public byte[]? RowVersion { get; set; }
        public decimal MaterialAdjustment { get; set; } = 0m;

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}