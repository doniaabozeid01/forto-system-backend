using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Employees
{
    public class CreateEmployeeUserRequest
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string PhoneNumber { get; set; } = "";

        public string Password { get; set; } = "";

        // "washer" / "cashier" / "admin"
        public string Role { get; set; } = "";
    }

}
