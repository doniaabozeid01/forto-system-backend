using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Employees;

namespace Forto.Application.Abstractions.Services.Employees
{
    public interface IEmployeeService
    {
        Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request);
        Task<EmployeeResponse?> GetByIdAsync(int id);
        Task<IReadOnlyList<EmployeeResponse>> GetAllAsync();
        Task<EmployeeResponse?> UpdateAsync(int id, UpdateEmployeeRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
