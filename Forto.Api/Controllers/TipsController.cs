using Forto.Application.Abstractions.Services.Billings.Tips;
using Forto.Application.DTOs.Billings;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/billing/tips")]
    public class TipsController : BaseApiController
    {
        private readonly ITipsService _service;

        public TipsController(ITipsService service) => _service = service;

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateTipRequest request)
        {
            var data = await _service.CreateAsync(request);
            return CreatedResponse(data, "Tip created");
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            var data = await _service.GetAllAsync(fromDate, toDate);
            return OkResponse(data, "OK");
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return FailResponse("Tip not found", 404);
            return OkResponse(data, "OK");
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTipRequest request)
        {
            var data = await _service.UpdateAsync(id, request);
            if (data == null) return FailResponse("Tip not found", 404);
            return OkResponse(data, "Tip updated");
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return FailResponse("Tip not found", 404);
            return OkResponse(new { id }, "Tip deleted");
        }
    }
}
