using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings
{
    public class HourSlotDto
    {
        public TimeOnly Hour { get; set; }   // 09:00, 10:00...
        public int Booked { get; set; }
        public int Available { get; set; }
    }
}
