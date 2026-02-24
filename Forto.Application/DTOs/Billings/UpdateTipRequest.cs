using System.ComponentModel.DataAnnotations;

namespace Forto.Application.DTOs.Billings
{
    public class UpdateTipRequest
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative")]
        public decimal Amount { get; set; }

        /// <summary>التاريخ بصيغة YYYY-M-D مثل 2026-4-2</summary>
        [Required]
        public string TipsDate { get; set; } = "";

        /// <summary>معرف الكاشير (اختياري)</summary>
        public int? CashierId { get; set; }
    }
}
