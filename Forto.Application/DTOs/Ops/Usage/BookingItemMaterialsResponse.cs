using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Usage
{
    public class BookingItemMaterialsResponse
    {
        public int BookingItemId { get; set; }
        public List<BookingItemMaterialDto> Materials { get; set; } = new();
        public decimal TotalExtraCharge { get; set; }
    }
}
