using Forto.Application.Abstractions.Services.Inventory.ProductCategories;
using Forto.Application.DTOs.Inventory.ProductCategories;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/inventory/product-categories")]
    public class ProductCategoriesController : BaseApiController
    {
        private readonly IProductCategoryService _service;

        public ProductCategoriesController(IProductCategoryService service) => _service = service;

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest request)
            => CreatedResponse(await _service.CreateAsync(request), "Product category created");

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] int? parentId = null)
            => OkResponse(await _service.GetAllAsync(parentId), "OK");

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return FailResponse("Product category not found", 404);
            return OkResponse(data, "OK");
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCategoryRequest request)
        {
            var data = await _service.UpdateAsync(id, request);
            if (data == null) return FailResponse("Product category not found", 404);
            return OkResponse(data, "Product category updated");
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return FailResponse("Product category not found", 404);
            return OkResponse(new { id }, "Product category deleted");
        }
    }
}
