using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Stock
{
    public class StockAdjustRequest
    {
        [Required]
        public int CashierId { get; set; }

        [Required]
        public int MaterialId { get; set; }

        // الكمية الفعلية بعد الجرد
        [Range(0, 100000000)]
        public decimal PhysicalOnHandQty { get; set; }

        public string? Notes { get; set; }
        public DateTime? OccurredAt { get; set; }
    }

}
