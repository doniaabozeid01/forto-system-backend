using Forto.Application.Abstractions.Services.Inventory.MaterialsCheck;
using Forto.Application.DTOs.Inventory.MaterialsCheck;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/branches/{branchId:int}/materials")]
    public class MaterialsCheckController : BaseApiController
    {
        private readonly IMaterialsCheckService _service;

        public MaterialsCheckController(IMaterialsCheckService service) => _service = service;

        [HttpPost("check")]
        public async Task<IActionResult> Check(int branchId, [FromBody] MaterialsCheckRequest request)
        {
            var data = await _service.CheckAsync(branchId, request);
            return OkResponse(data, "OK");
        }
    }
}
