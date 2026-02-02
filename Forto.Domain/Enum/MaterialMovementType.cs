using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Enum
{
    public enum MaterialMovementType
    {
        In = 1,           // Stock IN
        Consume = 2,      // Material consumption (completed or cancelled)
        Waste = 3,        // Cancel after start / damaged
        Adjust = 4,       // Inventory count adjustment
        ServiceCharge = 5 // سعر الخدمة الكامل (خدمة مكتملة) - مصدر للفاتورة
    }

}
