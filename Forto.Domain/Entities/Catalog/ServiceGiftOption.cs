using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities.Inventory;

namespace Forto.Domain.Entities.Catalog
{
    public class ServiceGiftOption : BaseEntity
    {
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }

}
