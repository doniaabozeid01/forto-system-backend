using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Employee
{
    public class EmployeeWorkSchedule : BaseEntity
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }

        public int? ShiftId { get; set; }
        public Shift? Shift { get; set; }

        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public bool IsOff { get; set; } = false;
    }

}
