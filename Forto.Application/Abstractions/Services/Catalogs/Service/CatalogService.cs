using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Catalog.Services;
using Forto.Application.DTOs.Employees;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Catalogs.Service
{
    public class CatalogService : ICatalogService
    {
        private readonly IUnitOfWork _uow;

        public CatalogService(IUnitOfWork uow) => _uow = uow;

        public async Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request)
        {
            // optional: validate category exists later لو حابة
            var repo = _uow.Repository<Domain.Entities.Catalog.Service>();

            var entity = new Domain.Entities.Catalog.Service
            {
                CategoryId = request.CategoryId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                IsActive = true
            };

            await repo.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return new ServiceResponse
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                Name = entity.Name,
                Description = entity.Description,
                Rates = new()
            };
        }

        public async Task<ServiceResponse?> GetServiceAsync(int id)
        {
            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var rateRepo = _uow.Repository<ServiceRate>();

            var s = await serviceRepo.GetByIdAsync(id);
            if (s == null) return null;

            var rates = await rateRepo.FindAsync(r => r.ServiceId == id);

            return Map(s, rates);
        }

        public async Task<IReadOnlyList<ServiceResponse>> GetServicesAsync(int? categoryId = null)
        {
            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var rateRepo = _uow.Repository<ServiceRate>();

            var services = categoryId.HasValue
                ? await serviceRepo.FindAsync(s => s.CategoryId == categoryId.Value)
                : await serviceRepo.GetAllAsync();

            // جمع rates لكل الخدمات (simple version)
            var serviceIds = services.Select(s => s.Id).ToList();
            var rates = serviceIds.Count == 0
                ? new List<ServiceRate>()
                : (await rateRepo.FindAsync(r => serviceIds.Contains(r.ServiceId))).ToList();

            var ratesGrouped = rates.GroupBy(r => r.ServiceId).ToDictionary(g => g.Key, g => g.ToList());

            return services.Select(s =>
            {
                ratesGrouped.TryGetValue(s.Id, out var r);
                return Map(s, r ?? new List<ServiceRate>());
            }).ToList();
        }

        public async Task<ServiceResponse?> UpsertRatesAsync(int serviceId, UpsertServiceRatesRequest request)
        {
            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var rateRepo = _uow.Repository<ServiceRate>();

            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null) return null;

            // منع تكرار BodyType في نفس الطلب
            var dups = request.Rates.GroupBy(x => x.BodyType).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (dups.Any())
                throw new BusinessException("Duplicate body types in rates", 400,
                    new Dictionary<string, string[]>
                    {
                        ["rates"] = dups.Select(x => $"{x} duplicated").ToArray()
                    });

            var existing = await rateRepo.FindAsync(r => r.ServiceId == serviceId);
            var map = existing.ToDictionary(x => x.BodyType, x => x);

            foreach (var r in request.Rates)
            {
                if (r.DurationMinutes <= 0)
                    throw new BusinessException("DurationMinutes must be > 0", 400);

                if (map.TryGetValue(r.BodyType, out var row))
                {
                    row.Price = r.Price;
                    row.DurationMinutes = r.DurationMinutes;
                    row.IsActive = true;
                    rateRepo.Update(row);
                }
                else
                {
                    await rateRepo.AddAsync(new ServiceRate
                    {
                        ServiceId = serviceId,
                        BodyType = r.BodyType,
                        Price = r.Price,
                        DurationMinutes = r.DurationMinutes,
                        IsActive = true
                    });
                }
            }

            await _uow.SaveChangesAsync();

            var after = await rateRepo.FindAsync(x => x.ServiceId == serviceId);
            return Map(service, after);
        }

        public async Task<bool> DeleteServiceAsync(int id)
        {
            var repo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var s = await repo.GetByIdAsync(id);
            if (s == null) return false;

            repo.Delete(s);
            await _uow.SaveChangesAsync();
            return true;
        }

        private static ServiceResponse Map(Domain.Entities.Catalog.Service s, IReadOnlyList<ServiceRate> rates)
        {
            return new ServiceResponse
            {
                Id = s.Id,
                CategoryId = s.CategoryId,
                Name = s.Name,
                Description = s.Description,
                Rates = rates
                    .OrderBy(x => (int)x.BodyType)
                    .Select(x => new ServiceRateResponse
                    {
                        Id = x.Id,
                        BodyType = x.BodyType,
                        Price = x.Price,
                        DurationMinutes = x.DurationMinutes
                    }).ToList()
            };
        }



        public async Task<IReadOnlyList<EmployeeResponse>> GetEmployeesForServiceAsync(int serviceId)
        {
            // تأكد الخدمة موجودة (اختياري بس لطيف)
            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null)
                throw new BusinessException("Service not found", 404);

            var linkRepo = _uow.Repository<EmployeeService>();
            var employeeRepo = _uow.Repository<Employee>();

            // هات الروابط الفعالة
            var links = await linkRepo.FindAsync(x => x.ServiceId == serviceId && x.IsActive);

            var employeeIds = links.Select(x => x.EmployeeId).Distinct().ToList();
            if (employeeIds.Count == 0)
                return new List<EmployeeResponse>();

            var employees = await employeeRepo.FindAsync(e => employeeIds.Contains(e.Id) && e.IsActive);

            return employees
                .OrderBy(e => e.Name)
                .Select(e => new EmployeeResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    PhoneNumber = e.PhoneNumber,
                    IsActive = e.IsActive
                })
                .ToList();
        }
    }
}