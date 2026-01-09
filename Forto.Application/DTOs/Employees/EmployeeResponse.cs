using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Employees
{
    public class EmployeeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string PhoneNumber { get; set; } = "";
        public bool IsActive { get; set; }
    }
}
