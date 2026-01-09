using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Employees
{
    public class UpdateEmployeeRequest
    {
        [Required, MinLength(3)]
        public string Name { get; set; } = "";

        [Range(16, 80)]
        public int Age { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; } = "";

        public bool IsActive { get; set; }
    }
}
