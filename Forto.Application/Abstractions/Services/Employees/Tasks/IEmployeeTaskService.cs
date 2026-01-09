using Forto.Application.DTOs.Employees.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Employees.Tasks
{
    public interface IEmployeeTaskService
    {
        Task<EmployeeTasksPageResponse> GetTasksAsync(int employeeId, DateOnly date);
    }
}
