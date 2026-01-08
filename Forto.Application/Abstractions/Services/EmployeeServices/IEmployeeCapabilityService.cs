using Forto.Application.DTOs.Employees.EmployeeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.EmployeeServices
{

    public interface IEmployeeCapabilityService
    {
        Task<EmployeeServicesResponse> GetAsync(int employeeId);
        Task<EmployeeServicesResponse> UpdateAsync(int employeeId, UpdateEmployeeServicesRequest request);
    }
}
