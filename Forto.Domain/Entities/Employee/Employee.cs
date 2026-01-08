using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Employee
{
    public class Employee : BaseEntity
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string PhoneNumber { get; set; } = "";
        public bool IsActive { get; set; } = true;

        public ICollection<EmployeeWorkSchedule> WorkSchedules { get; set; } = new List<EmployeeWorkSchedule>();

        public ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();


    }

}
