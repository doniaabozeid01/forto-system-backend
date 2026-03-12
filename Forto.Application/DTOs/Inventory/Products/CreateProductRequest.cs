using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Inventory.Products
{
    public class CreateProductRequest
    {
        [Required, MinLength(2)]
        public string Name { get; set; } = "";

        public string? Sku { get; set; }

        [Range(0, 100000000)]
        public decimal SalePrice { get; set; }

        [Range(0, 100000000)]
        public decimal CostPerUnit { get; set; }

        /// <summary>معرف فئة المنتج (اختياري).</summary>
        public int? CategoryId { get; set; }

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
