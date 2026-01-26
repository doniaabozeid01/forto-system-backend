using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Ops.Usage;

namespace Forto.Application.DTOs.Inventory.Materials
{
    public class UpdateBookingItemMaterialsByCashierRequest
    {
        public int CashierId { get; set; }
        public List<UpdateMaterialActualDto> Materials { get; set; } = new();
    }

}
