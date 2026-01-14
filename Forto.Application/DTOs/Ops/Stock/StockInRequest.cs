using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Stock
{
    public class StockInRequest
    {
        [Required]
        public int CashierId { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Range(0.001, 100000000)]
        public decimal Qty { get; set; }

        [Range(0, 100000000)]
        public decimal UnitCost { get; set; } // تكلفة الشراء

        public string? Notes { get; set; }
        public DateTime? OccurredAt { get; set; } // لو null هنستخدم now
    }

}
