using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings.cashier
{
    public class CashierActionRequest
    {
        public int CashierId { get; set; }
        public string? Reason { get; set; }

        // اختياري: الكاشير يدخل الاستهلاك الحقيقي
        public List<MaterialUsedOverrideDto>? UsedOverride { get; set; }
    }
}
