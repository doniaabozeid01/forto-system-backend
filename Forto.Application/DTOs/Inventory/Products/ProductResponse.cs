using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Inventory.Products
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Sku { get; set; }
        public decimal SalePrice { get; set; }
        public decimal CostPerUnit { get; set; }
        public bool IsActive { get; set; }
    }

}
