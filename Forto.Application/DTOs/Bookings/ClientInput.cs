using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings
{
    public class ClientInput
    {
        [Required]
        public string PhoneNumber { get; set; } = "";

        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}
