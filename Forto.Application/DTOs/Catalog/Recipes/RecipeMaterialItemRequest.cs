using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Recipes
{
    public class RecipeMaterialItemRequest
    {
        [Required]
        public int MaterialId { get; set; }

        [Range(0, 100000000)]
        public decimal DefaultQty { get; set; }
    }
}
