using Azure.Core;
using Forto.Api.Common;
using Forto.Application.Abstractions.Services.Shift;
using Forto.Application.DTOs.Shifts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/shifts")]
    public class ShiftsController : BaseApiController
    {
        private readonly IShiftService _shiftService;

        public ShiftsController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateShiftRequest request)
        {
            
            var created = await _shiftService.CreateAsync(request);
            return CreatedResponse(created, "Created");
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _shiftService.GetAllAsync();
            return OkResponse(data, "OK");
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _shiftService.GetByIdAsync(id);
            if (data == null)
                return FailResponse("Not found", 404);

            return OkResponse(data, "OK");
        }


        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, CreateShiftRequest request)
        {
            var updated = await _shiftService.UpdateAsync(id, request);
            if (updated == null)
                return FailResponse("Not found", 404);

            return OkResponse(updated, "Updated");
        }


        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _shiftService.DeleteAsync(id);
            if (!ok)
                return FailResponse("Not found", 404);

            return OkResponse(new {} , "Deleted");
        }
    }
}
