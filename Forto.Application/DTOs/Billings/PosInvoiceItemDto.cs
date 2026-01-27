using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings
{
    public class PosInvoiceItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(0.001, 100000000)]
        public int Qty { get; set; }
    }
}
