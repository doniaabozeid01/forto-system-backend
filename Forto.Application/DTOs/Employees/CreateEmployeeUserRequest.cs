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

        // "worker" / "cashier" / "supervisor" / "admin" — Auth مسموح لـ cashier, supervisor, admin فقط
        public string Role { get; set; } = "";
    }

}
