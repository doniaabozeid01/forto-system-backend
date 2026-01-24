using Forto.Application.Abstractions.Services.Catalogs.Recipes.MaterialApprove;
using Forto.Application.DTOs.Catalog.Recipes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/booking-items/")]
    public class MaterialRequestsController : BaseApiController
    {
        private readonly IMaterialApprovalService _service;
        public MaterialRequestsController(IMaterialApprovalService service) => _service = service;

        [HttpPost("{bookingItemId:int}/materials/requests")]
        public async Task<IActionResult> Create(int bookingItemId, [FromBody] CreateMaterialChangeRequestDto dto)
        {
            var id = await _service.CreateRequestAsync(bookingItemId, dto);
            return OkResponse(new { requestId = id, status = "Pending" }, "Request created");
        }



        [HttpPost("{bookingItemId:int}/materials/requests/{requestId:int}/approve")]
        public async Task<IActionResult> Approve(int requestId, [FromBody] ReviewMaterialChangeRequestDto dto)
        {
            await _service.ApproveAsync(requestId, dto);
            return OkResponse(new { requestId }, "Approved");
        }

        [HttpPost("{bookingItemId:int}/materials/requests/{requestId:int}/reject")]
        public async Task<IActionResult> Reject(int requestId, [FromBody] ReviewMaterialChangeRequestDto dto)
        {
            await _service.RejectAsync(requestId, dto);
            return OkResponse(new { requestId }, "Rejected");
        }



            [HttpGet("pending")]
            public async Task<IActionResult> Pending([FromQuery] int branchId, [FromQuery] string date)
            {
                var d = DateOnly.Parse(date);
                var data = await _service.ListPendingAsync(branchId, d);
                return OkResponse(data, "OK");
            }
        


    }
}
