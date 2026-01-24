using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Inventory.Products
{
    public class ProductWithStockResponse : ProductResponse
    {
        public decimal OnHandQty { get; set; }
        public decimal ReservedQty { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal ReorderLevel { get; set; }
    }

}
