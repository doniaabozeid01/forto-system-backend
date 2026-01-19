using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings.Gifts
{
    public class SelectInvoiceGiftRequest
    {
        [Required]
        public int CashierId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public DateTime? OccurredAt { get; set; }
        public string? Notes { get; set; }
    }
}
