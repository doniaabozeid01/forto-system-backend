using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Dashboard;

namespace Forto.Application.Abstractions.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardSummaryResponse> GetSummaryAsync(int branchId, DateOnly from, DateOnly to);

        Task<AnalyticsResponse> GetTopServicesAsync(int branchId, DateOnly from, DateOnly to);


        Task<AnalyticsResponse> GetTopEmployeesAsync(int branchId, DateOnly from, DateOnly to);

        /// <summary>كل الموظفين مع تفصيل الخدمات + فواتير ككاشير/مشرف. role: فلتر اختياري. invoicesOnly: لو true نرجع الفواتير فقط (بدون حجوزات/خدمات).</summary>
        Task<TopEmployeesWithServicesResponse> GetTopEmployeesWithServicesAsync(int branchId, DateOnly from, DateOnly to, int? role = null);
    }

}
