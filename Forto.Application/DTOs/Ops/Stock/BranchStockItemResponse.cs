using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Stock
{
    public class BranchStockItemResponse
    {
        public int BranchId { get; set; }
        public int MaterialId { get; set; }

        public string MaterialName { get; set; } = "";
        public string Unit { get; set; } = "";

        public decimal OnHandQty { get; set; }
        public decimal ReservedQty { get; set; }
        public decimal AvailableQty { get; set; } // OnHand - Reserved
        public decimal ReorderLevel { get; set; }
    }
}
