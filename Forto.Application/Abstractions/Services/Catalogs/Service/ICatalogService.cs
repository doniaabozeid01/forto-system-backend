using Forto.Application.DTOs.Catalog.Services;
using Forto.Application.DTOs.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Catalogs.Service
{
    public interface ICatalogService
    {
        Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request);
        Task<ServiceResponse?> GetServiceAsync(int id);
        Task<IReadOnlyList<ServiceResponse>> GetServicesAsync(int? categoryId = null);
        Task<ServiceResponse?> UpsertRatesAsync(int serviceId, UpsertServiceRatesRequest request);
        Task<bool> DeleteServiceAsync(int id);
        Task<IReadOnlyList<EmployeeResponse>> GetEmployeesForServiceAsync(int serviceId);
    }
}