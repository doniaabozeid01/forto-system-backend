using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings
{

    public class CreatePosInvoiceRequest
    {
        [Required]
        public int BranchId { get; set; }

        [Required]
        public int CashierId { get; set; }

        [Required, MinLength(1)]
        public List<PosInvoiceItemDto> Items { get; set; } = new();

        public DateTime? OccurredAt { get; set; }
        public string? Notes { get; set; }
    }



}
