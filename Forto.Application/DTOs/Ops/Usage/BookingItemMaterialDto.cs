using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Usage
{
    public class BookingItemMaterialDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = "";
        public string Unit { get; set; } = "";

        public decimal DefaultQty { get; set; }
        public decimal ReservedQty { get; set; }
        public decimal ActualQty { get; set; }

        public decimal UnitCharge { get; set; }
        public decimal ExtraCharge { get; set; }
    }
}
