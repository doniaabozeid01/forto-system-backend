using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings
{
    public class StartBookingItemRequest
    {
        [Required]
        public int EmployeeId { get; set; }
    }
}
