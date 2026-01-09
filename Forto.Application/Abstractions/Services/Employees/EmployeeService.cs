using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Employees;
using Forto.Domain.Entities.Employee;

namespace Forto.Application.Abstractions.Services.Employees
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _uow;

        public EmployeeService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request)
        {
            var repo = _uow.Repository<Employee>();

            // Business rule مثال: رقم التليفون ميكونش مكرر
            var exists = await repo.AnyAsync(x => x.PhoneNumber == request.PhoneNumber);
            if (exists)
                throw new BusinessException(
                    "Phone number already exists",
                    409,
                    new Dictionary<string, string[]>
                    {
                        ["phoneNumber"] = new[] { "This phone number is already used." }
                    });

            var employee = new Employee
            {
                Name = request.Name.Trim(),
                Age = request.Age,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            await repo.AddAsync(employee);
            await _uow.SaveChangesAsync();

            return Map(employee);
        }

        public async Task<EmployeeResponse?> GetByIdAsync(int id)
        {
            var employee = await _uow.Repository<Employee>().GetByIdAsync(id);
            return employee == null ? null : Map(employee);
        }

        public async Task<IReadOnlyList<EmployeeResponse>> GetAllAsync()
        {
            var employees = await _uow.Repository<Employee>().GetAllAsync();
            return employees.Select(Map).ToList();
        }

        public async Task<EmployeeResponse?> UpdateAsync(int id, UpdateEmployeeRequest request)
        {
            var repo = _uow.Repository<Employee>();
            var employee = await repo.GetByIdAsync(id);
            if (employee == null) return null;

            employee.Name = request.Name.Trim();
            employee.Age = request.Age;
            employee.PhoneNumber = request.PhoneNumber;
            employee.IsActive = request.IsActive;

            repo.Update(employee);
            await _uow.SaveChangesAsync();

            return Map(employee);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Employee>();
            var employee = await repo.GetByIdAsync(id);
            if (employee == null) return false;

            repo.Delete(employee); // soft delete
            await _uow.SaveChangesAsync();
            return true;
        }

        private static EmployeeResponse Map(Employee e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            Age = e.Age,
            PhoneNumber = e.PhoneNumber,
            IsActive = e.IsActive
        };
    }
}
