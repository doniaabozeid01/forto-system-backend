using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Employees.Tasks
{
    public class EmployeeTasksPageResponse
    {
        public int EmployeeId { get; set; }
        public DateOnly Date { get; set; }

        public List<EmployeeTaskResponse> Available { get; set; } = new();
        public List<EmployeeTaskResponse> MyActive { get; set; } = new();
    }
}
