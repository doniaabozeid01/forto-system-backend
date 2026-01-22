using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings
{
    public class AssignBookingEmployeesRequest
    {
        public int CashierId { get; set; }
        public List<BookingItemAssignmentDto> Assignments { get; set; } = new();
    }
}
