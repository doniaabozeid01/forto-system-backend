using Forto.Application.Abstractions.Services.Catalogs.Service;
using Forto.Application.DTOs;
using Forto.Application.DTOs.Catalog.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

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

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceRequest request)
        {
            var data = await _service.UpdateServiceAsync(id, request);
            if (data == null) return FailResponse("Service not found", 404);
            return OkResponse(data, "Service updated");
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


        [HttpGet("{id:int}/employees")]
        public async Task<IActionResult> GetEmployeesByAServiceId(int id)
        {
            var data = await _service.GetEmployeesForServiceAsync(id);
            return OkResponse(data, "OK");
        }



        [HttpGet("bookings/{bookingId:int}/services/{serviceId:int}/employees")]
        public async Task<IActionResult> GetEmployees(
            int bookingId,
            int serviceId,
            [FromQuery] DateTime scheduledStart)
        {
            var data = await _service.GetEmployeesForServiceAtAsync(bookingId, serviceId, scheduledStart);
            return OkResponse(data, "OK");
        }

        /// <summary>الهدايا المتاحة بناءً على قائمة خدمات — اختياري: branchId لعرض المخزون.</summary>
        [HttpGet("gift-options")]
        public async Task<IActionResult> GetGiftOptionsByServices([FromQuery] string? serviceIds, [FromQuery] int? branchId = null)
        {
            var ids = string.IsNullOrWhiteSpace(serviceIds)
                ? new List<int>()
                : serviceIds.Split(',').Select(s => int.TryParse(s.Trim(), out var n) ? n : 0).Where(n => n > 0).ToList();
            var data = await _service.GetGiftOptionsByServiceIdsAsync(ids, branchId);
            return OkResponse(data, "OK");
        }

        /// <summary>قائمة الهدايا (منتجات) المربوطة بخدمة معينة.</summary>
        [HttpGet("{serviceId:int}/gift-options")]
        public async Task<IActionResult> GetGiftOptionsForService(int serviceId)
        {
            var data = await _service.GetGiftOptionsForServiceAsync(serviceId);
            return OkResponse(data, "OK");
        }

        /// <summary>إضافة هدايا لخدمة — body: productIds فقط (حتى لو منتج واحد).</summary>
        [HttpPost("{serviceId:int}/gift-options")]
        public async Task<IActionResult> AddGiftOptionToService(int serviceId, [FromBody] AddGiftOptionToServiceRequest request)
        {
            if (request?.ProductIds == null || request.ProductIds.Count == 0)
                return FailResponse("productIds is required (at least one)", 400);
            var data = await _service.AddGiftOptionsToServiceAsync(serviceId, request.ProductIds);
            return CreatedResponse(data, "Gift options added");
        }

        /// <summary>إزالة هدايا من الخدمة — body: productIds (قائمة).</summary>
        [HttpDelete("{serviceId:int}/gift-options")]
        public async Task<IActionResult> RemoveGiftOptionsFromService(int serviceId, [FromBody] RemoveGiftOptionsFromServiceRequest request)
        {
            if (request?.ProductIds == null || request.ProductIds.Count == 0)
                return FailResponse("productIds is required (at least one)", 400);
            var removed = await _service.RemoveGiftOptionsFromServiceAsync(serviceId, request.ProductIds);
            return OkResponse(new { serviceId, removedCount = removed }, "Gift options removed");
        }
    }
}
