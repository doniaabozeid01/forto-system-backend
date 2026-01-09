using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings
{
    public class AvailableSlotsResponse
    {
        public DateOnly Date { get; set; }
        public int CapacityPerHour { get; set; }
        public List<HourSlotDto> Slots { get; set; } = new();
    }
}
