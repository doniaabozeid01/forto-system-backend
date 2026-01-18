
using Forto.Application.Abstractions.Services.Ops.Products;
using Forto.Application.DTOs.Ops.Products;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/")]
    public class BranchProductStockController : BaseApiController
    {
        private readonly IBranchProductStockService _service;

        public BranchProductStockController(IBranchProductStockService service) { 
            _service = service; 
        }

        [HttpPut("UpsertBranchProductStock")]
        public async Task<IActionResult> Upsert(int branchId, [FromBody] UpsertBranchProductStockRequest request)
        { 
            return OkResponse(await _service.UpsertAsync(branchId, request), "Stock updated"); 
        }

        [HttpGet("GetBranchProductStock/{branchId}")]
        public async Task<IActionResult> GetBranchStock(int branchId)
        { 
            return OkResponse(await _service.GetBranchStockAsync(branchId), "OK");
        }

    }

}
