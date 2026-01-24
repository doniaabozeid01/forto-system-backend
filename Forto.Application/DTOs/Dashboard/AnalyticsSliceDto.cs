using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Dashboard
{
    public class AnalyticsSliceDto
    {
        public int Id { get; set; }              // ServiceId or EmployeeId
        public string Name { get; set; } = "";   // ServiceName or EmployeeName
        public int Count { get; set; }
        public decimal Percent { get; set; }     // 0..100
    }
}
