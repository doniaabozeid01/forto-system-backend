using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings
{
    public class InvoiceResponse
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = "";

        public int BookingId { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        /// <summary>المجموع قبل الضريبة بعد تعديل الكاشير (إن وُجد). الـ Total النهائي = AdjustedTotal + ضريبة 14% - الخصم.</summary>
        public decimal? AdjustedTotal { get; set; }

        public InvoiceStatus Status { get; set; }

        public int? PaidByEmployeeId { get; set; }
        public int? SupervisorId { get; set; }
        public DateTime? PaidAt { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        /// <summary>مبلغ الدفع كاش (للتسجيل فقط).</summary>
        public decimal? CashAmount { get; set; }
        /// <summary>مبلغ الدفع فيزا (للتسجيل فقط).</summary>
        public decimal? VisaAmount { get; set; }
        public string ClientName { get; set; }
        public string ClientNumber { get; set; }
        /// <summary>رقم لوحة العربية من الحجز.</summary>
        public string PlateNumber { get; set; } = "";

        public List<InvoiceLineResponse> Lines { get; set; } = new();
    }
}
