using Forto.Application.Abstractions.Services.Employees.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/employees/{employeeId:int}/tasks")]
    public class EmployeeTasksController : BaseApiController
    {
        private readonly IEmployeeTaskService _service;

        public EmployeeTasksController(IEmployeeTaskService service) => _service = service;

        [HttpGet("GetEmployeeTasks")]
        public async Task<IActionResult> Get(int employeeId, [FromQuery] string date)
        {
            var parsed = DateOnly.Parse(date); // "2026-01-11"
            var data = await _service.GetTasksAsync(employeeId, parsed);
            return OkResponse(data, "OK");
        }
    }

}
