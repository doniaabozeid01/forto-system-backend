using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Employees;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forto.Application.Abstractions.Services.Employees
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
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
                IsActive = true,
                Role = request.Role,
            };

            await repo.AddAsync(employee);
            await _uow.SaveChangesAsync();

            return Map(employee);
        }




        /// <summary>
        /// إنشاء موظف + حساب دخول (Auth). مسموح فقط لـ Cashier أو Admin — لو عامل لا تستدعِ هذا؛ استخدم Create فقط.
        /// </summary>
        public async Task<EmployeeResponse> CreateEmployeeUserAsync(CreateEmployeeUserRequest req)
        {
            var role = req.Role.Trim().ToLowerInvariant();
            var authAllowed = new[] { "cashier", "admin", "supervisor" };
            if (!authAllowed.Contains(role))
                throw new BusinessException("Auth (login) is only for Cashier, Supervisor or Admin. For Worker use Create employee without user.", 400);

            var phone = req.PhoneNumber.Trim();

            // 1) prevent duplicate employee phone
            var empRepo = _uow.Repository<Employee>();
            if (await empRepo.AnyAsync(e => e.PhoneNumber == phone))
                throw new BusinessException("Phone number already exists", 409);

            // 2) prevent duplicate identity phone
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (existingUser != null)
                throw new BusinessException("Phone number already registered", 409);

            // 3) create employee first
            var employee = new Employee
            {
                Name = req.Name.Trim(),
                Age = req.Age,
                PhoneNumber = phone,
                IsActive = true,
                Role = role == "worker" ? Domain.Enum.EmployeeRole.Worker :
                       role == "cashier" ? Domain.Enum.EmployeeRole.Cashier :
                       role == "supervisor" ? Domain.Enum.EmployeeRole.Supervisor :
                       Domain.Enum.EmployeeRole.Admin
            };

            await empRepo.AddAsync(employee);
            await _uow.SaveChangesAsync(); // get employee.Id

            // 4) create identity user
            var user = new ApplicationUser
            {
                UserName = phone,
                PhoneNumber = phone,
                PhoneNumberConfirmed = true,
            };

            var createUser = await _userManager.CreateAsync(user, req.Password);
            if (!createUser.Succeeded)
            {
                // rollback employee (soft delete or hard delete) – MVP simplest:
                empRepo.Delete(employee);
                await _uow.SaveChangesAsync();

                throw new BusinessException("Failed to create user", 400,
                    new Dictionary<string, string[]>
                    {
                        ["identity"] = createUser.Errors.Select(e => e.Description).ToArray()
                    });
            }

            // 5) add role
            var addRole = await _userManager.AddToRoleAsync(user, role);
            if (!addRole.Succeeded)
            {
                // rollback user + employee
                await _userManager.DeleteAsync(user);
                empRepo.Delete(employee);
                await _uow.SaveChangesAsync();

                throw new BusinessException("Failed to assign role", 400);
            }

            // 6) link employee -> user
            employee.UserId = user.Id;
            empRepo.Update(employee);
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
            var employees = await _uow.Repository<Employee>().FindAsync(e => !e.IsDeleted);
            return employees.Select(Map).ToList();
        }

        public async Task<IReadOnlyList<EmployeeResponse>> GetSupervisorsAsync()
        {
            var repo = _uow.Repository<Employee>();
            var supervisors = await repo.FindAsync(e => !e.IsDeleted && e.Role == Domain.Enum.EmployeeRole.Supervisor && e.IsActive);
            return supervisors.Select(Map).ToList();
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
            employee.Role = request.Role;

            repo.Update(employee);
            await _uow.SaveChangesAsync();

            return Map(employee);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Employee>();
            var employee = await repo.GetByIdAsync(id);
            if (employee == null) return false;

            repo.HardDelete(employee); // soft delete
            await _uow.SaveChangesAsync();
            return true;
        }

        private static EmployeeResponse Map(Employee e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            Age = e.Age,
            PhoneNumber = e.PhoneNumber,
            IsActive = e.IsActive,
            Role = e.Role,
        };
    }
}
