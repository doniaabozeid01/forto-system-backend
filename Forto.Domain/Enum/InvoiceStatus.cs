using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Enum
{
    public enum InvoiceStatus
    {
        Unpaid = 1,
        Paid = 2,
        Cancelled = 3,
        /// <summary>طلب حذف من الكاشير — في انتظار موافقة الأدمن.</summary>
        PendingDeletion = 4,
        /// <summary>تم الحذف بعد موافقة الأدمن.</summary>
        Deleted = 5
    }
}
