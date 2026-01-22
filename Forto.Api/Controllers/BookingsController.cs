using Forto.Application.Abstractions.Services.Bookings;
using Forto.Application.Abstractions.Services.Bookings.Admin;
using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Bookings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{

    [Route("api/bookings")]
    public class BookingsController : BaseApiController
    {
        private readonly IBookingService _service;
        private readonly IBookingAdminService _adminService;

        public BookingsController(IBookingService service, IBookingAdminService adminService) {
            _service = service;
            _adminService = adminService;
        }

        [HttpGet("available-slots")]
        public async Task<IActionResult> AvailableSlots([FromQuery] int branchId, [FromQuery] string date,[FromQuery] string serviceIds)
        {
            var parsedDate = DateOnly.Parse(date); // "2026-01-11"
            var ids = serviceIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            var data = await _service.GetAvailableSlotsAsync(branchId, parsedDate, ids);
            return OkResponse(data, "OK");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
        {
            var data = await _service.CreateAsync(request);
            return CreatedResponse(data, "Booking created");
        }




        [HttpPost("quick-create")]
        public async Task<IActionResult> QuickCreate([FromBody] QuickCreateBookingRequest request)
        {
            var data = await _service.QuickCreateAsync(request);
            return CreatedResponse(data, "Booking created");
        }





        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return FailResponse("Booking not found", 404);
            return OkResponse(data, "OK");
        }










        // Admin
        [HttpPost("{bookingId:int}/complete")]
        public async Task<IActionResult> ManualComplete(int bookingId, [FromBody] CashierActionRequest request)
        {
            await _adminService.CompleteBookingAsync(bookingId, request);
            return OkResponse(new { bookingId }, "Booking completed");
        }




        [HttpPost("{bookingId:int}/cancel")]
        public async Task<IActionResult> CancelBooking(int bookingId, [FromBody] CashierActionRequest request)
        {
            await _adminService.CancelBookingAsync(bookingId, request);
            return OkResponse(new { bookingId }, "Booking cancelled");
        }





        [HttpPut("{bookingId:int}/assign")]
        public async Task<IActionResult> Assign(int bookingId, [FromBody] AssignBookingEmployeesRequest request)
        {
            var data = await _adminService.AssignEmployeesAsync(bookingId, request);
            return OkResponse(data, "Employees assigned");
        }

    }

}
