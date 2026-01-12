using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Enum;

namespace Forto.Domain.Entities.Catalog
{
    public class ServiceMaterialRecipe : BaseEntity
    {
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public CarBodyType BodyType { get; set; }

        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public decimal DefaultQty { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
