using Forto.Application.Abstractions.Services.Catalogs.Recipes;
using Forto.Application.DTOs.Catalog.Recipes;
using Forto.Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/catalog/services/{serviceId:int}/recipes")]
    public class ServiceRecipesController : BaseApiController
    {
        private readonly IServiceRecipeService _service;

        public ServiceRecipesController(IServiceRecipeService service) => _service = service;

        [HttpGet("{bodyType:int}")]
        public async Task<IActionResult> Get(int serviceId, int bodyType)
        {
            var bt = (CarBodyType)bodyType;
            var data = await _service.GetAsync(serviceId, bt);
            return OkResponse(data, "OK");
        }

        [HttpPut("{bodyType:int}")]
        public async Task<IActionResult> Upsert(int serviceId, int bodyType, [FromBody] UpsertServiceRecipeRequest request)
        {
            var bt = (CarBodyType)bodyType;
            var data = await _service.UpsertAsync(serviceId, bt, request);
            return OkResponse(data, "Recipe updated");
        }
    }
}
