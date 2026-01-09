using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }

        public DateTime ScheduledStart { get; set; }
        public DateTime SlotHourStart { get; set; }

        public decimal TotalPrice { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        public BookingStatus Status { get; set; }

        public List<BookingItemResponse> Items { get; set; } = new();
    }
}
