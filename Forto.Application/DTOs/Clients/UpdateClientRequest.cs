using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Clients
{
    public class UpdateClientRequest
    {
        [Required, MinLength(3)]
        public string FullName { get; set; } = "";

        [Required]
        public string PhoneNumber { get; set; } = "";

        public string? Email { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
