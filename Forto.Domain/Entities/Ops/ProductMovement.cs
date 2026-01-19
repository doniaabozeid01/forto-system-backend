using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Enum;

namespace Forto.Domain.Entities.Ops
{

    public class ProductMovement : BaseEntity
    {
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public ProductMovementType MovementType { get; set; }

        public decimal Qty { get; set; }                 // Adjust ممكن بالسالب
        public decimal UnitCostSnapshot { get; set; }    // تكلفة الوحدة وقت الحركة
        public decimal TotalCost { get; set; }           // Qty * UnitCostSnapshot

        public DateTime OccurredAt { get; set; }         // ✅ فلترة التقارير

        public int? InvoiceId { get; set; }              // للـ SELL لاحقاً
        public int? BookingId { get; set; }              // للـ GIFT لاحقاً
        public int? BookingItemId { get; set; }          // للـ GIFT لاحقاً

        public int? RecordedByEmployeeId { get; set; }   // cashier
        public string? Notes { get; set; }
    }

}
