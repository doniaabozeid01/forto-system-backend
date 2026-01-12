using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Stock
{
    public class UpsertBranchStockRequest
    {
        [Required]
        public int MaterialId { get; set; }

        [Range(0, 100000000)]
        public decimal OnHandQty { get; set; }

        [Range(0, 100000000)]
        public decimal ReorderLevel { get; set; } = 0;
    }
}
