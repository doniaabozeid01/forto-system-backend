using Forto.Application.Abstractions.Services.Ops.Stock;
using Forto.Application.DTOs.Ops.Stock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/branches/{branchId:int}/stock")]
    public class BranchStockController : BaseApiController
    {

        private readonly IBranchStockService _service;

        public BranchStockController(IBranchStockService service) => _service = service;



        [HttpGet("GetBranchStock")]
        public async Task<IActionResult> Get(int branchId)
            => OkResponse(await _service.GetBranchStockAsync(branchId), "OK");



        [HttpPut("Upsert")]
        public async Task<IActionResult> Upsert(int branchId, [FromBody] UpsertBranchStockRequest request)
            => OkResponse(await _service.UpsertAsync(branchId, request), "Stock updated");
        


    }
}
