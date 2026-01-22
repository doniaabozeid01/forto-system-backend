using Forto.Domain.Entities.Identity;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Employees
{
    public class Employee : BaseEntity
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string PhoneNumber { get; set; } = "";
        public bool IsActive { get; set; } = true;

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public EmployeeRole Role { get; set; } = EmployeeRole.Worker;

        public ICollection<EmployeeWorkSchedule> WorkSchedules { get; set; } = new List<EmployeeWorkSchedule>();

        public ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();


    }

}
