using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Bookings
{
    public class BookingItemMaterialChangeRequestLine : BaseEntity
    {
        public int RequestId { get; set; }
        public BookingItemMaterialChangeRequest Request { get; set; } = null!;

        public int MaterialId { get; set; }

        public decimal ProposedActualQty { get; set; }
    }

}
