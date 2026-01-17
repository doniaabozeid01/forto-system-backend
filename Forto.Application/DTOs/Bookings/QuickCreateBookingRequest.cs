using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings
{
    public class QuickCreateBookingRequest
    {
        [Required]
        public int BranchId { get; set; }

        [Required]
        public DateTime ScheduledStart { get; set; } // على الساعة

        [Required, MinLength(1)]
        public List<int> ServiceIds { get; set; } = new();

        [Required]
        public ClientInput Client { get; set; } = new();

        [Required]
        public CarInput Car { get; set; } = new();

        public int? AssignedEmployeeId { get; set; }

        public string? Notes { get; set; }
    }
}
