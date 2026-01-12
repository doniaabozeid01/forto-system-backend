using Forto.Application.Abstractions.Services.Inventory.Materials;
using Forto.Application.DTOs.Inventory.Materials;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/materials")]
    public class MaterialsController : BaseApiController
    {
        private readonly IMaterialService _service;
        public MaterialsController(IMaterialService service) => _service = service;

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateMaterialRequest request)
            => CreatedResponse(await _service.CreateAsync(request), "Material created");

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
            => OkResponse(await _service.GetAllAsync(), "OK");

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return FailResponse("Material not found", 404);
            return OkResponse(data, "OK");
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMaterialRequest request)
        {
            var data = await _service.UpdateAsync(id, request);
            if (data == null) return FailResponse("Material not found", 404);
            return OkResponse(data, "Material updated");
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return FailResponse("Material not found", 404);
            return OkResponse(new { id }, "Material deleted");
        }
    }
}
