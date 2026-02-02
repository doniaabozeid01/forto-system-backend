using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Enum;

namespace Forto.Domain.Entities.Ops
{
    public class MaterialMovement : BaseEntity
    {
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public int? MaterialId { get; set; }             // null للـ ServiceCharge
        public Material? Material { get; set; }

        public MaterialMovementType MovementType { get; set; }

        public decimal Qty { get; set; }                 // + أو - حسب النوع (Adjust ممكن بالسالب)
        public decimal UnitCostSnapshot { get; set; }    // تكلفة الوحدة وقت الحركة
        public decimal TotalCost { get; set; }           // Qty * UnitCostSnapshot (للجرد)

        /// <summary>
        /// سعر بيع الوحدة - ما يدفعه العميل.
        /// Consume (خدمة مكتملة): لا يُستخدم هنا، نستخدم ServiceCharge بدلاً منه.
        /// Consume (خدمة ملغاة): سعر بيع الخامات المستخدمة.
        /// </summary>
        public decimal? UnitCharge { get; set; }
        /// <summary>
        /// إجمالي ما يدفعه العميل (Qty * UnitCharge أو سعر الخدمة للـ ServiceCharge).
        /// مصدر الفاتورة - نجمع من هنا.
        /// </summary>
        public decimal? TotalCharge { get; set; }

        public DateTime OccurredAt { get; set; }

        public int? BookingId { get; set; }
        public int? BookingItemId { get; set; }

        public string? Notes { get; set; }
    }

}
