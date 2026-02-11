using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Clients;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Billings
{
    public class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = null!;
        public int? BookingId { get; set; }
        public Booking? Booking { get; set; } = null!;
        public int? BranchId { get; set; }            // ✅ required
        public Branch? Branch { get; set; } = null!;
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal Total { get; set; }
        /// <summary>المجموع قبل الضريبة بعد تعديل الكاشير (زيادة أو نقص). لو null يُستخدم SubTotal. الـ Total يُحسب منه: AdjustedTotal + (AdjustedTotal × 14%) - Discount.</summary>
        public decimal? AdjustedTotal { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
        public int? PaidByEmployeeId { get; set; }   // cashier employee id
        public int? SupervisorId { get; set; }      // مشرف الفاتورة (من الـ checkout)
        public DateTime? PaidAt { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        /// <summary>مبلغ الدفع كاش (للتسجيل فقط، لا يغيّر Total).</summary>
        public decimal? CashAmount { get; set; }
        /// <summary>مبلغ الدفع فيزا (للتسجيل فقط، لا يغيّر Total).</summary>
        public decimal? VisaAmount { get; set; }
        public int? ClientId { get; set; }          // nullable
        public Client? Client { get; set; }         // optional navigation
        public string? CustomerPhone { get; set; }  // snapshot
        public string? CustomerName { get; set; }   // snapshot
        public decimal TaxRate { get; set; } = 0.14m; // 14%
        public decimal TaxAmount { get; set; }       // محسوبة

        /// <summary>سبب طلب الحذف من الكاشير.</summary>
        public string? DeletionReason { get; set; }
        /// <summary>وقت طلب الحذف.</summary>
        public DateTime? DeletionRequestedAt { get; set; }
        /// <summary>موظف الكاشير اللي طلب الحذف.</summary>
        public int? DeletionRequestedByEmployeeId { get; set; }
        /// <summary>لو الأدمن رفض الحذف — وقت الرفض (عشان الكاشير يشوف "رفض الأدمن").</summary>
        public DateTime? DeletionRejectedAt { get; set; }

        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    }

}
