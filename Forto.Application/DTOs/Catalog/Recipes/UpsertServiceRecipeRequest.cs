using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Recipes
{
    public class UpsertServiceRecipeRequest
    {
        [Required, MinLength(1)]
        public List<RecipeMaterialItemRequest> Materials { get; set; } = new();
    }
}
