using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Bookings
{
    public class BookingItemMaterialChangeRequest : BaseEntity
    {
        public int BookingItemId { get; set; }

        public MaterialChangeRequestStatus Status { get; set; } = MaterialChangeRequestStatus.Pending;

        public int RequestedByEmployeeId { get; set; }
        public DateTime RequestedAt { get; set; }

        public int? ReviewedByCashierId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }

        public List<BookingItemMaterialChangeRequestLine> Lines { get; set; } = new();
    }

}
