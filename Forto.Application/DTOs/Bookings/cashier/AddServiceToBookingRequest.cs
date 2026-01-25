using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings.cashier
{
    public class AddServiceToBookingRequest
    {
        public int CashierId { get; set; }
        public int ServiceId { get; set; }
        public int AssignedEmployeeId { get; set; } // لازم نعرف مين هيعملها
    }

}
