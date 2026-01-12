using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Catalog.Recipes;
using Forto.Domain.Enum;

namespace Forto.Application.Abstractions.Services.Catalogs.Recipes
{
    public interface IServiceRecipeService
    {
        Task<ServiceRecipeResponse> UpsertAsync(int serviceId, CarBodyType bodyType, UpsertServiceRecipeRequest request);
        Task<ServiceRecipeResponse> GetAsync(int serviceId, CarBodyType bodyType);
    }
}
