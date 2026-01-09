using Forto.Application.Abstractions.Services.Schedule;
using Forto.Application.DTOs.Schedule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/employees/{employeeId:int}/schedule")]
    public class EmployeeSchedulesController : BaseApiController
    {
        private readonly IEmployeeScheduleService _service;

        public EmployeeSchedulesController(IEmployeeScheduleService service)
        {
            _service = service;
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get(int employeeId)
        {
            var data = await _service.GetWeekAsync(employeeId);
            return OkResponse(data, "OK");
        }

        [HttpPut("Upsert")]
        public async Task<IActionResult> Upsert(int employeeId, [FromBody] UpsertEmployeeScheduleRequest request)
        {
            var data = await _service.UpsertWeekAsync(employeeId, request);
            return OkResponse(data, "Schedule updated");
        }
    }
}
