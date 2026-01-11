using Forto.Application.Abstractions.Services.Clients;
using Forto.Application.DTOs.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/clients")]
    public class ClientsController : BaseApiController
    {
        private readonly IClientService _service;

        public ClientsController(IClientService service) => _service = service;

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
        {
            var data = await _service.CreateAsync(request);
            return CreatedResponse(data, "Client created");
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return OkResponse(data, "OK");
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return FailResponse("Client not found", 404);
            return OkResponse(data, "OK");
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClientRequest request)
        {
            var data = await _service.UpdateAsync(id, request);
            if (data == null) return FailResponse("Client not found", 404);
            return OkResponse(data, "Client updated");
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return FailResponse("Client not found", 404);
            return OkResponse(new { id }, "Client deleted");
        }

        // SEARCH / LOOKUP (B)
        [HttpGet("lookup")]
        public async Task<IActionResult> Search([FromQuery] string phone, [FromQuery] int take = 10)
        {
            var data = await _service.SearchByPhoneAsync(phone, take);
            if (data == null) return OkResponse<object?>(null, "Client not found");
            return OkResponse(data, "OK");
        }
    }

}
