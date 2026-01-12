using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities.Inventory;

namespace Forto.Domain.Entities.Ops
{
    public class BranchMaterialStock : BaseEntity
    {
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public decimal OnHandQty { get; set; } = 0;     // الموجود فعليًا
        public decimal ReservedQty { get; set; } = 0;   // المحجوز (للشغل اللي بدأ)
        public decimal ReorderLevel { get; set; } = 0;  // اختياري عشان يرجعلي انذار مثلا بعدين ان لازم اشتري باه 

    }
}
