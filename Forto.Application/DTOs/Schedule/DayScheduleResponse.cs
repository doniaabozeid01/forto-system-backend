using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Schedule
{
    public class DayScheduleResponse
    {
        public int DayOfWeek { get; set; }
        public bool IsOff { get; set; }

        public int? ShiftId { get; set; }
        public string? ShiftName { get; set; }

        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }
}
