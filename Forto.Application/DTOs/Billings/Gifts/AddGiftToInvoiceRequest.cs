using System;
using System.ComponentModel.DataAnnotations;

namespace Forto.Application.DTOs.Billings.Gifts
{
    /// <summary>طلب إضافة هدية على فاتورة (بعد الـ Complete/Checkout) — كل القيم في body واحد.</summary>
    public class AddGiftToInvoiceRequest
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public int CashierId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public DateTime? OccurredAt { get; set; }
        public string? Notes { get; set; }
    }
}
