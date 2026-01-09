using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Schedule
{
    public class EmployeeScheduleResponse
    {
        public int EmployeeId { get; set; }
        public List<DayScheduleResponse> Days { get; set; } = new();
    }

}
