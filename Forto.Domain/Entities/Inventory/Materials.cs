using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Domain.Entities.Inventory
{
    public class Material : BaseEntity
    {
        public string Name { get; set; } = "";
        public MaterialUnit Unit { get; set; }

        public decimal CostPerUnit { get; set; }      // cost to you
        public decimal ChargePerUnit { get; set; }    // charged to customer (for extra)

        public bool IsActive { get; set; } = true;
    }
}
