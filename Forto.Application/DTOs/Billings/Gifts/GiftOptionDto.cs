using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings.Gifts
{
    public class GiftOptionDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? Sku { get; set; }

        public decimal AvailableQty { get; set; } // من مخزون الفرع
    }
}
