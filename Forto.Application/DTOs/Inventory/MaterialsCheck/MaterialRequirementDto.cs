using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Inventory.MaterialsCheck
{
    public class MaterialRequirementDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = "";
        public string Unit { get; set; } = "";

        public decimal RequiredQty { get; set; }
        public decimal AvailableQty { get; set; } // OnHand - Reserved
        public decimal ShortageQty { get; set; }  // max(0, required - available)
    }
}
