using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Bookings
{
    public class BookingServiceOptionsResponse
    {
        public int BookingId { get; set; }

        public List<ServiceInBookingDto> InBooking { get; set; } = new();
        public List<ServiceOptionDto> NotInBooking { get; set; } = new();
    }

    public class ServiceInBookingDto
    {
        public int BookingItemId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";

        public BookingItemStatus Status { get; set; }
        public decimal UnitPrice { get; set; }
        public int DurationMinutes { get; set; }

        public int? AssignedEmployeeId { get; set; }
    }

    public class ServiceOptionDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";
    }

}
