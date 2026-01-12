using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Catalog.Recipes
{
    public class ServiceRecipeResponse
    {
        public int ServiceId { get; set; }
        public CarBodyType BodyType { get; set; }

        public List<ServiceRecipeMaterialResponse> Materials { get; set; } = new();
    }
}
