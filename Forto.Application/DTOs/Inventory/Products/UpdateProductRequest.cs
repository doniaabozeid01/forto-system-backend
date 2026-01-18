using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Inventory.Products
{
    public class UpdateProductRequest
    {
        [Required, MinLength(2)]
        public string Name { get; set; } = "";

        public string? Sku { get; set; }

        [Range(0, 100000000)]
        public decimal SalePrice { get; set; }

        [Range(0, 100000000)]
        public decimal CostPerUnit { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
