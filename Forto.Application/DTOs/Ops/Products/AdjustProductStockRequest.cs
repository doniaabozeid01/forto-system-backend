using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Products
{
    public class AdjustProductStockRequest
    {
        [Required]
        public int CashierId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(0, 100000000)]
        public decimal PhysicalOnHandQty { get; set; }

        public string? Notes { get; set; }
        public DateTime? OccurredAt { get; set; }
    }

}
