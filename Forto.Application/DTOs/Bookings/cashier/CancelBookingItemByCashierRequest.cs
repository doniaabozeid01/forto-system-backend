using Forto.Application.DTOs.Billings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings.cashier
{
    public class CancelBookingItemByCashierRequest
    {
        public int CashierId { get; set; }
        public string? Reason { get; set; }

        // optional: cashier can override actual used when cancel inprogress
        public List<MaterialUsedOverrideDto>? UsedOverride { get; set; }
    }
}
