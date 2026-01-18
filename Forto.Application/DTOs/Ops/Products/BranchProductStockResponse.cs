using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Products
{

    public class BranchProductStockResponse
    {
        public int BranchId { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; } = "";
        public string? Sku { get; set; }

        public decimal OnHandQty { get; set; }
        public decimal ReservedQty { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal ReorderLevel { get; set; }
    }

}
