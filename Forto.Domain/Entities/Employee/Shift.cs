using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Employee
{
    public class Shift : BaseEntity
    {
        public string Name { get; set; } = "";
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public ICollection<EmployeeWorkSchedule> EmployeeSchedules { get; set; } = new List<EmployeeWorkSchedule>();
    }

}
