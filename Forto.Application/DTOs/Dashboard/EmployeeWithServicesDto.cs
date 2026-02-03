using System.Collections.Generic;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Dashboard
{
    /// <summary>موظف مع عدد الخدمات المكتملة + فواتير ككاشير/مشرف + فلتر بالـ role.</summary>
    public class EmployeeWithServicesDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public EmployeeRole Role { get; set; }
        /// <summary>عدد الخدمات المكتملة (عامل).</summary>
        public int Count { get; set; }
        public decimal Percent { get; set; }
        /// <summary>الخدمات اللي اشتغل عليها (مع عدد كل خدمة).</summary>
        public List<ServiceCountDto> Services { get; set; } = new();
        /// <summary>عدد الفواتير اللي دفعها (كاشير).</summary>
        public int InvoicesAsCashierCount { get; set; }
        /// <summary>عدد الفواتير اللي هو مشرف عليها.</summary>
        public int InvoicesAsSupervisorCount { get; set; }
    }
}
