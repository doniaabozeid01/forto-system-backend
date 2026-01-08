using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Employees.EmployeeServices
{
    public class EmployeeServicesResponse
    {
        public int EmployeeId { get; set; }
        public List<int> ServiceIds { get; set; } = new();
    }
}
