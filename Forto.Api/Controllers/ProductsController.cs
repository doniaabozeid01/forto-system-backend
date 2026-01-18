using Forto.Application.Abstractions.Services.Inventory.Products;
using Forto.Application.DTOs.Inventory.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/products")]
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service) => _service = service;

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
            => CreatedResponse(await _service.CreateAsync(request), "Product created");

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
            => OkResponse(await _service.GetAllAsync(), "OK");

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return FailResponse("Product not found", 404);
            return OkResponse(data, "OK");
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
        {
            var data = await _service.UpdateAsync(id, request);
            if (data == null) return FailResponse("Product not found", 404);
            return OkResponse(data, "Product updated");
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return FailResponse("Product not found", 404);
            return OkResponse(new { id }, "Product deleted");
        }
    }

}
