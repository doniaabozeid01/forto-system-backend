using Forto.Application.DTOs.Billings.Gifts;
using Forto.Application.DTOs.Catalog;
using Forto.Application.DTOs.Catalog.Services;
using Forto.Application.DTOs.Employees;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Catalogs.Service
{
    public interface ICatalogService
    {
        Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request);
        Task<ServiceResponse?> UpdateServiceAsync(int id, UpdateServiceRequest request);
        Task<ServiceResponse?> GetServiceAsync(int id);
        Task<IReadOnlyList<ServiceResponse>> GetServicesAsync(int? categoryId = null);
        Task<ServiceResponse?> UpsertRatesAsync(int serviceId, UpsertServiceRatesRequest request);
        Task<bool> DeleteServiceAsync(int id);
        /// <summary>العمال اللي بيعملوا الخدمة. لو shiftId مُمرّر يرجع فقط اللي شغالين في الشيفت ده (من جدول الدوام، يوم اليوم).</summary>
        Task<IReadOnlyList<EmployeeResponse>> GetEmployeesForServiceAsync(int serviceId, int? shiftId = null);
        Task<EmployeeAvailabilityResponse> GetEmployeesForServiceAtAsync(
            int bookingId,
            int serviceId,
            System.DateTime scheduledStart);

        /// <summary>الهدايا المتاحة بناءً على قائمة خدمات — مع مخزون الفرع لو branchId مُمرّر.</summary>
        Task<GiftOptionsByServicesResponse> GetGiftOptionsByServiceIdsAsync(IReadOnlyList<int> serviceIds, int? branchId = null);

        /// <summary>قائمة الهدايا (منتجات) المربوطة بخدمة معينة.</summary>
        Task<IReadOnlyList<ServiceGiftOptionDto>> GetGiftOptionsForServiceAsync(int serviceId);

        /// <summary>ربط منتجات كهدايا لخدمة — productIds فقط (حتى لو منتج واحد).</summary>
        Task<IReadOnlyList<ServiceGiftOptionDto>> AddGiftOptionsToServiceAsync(int serviceId, IReadOnlyList<int> productIds);

        /// <summary>إزالة قائمة منتجات من هدايا الخدمة.</summary>
        Task<int> RemoveGiftOptionsFromServiceAsync(int serviceId, IReadOnlyList<int> productIds);
    }
}