using Forto.Domain.Enum;
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

        public PosCustomerDto? Customer { get; set; }   // ✅ new (optional)

        /// <summary>المجموع قبل الضريبة بعد تعديل الكاشير (زيادة أو نقص). لو مش مُرسل يُستخدم الـ SubTotal المحسوب. الـ Total النهائي = AdjustedTotal + (AdjustedTotal × 14%) - الخصم.</summary>
        public decimal? AdjustedTotal { get; set; }

        /// <summary>طريقة الدفع: Cash = كل المبلغ كاش والفيزا 0. Visa = كل المبلغ فيزا والكاش 0. Custom = حسب CashAmount و VisaAmount المُرسلة.</summary>
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        /// <summary>لـ Custom فقط: مبلغ الكاش. مع Cash/Visa يُتجاهل.</summary>
        public decimal? CashAmount { get; set; }

        /// <summary>لـ Custom فقط: مبلغ الفيزا. مع Cash/Visa يُتجاهل.</summary>
        public decimal? VisaAmount { get; set; }

        /// <summary>معرف وردية الكاشير (اختياري). يُسجّل مع الفاتورة للربط بالشيفت.</summary>
        public int? CashierShiftId { get; set; }
    }



    public class PosCustomerDto
    {
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
    }


}
