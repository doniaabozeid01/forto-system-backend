using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Employees.EmployeeServices;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.EmployeeServices
{
    public class EmployeeCapabilityService : IEmployeeCapabilityService
    {
        private readonly IUnitOfWork _uow;

        public EmployeeCapabilityService(IUnitOfWork uow) => _uow = uow;

        public async Task<EmployeeServicesResponse> GetAsync(int employeeId)
        {
            var emp = await _uow.Repository<Employee>().GetByIdAsync(employeeId);
            if (emp == null)
                throw new BusinessException("Employee not found", 404);

            var linkRepo = _uow.Repository<EmployeeService>();
            var links = await linkRepo.FindAsync(x => x.EmployeeId == employeeId && x.IsActive);

            return new EmployeeServicesResponse
            {
                EmployeeId = employeeId,
                ServiceIds = links.Select(x => x.ServiceId).Distinct().ToList()
            };
        }

        public async Task<EmployeeServicesResponse> UpdateAsync(int employeeId, UpdateEmployeeServicesRequest request)
        {
            var emp = await _uow.Repository<Employee>().GetByIdAsync(employeeId);
            if (emp == null)
                throw new BusinessException("Employee not found", 404);

            var serviceIds = request.ServiceIds?.Distinct().ToList() ?? new List<int>();

            // validate services exist
            if (serviceIds.Count > 0)
            {
                var serviceRepo = _uow.Repository<Service>();
                var found = await serviceRepo.FindAsync(s => serviceIds.Contains(s.Id));
                var foundIds = found.Select(s => s.Id).ToHashSet();

                var missing = serviceIds.Where(id => !foundIds.Contains(id)).ToList();
                if (missing.Any())
                    throw new BusinessException("Some services do not exist", 400,
                        new Dictionary<string, string[]>
                        {
                            ["serviceIds"] = missing.Select(x => $"ServiceId {x} not found").ToArray()
                        });
            }

            var linkRepo = _uow.Repository<EmployeeService>();
            var existing = await linkRepo.FindAsync(x => x.EmployeeId == employeeId);

            var existingMap = existing.ToDictionary(x => x.ServiceId, x => x);

            // 1) Deactivate removed
            foreach (var link in existing)
            {
                if (!serviceIds.Contains(link.ServiceId))
                {
                    if (link.IsActive)
                    {
                        link.IsActive = false;
                        linkRepo.Update(link);
                    }
                }
            }

            // 2) Upsert selected
            foreach (var sid in serviceIds)
            {
                if (existingMap.TryGetValue(sid, out var link))
                {
                    if (!link.IsActive)
                    {
                        link.IsActive = true;
                        linkRepo.Update(link);
                    }
                }
                else
                {
                    await linkRepo.AddAsync(new EmployeeService
                    {
                        EmployeeId = employeeId,
                        ServiceId = sid,
                        IsActive = true
                    });
                }
            }

            await _uow.SaveChangesAsync();

            return new EmployeeServicesResponse
            {
                EmployeeId = employeeId,
                ServiceIds = serviceIds
            };
        }
    }
}