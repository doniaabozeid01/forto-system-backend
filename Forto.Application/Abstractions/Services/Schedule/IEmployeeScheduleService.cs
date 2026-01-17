using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Schedule;

namespace Forto.Application.Abstractions.Services.Schedule
{
    public interface IEmployeeScheduleService
    {
        Task<EmployeeScheduleResponse> UpsertWeekAsync(int employeeId, UpsertEmployeeScheduleRequest request);
        Task<EmployeeScheduleResponse> GetWeekAsync(int employeeId);
        Task<bool> IsEmployeeWorkingAsync(int employeeId, DateTime dateTime);

    }
}
