using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Billings
{
    public class Tips : BaseEntity
    {
        public decimal Amount { get; set; }
        public DateOnly TipsDate { get; set; }
        public int? CashierId { get; set; }
    }
}
