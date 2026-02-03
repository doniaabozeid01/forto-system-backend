using System;
using System.Collections.Generic;

namespace Forto.Application.DTOs.Dashboard
{
    /// <summary>تقرير أفضل الموظفين مع تفصيل الخدمات اللي اشتغلوا عليها.</summary>
    public class TopEmployeesWithServicesResponse
    {
        public int BranchId { get; set; }
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int? RoleFilter { get; set; }
        /// <summary>لو true: التقرير على الفواتير فقط (بدون حجوزات/خدمات).</summary>
        //public bool InvoicesOnly { get; set; }
        public int TotalDoneItems { get; set; }
        //public int TotalInvoicesAsCashier { get; set; }
        //public int TotalInvoicesAsSupervisor { get; set; }
        public List<EmployeeWithServicesDto> Items { get; set; } = new();
    }
}
