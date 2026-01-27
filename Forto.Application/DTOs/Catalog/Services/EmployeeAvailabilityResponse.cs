using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Services
{
    public class EmployeeAvailabilityResponse
    {
        public int BookingId { get; set; }
        public int ServiceId { get; set; }
        public DateTime SlotHourStart { get; set; }

        public List<EmployeeSimpleDto> AvailableEmployees { get; set; } = new();
        public List<BusyEmployeeDto> BusyEmployees { get; set; } = new();
    }

    public class EmployeeSimpleDto
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = "";
    }

    public class BusyEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = "";
        public int BusyBookingId { get; set; }
        public string Reason { get; set; } = "";
    }

}
