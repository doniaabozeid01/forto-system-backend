using Forto.Application.Abstractions.Services.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/dashboard")]
    public class DashboardController : BaseApiController
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service) => _service = service;

        [HttpGet("summary")]
        public async Task<IActionResult> Summary ([FromQuery] int branchId, [FromQuery] string from, [FromQuery] string to)
        {
            var fromDate = DateOnly.Parse(from); // "2026-01-01"
            var toDate = DateOnly.Parse(to);     // "2026-01-31"

            var data = await _service.GetSummaryAsync(branchId, fromDate, toDate);
            return OkResponse(data, "OK");
        }












            [HttpGet("services")]
            public async Task<IActionResult> Services([FromQuery] int branchId, [FromQuery] string from, [FromQuery] string to)
            {
                var f = DateOnly.Parse(from);
                var t = DateOnly.Parse(to);
                var data = await _service.GetTopServicesAsync(branchId, f, t);
                return OkResponse(data, "OK");
            }

            [HttpGet("employees")]
            public async Task<IActionResult> Employees([FromQuery] int branchId, [FromQuery] string from, [FromQuery] string to)
            {
                var f = DateOnly.Parse(from);
                var t = DateOnly.Parse(to);
                var data = await _service.GetTopEmployeesAsync(branchId, f, t);
                return OkResponse(data, "OK");
            }
        
    }

}
