using Forto.Application.Abstractions.Services.EmployeeServices;
using Forto.Application.DTOs.Employees.EmployeeServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/employees/{employeeId:int}/services")]
    public class EmployeeServicesController : BaseApiController
    {
        private readonly IEmployeeCapabilityService _service;

        public EmployeeServicesController(IEmployeeCapabilityService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int employeeId)
        {
            var data = await _service.GetAsync(employeeId);
            return OkResponse(data, "OK");
        }

        [HttpPut]
        public async Task<IActionResult> Update(int employeeId, [FromBody] UpdateEmployeeServicesRequest request)
        {
            var data = await _service.UpdateAsync(employeeId, request);
            return OkResponse(data, "Employee services updated");
        }
    }

}
