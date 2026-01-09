using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Schedule
{
    public class DayScheduleRequest
    {
        /// <summary>0=Sunday ... 6=Saturday</summary>
        [Range(0, 6)]
        public int DayOfWeek { get; set; }

        public bool IsOff { get; set; }

        public int? ShiftId { get; set; }

        // لو مش هتستخدمي Shift
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }
}
