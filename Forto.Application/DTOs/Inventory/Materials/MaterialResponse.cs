using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Inventory.Materials
{
    public class MaterialResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public MaterialUnit Unit { get; set; }
        public decimal CostPerUnit { get; set; }
        public decimal ChargePerUnit { get; set; }
        public bool IsActive { get; set; }
    }
}
