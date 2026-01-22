using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Bookings
{
    public class CreateBookingRequest
    {
        [Required]
        public int BranchId { get; set; }  // دلوقتي هتبقى main branch id

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        public DateTime ScheduledStart { get; set; } // على ساعة كاملة

        [Required, MinLength(1)]
        public List<int> ServiceIds { get; set; } = new();



        public BookingCreatedByType CreatedByType { get; set; }
        public int? CreatedByEmployeeId { get; set; }
        public int? CreatedByClientId { get; set; }


        public string? Notes { get; set; }
    }
}
