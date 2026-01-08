using Forto.Application.Abstractions.Services.Catalogs.Categories;
using Forto.Application.DTOs.Catalog.Categories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/catalog/categories")]
    public class CategoriesController : BaseApiController
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service) => _service = service;

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var data = await _service.CreateAsync(request);
            return CreatedResponse(data, "Category created");
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] int? parentId = null)
        {
            var data = await _service.GetAllAsync(parentId);
            return OkResponse(data, "OK");
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return FailResponse("Category not found", 404);
            return OkResponse(data, "OK");
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
        {
            var data = await _service.UpdateAsync(id, request);
            if (data == null) return FailResponse("Category not found", 404);
            return OkResponse(data, "Category updated");
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return FailResponse("Category not found", 404);
            return OkResponse(new { id }, "Category deleted");
        }
    }

}
