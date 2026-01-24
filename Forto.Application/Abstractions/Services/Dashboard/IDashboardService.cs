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

    }

}
