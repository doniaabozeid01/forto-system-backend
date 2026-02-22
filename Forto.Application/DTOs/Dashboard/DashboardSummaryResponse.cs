using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Dashboard
{
    public class DashboardSummaryResponse
    {
        public int BranchId { get; set; }
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }

        public decimal PaidRevenue { get; set; }

        public decimal MaterialsConsumeCost { get; set; }
        public decimal MaterialsWasteCost { get; set; }
        public decimal MaterialsAdjustNet { get; set; }

        public decimal ProductsSoldCost { get; set; }
        public decimal GiftsCost { get; set; }
        public decimal ProductsAdjustNet { get; set; }

        public decimal TotalCosts { get; set; }

        /// <summary>ربح التشغيل = الإيراد المدفوع − إجمالي التكاليف (بدون فروقات الجرد).</summary>
        public decimal OperatingProfit { get; set; }

        /// <summary>فروقات الجرد = صافي تعديلات المواد + صافي تعديلات المنتجات. معروضة منفصلة ولا تدخل في ربح التشغيل.</summary>
        public decimal InventoryVariance { get; set; }

        /// <summary>الصافي المحاسبي النهائي (اختياري) = ربح التشغيل + فروقات الجرد.</summary>
        public decimal FinalAccountingNet { get; set; }

        /// <summary>يساوي ربح التشغيل (للتوافق مع الاستخدام السابق؛ الاعتماد على OperatingProfit موصى به).</summary>
        public decimal NetProfit { get; set; }
    }

}
