using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Inventory
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }  = "";
        public string? Sku { get; set; }       // اختياري

        public decimal SalePrice { get; set; } // سعر البيع للعميل
        public decimal CostPerUnit { get; set; } // تكلفة عليك

        public bool IsActive { get; set; } = true;
    }
}