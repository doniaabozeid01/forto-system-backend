using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings
{
    public class BookingItemResponse
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public CarBodyType BodyType { get; set; }
        public decimal UnitPrice { get; set; }
        public int DurationMinutes { get; set; }

        public BookingItemStatus Status { get; set; }

        public int? AssignedEmployeeId { get; set; }
    }
}
