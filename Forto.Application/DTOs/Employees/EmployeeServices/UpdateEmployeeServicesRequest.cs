using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Employees.EmployeeServices
{
    public class UpdateEmployeeServicesRequest
    {
        [Required]
        public List<int> ServiceIds { get; set; } = new();
    }
}
