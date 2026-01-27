using Forto.Application.Abstractions.Services.Catalogs.Service;
using Forto.Application.DTOs;
using Forto.Application.DTOs.Catalog.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/catalog/services")]
    public class CatalogServicesController : BaseApiController
    {
        private readonly ICatalogService _service;

        public CatalogServicesController(ICatalogService service) => _service = service;

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
        {
            var data = await _service.CreateServiceAsync(request);
            return CreatedResponse(data, "Service created");
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] int? categoryId = null)
        {
            var data = await _service.GetServicesAsync(categoryId);
            return OkResponse(data, "OK");
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetServiceAsync(id);
            if (data == null) return FailResponse("Service not found", 404);
            return OkResponse(data, "OK");
        }

        [HttpPut("UpsertRates/{id:int}/rates")]
        public async Task<IActionResult> UpsertRates(int id, [FromBody] UpsertServiceRatesRequest request)
        {
            var data = await _service.UpsertRatesAsync(id, request);
            if (data == null) return FailResponse("Service not found", 404);
            return OkResponse(data, "Rates updated");
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteServiceAsync(id);
            if (!ok) return FailResponse("Service not found", 404);
            return OkResponse(new { id }, "Service deleted");
        }


        //[HttpGet("{id:int}/employees")]
        //public async Task<IActionResult> GetEmployees(int id)
        //{
        //    var data = await _service.GetEmployeesForServiceAsync(id);
        //    return OkResponse(data, "OK");
        //}



        [HttpGet("bookings/{bookingId:int}/services/{serviceId:int}/employees")]
        public async Task<IActionResult> GetEmployees(
            int bookingId,
            int serviceId,
            [FromQuery] DateTime scheduledStart)
        {
            var data = await _service.GetEmployeesForServiceAtAsync(bookingId, serviceId, scheduledStart);
            return OkResponse(data, "OK");
        }


    }

}
