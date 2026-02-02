using Forto.Application.Abstractions.Services.Employees;
using Forto.Application.DTOs.Catalog.Recipes;
using Forto.Application.DTOs.Employees;
using Forto.Domain.Entities.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/employees")]
    public class EmployeesController : BaseApiController
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
        {
            var result = await _employeeService.CreateAsync(request);
            return CreatedResponse(result, "Employee created");
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _employeeService.GetAllAsync();
            return OkResponse(data);
        }

        [HttpGet("supervisors")]
        public async Task<IActionResult> GetSupervisors()
        {
            var data = await _employeeService.GetSupervisorsAsync();
            return OkResponse(data);
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _employeeService.GetByIdAsync(id);
            if (data == null)
                return FailResponse("Employee not found", 404);

            return OkResponse(data);
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest request)
        {
            var data = await _employeeService.UpdateAsync(id, request);
            if (data == null)
                return FailResponse("Employee not found", 404);

            return OkResponse(data, "Employee updated");
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _employeeService.DeleteAsync(id);
            if (!ok)
                return FailResponse("Employee not found", 404);

            return OkResponse(new {}, "Employee deleted");
        }



        [HttpPost("admin/employees/create-user")]
        public async Task<IActionResult> CreateEmployeeUser([FromBody] CreateEmployeeUserRequest req)
        {
            var data = await _employeeService.CreateEmployeeUserAsync(req);
            return OkResponse(data, "Employee user created");
        }









    }
}
