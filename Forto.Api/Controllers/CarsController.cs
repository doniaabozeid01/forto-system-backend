using Forto.Application.Abstractions.Services.Cars;
using Forto.Application.DTOs.Cars;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/")]

    public class CarsController : BaseApiController
    {
        private readonly ICarService _service;

        public CarsController(ICarService service) => _service = service;

        // Add Car to Client
        [HttpPost("clients/{clientId:int}/addCars")]
        public async Task<IActionResult> Add(int clientId, [FromBody] CreateCarRequest request)
        {
            var data = await _service.AddToClientAsync(clientId, request);
            return CreatedResponse(data, "Car created");
        }

        // Update Car
        [HttpPut("clients/UpdateCars/{carId:int}")]
        public async Task<IActionResult> Update(int carId, [FromBody] UpdateCarRequest request)
        {
            var data = await _service.UpdateAsync(carId, request);
            if (data == null) return FailResponse("Car not found", 404);
            return OkResponse(data, "Car updated");
        }

        // Delete Car (soft)
        [HttpDelete("clients/DeleteCar/{carId:int}")]
        public async Task<IActionResult> Delete(int carId)
        {
            var ok = await _service.DeleteAsync(carId);
            if (!ok) return FailResponse("Car not found", 404);
            return OkResponse(new { id = carId }, "Car deleted");
        }

        // Set Default Car
        [HttpPut("clients/{clientId:int}/cars/{carId:int}/default")]
        public async Task<IActionResult> SetDefault(int clientId, int carId)
        {
            var data = await _service.SetDefaultAsync(clientId, carId);
            if (data == null) return FailResponse("Car not found for this client", 404);
            return OkResponse(data, "Default car updated");
        }
    }
}