using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities.Inventory;

namespace Forto.Domain.Entities.Ops
{
    public class BranchProductStock : BaseEntity
    {
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public decimal OnHandQty { get; set; } = 0;
        public decimal ReservedQty { get; set; } = 0;
        public decimal ReorderLevel { get; set; } = 0;
    }

}
