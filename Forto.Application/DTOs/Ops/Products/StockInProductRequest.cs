using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Products
{
    public class StockInProductRequest
    {
        [Required]
        public int CashierId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(0.001, 100000000)]
        public decimal Qty { get; set; }

        public decimal? UnitCost { get; set; } // ✅ اختياري: لو مش مبعوت ناخد من Product.CostPerUnit

        public string? Notes { get; set; }
        public DateTime? OccurredAt { get; set; }
    }

}
