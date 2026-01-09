using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Employees.Tasks
{
    public class EmployeeTaskResponse
    {
        public int BookingItemId { get; set; }
        public int BookingId { get; set; }

        public DateTime ScheduledStart { get; set; }

        public int ClientId { get; set; }
        public int CarId { get; set; }
        public string PlateNumber { get; set; } = "";

        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";

        public BookingItemStatus ItemStatus { get; set; }
    }
}
