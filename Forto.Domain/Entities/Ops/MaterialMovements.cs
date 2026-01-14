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

        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public MaterialMovementType MovementType { get; set; }

        public decimal Qty { get; set; }                 // + أو - حسب النوع (Adjust ممكن بالسالب)
        public decimal UnitCostSnapshot { get; set; }    // تكلفة الوحدة وقت الحركة
        public decimal TotalCost { get; set; }           // Qty * UnitCostSnapshot (خليها بالسالب لو Qty سالب)

        public DateTime OccurredAt { get; set; }         // ✅ اللي هنفلتر عليه

        public int? BookingId { get; set; }              // اختياري
        public int? BookingItemId { get; set; }          // ✅ مهم للـ Waste/Consume

        public int? RecordedByEmployeeId { get; set; }   // الكاشير/العامل
        public string? Notes { get; set; }
    }

}
