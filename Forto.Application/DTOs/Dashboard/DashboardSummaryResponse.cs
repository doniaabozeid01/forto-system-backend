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
        public decimal NetProfit { get; set; }
    }

}
