using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Inventory.Materials
{
    public class CreateMaterialRequest
    {
        [Required, MinLength(2)]
        public string Name { get; set; } = "";

        [Required]
        public MaterialUnit Unit { get; set; }

        [Range(0, 1000000)]
        public decimal CostPerUnit { get; set; }

        [Range(0, 1000000)]
        public decimal ChargePerUnit { get; set; }

        /// <summary>معرف الفرع (مطلوب عند إدخال مخزون ابتدائي أو حد إعادة الطلب).</summary>
        public int? BranchId { get; set; }

        /// <summary>المخزون الابتدائي — لو مُدخل يُسجّل كحركة Stock In في الفرع.</summary>
        [Range(0, 100000000)]
        public decimal? InitialStockQty { get; set; }

        /// <summary>حد إعادة الطلب للفرع (يُطبّق عند إنشاء/تحديث رصيد الفرع).</summary>
        [Range(0, 100000000)]
        public decimal? ReorderLevel { get; set; }
    }

}
