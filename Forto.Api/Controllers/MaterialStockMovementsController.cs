using Forto.Application.Abstractions.Services.Ops.Stock.StockMovement;
using Forto.Application.DTOs.Ops.Stock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{


    [Route("api/branches/{branchId:int}/stock")]
    public class MaterialStockMovementsController : BaseApiController
    {
        private readonly IStockMovementService _service;
        public MaterialStockMovementsController(IStockMovementService service) => _service = service;

        [HttpPost("in")]
        public async Task<IActionResult> StockIn(int branchId, [FromBody] StockInRequest request)
        {
            await _service.StockInAsync(branchId, request);
            return OkResponse(new { branchId }, "Stock IN recorded");
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> Adjust(int branchId, [FromBody] StockAdjustRequest request)
        {
            await _service.StockAdjustAsync(branchId, request);
            return OkResponse(new { branchId }, "Stock adjusted");
        }
    }

}
