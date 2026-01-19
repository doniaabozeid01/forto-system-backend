using Forto.Application.Abstractions.Services.Ops.Products.StockMovement;
using Forto.Application.DTOs.Ops.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/branches/{branchId:int}/products/stock")]
    public class ProductStockMovementsController : BaseApiController
    {
        private readonly IProductStockMovementService _service;
        public ProductStockMovementsController(IProductStockMovementService service) => _service = service;

        [HttpPost("in")]
        public async Task<IActionResult> StockIn(int branchId, [FromBody] StockInProductRequest request)
        {
            await _service.StockInAsync(branchId, request);
            return OkResponse(new { branchId }, "Product stock IN recorded");
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> Adjust(int branchId, [FromBody] AdjustProductStockRequest request)
        {
            await _service.AdjustAsync(branchId, request);
            return OkResponse(new { branchId }, "Product stock adjusted");
        }
    }

}
