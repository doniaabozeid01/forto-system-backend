using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Enum
{
    public enum PaymentMethod
    {
        Cash = 1,
        Visa = 2,
        Custom = 3  // كاش + فيزا حسب ما يُدخل الكاشير
    }
}
