using Forto.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Forto.Application.DTOs.Billings
{
    public class PayCashRequest
    {
        [Required]
        public int CashierId { get; set; }
        /// <summary>Cash = كل المبلغ كاش. Visa = كل المبلغ فيزا. Custom = حسب CashAmount و VisaAmount.</summary>
        public PaymentMethod? PaymentMethod { get; set; }
        /// <summary>لـ Custom فقط: مبلغ الكاش.</summary>
        public decimal? CashAmount { get; set; }
        /// <summary>لـ Custom فقط: مبلغ الفيزا.</summary>
        public decimal? VisaAmount { get; set; }
    }
}
