using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings
{
    public class PayCashRequest
    {
        [Required]
        public int CashierId { get; set; }
    }
}
