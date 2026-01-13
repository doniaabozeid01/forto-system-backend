using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Inventory;

namespace Forto.Domain.Entities.Ops
{
    public class BookingItemMaterialUsage : BaseEntity
    {
        public int BookingItemId { get; set; }
        public BookingItem BookingItem { get; set; } = null!;

        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public decimal DefaultQty { get; set; }   // من الـ recipe
        public decimal ReservedQty { get; set; }  // المحجوز حالياً
        public decimal ActualQty { get; set; }    // العامل يعدله (مبدئياً = Default)

        public decimal UnitCost { get; set; }     // snapshot من Material وقت start
        public decimal UnitCharge { get; set; }   // snapshot من Material وقت start

        public decimal ExtraCharge { get; set; }  // max(0, actual-default)*unitCharge

        public int? RecordedByEmployeeId { get; set; }
        public DateTime? RecordedAt { get; set; }
    }

}
