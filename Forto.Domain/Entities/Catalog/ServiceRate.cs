using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Catalog
{
    public class ServiceRate : BaseEntity
    {
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public CarBodyType BodyType { get; set; }

        public decimal Price { get; set; }
        public int DurationMinutes { get; set; } // مهم للحجز

        public bool IsActive { get; set; } = true;
    }
}
