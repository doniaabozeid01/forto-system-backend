using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Recipes
{
    public class ServiceRecipeMaterialResponse
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = "";
        public string Unit { get; set; } = "";

        public decimal DefaultQty { get; set; }
    }
}
