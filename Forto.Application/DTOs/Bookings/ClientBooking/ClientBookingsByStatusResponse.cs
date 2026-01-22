using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Bookings.ClientBooking
{
    public class ClientBookingsByStatusResponse
    {
        public List<BookingListItemDto> Pending { get; set; } = new();
        public List<BookingListItemDto> InProgress { get; set; } = new();
        public List<BookingListItemDto> Completed { get; set; } = new();
        public List<BookingListItemDto> Cancelled { get; set; } = new();
    }

    public class BookingListItemDto
    {
        public int BookingId { get; set; }
        public DateTime ScheduledStart { get; set; }
        public decimal TotalPrice { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public BookingStatus Status { get; set; }

        public string PlateNumber { get; set; } = "";
        public int CarId { get; set; }

        // optional summary
        public int ServicesCount { get; set; }
    }

}
