using System;

namespace Forto.Application.DTOs.CashierShifts
{
    /// <summary>بيانات شيفت كاشير (جلسة) للعرض.</summary>
    public class CashierShiftResponse
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        /// <summary>شيفت الدوام (صباحي/مسائي) المرتبط بالوردية — عشان تجيب العمال من جدول الدوام.</summary>
        public int? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public int OpenedByEmployeeId { get; set; }
        public string? OpenedByEmployeeName { get; set; }
        public DateTime OpenedAt { get; set; }
        public int? ClosedByEmployeeId { get; set; }
        public string? ClosedByEmployeeName { get; set; }
        public DateTime? ClosedAt { get; set; }
        /// <summary>لو الوردية لسه مفتوحة.</summary>
        public bool IsActive { get; set; }
    }

    /// <summary>موظف في الوردية (من جدول الدوام حسب الشيفت صباحي/مسائي).</summary>
    public class CashierShiftEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = "";
        public string? RoleName { get; set; }
        public int Role { get; set; }
    }

    /// <summary>ملخص وردية واحدة ضمن ملخص الورديات اليومي: رقم الشيفت (صباحي/مسائي)، اسم الشيفت، المسؤول، إجمالي المبيعات، كاش، فيزا، خصومات.</summary>
    public class DailyShiftSummaryItemDto
    {
        /// <summary>رقم الشيفت (شيفت الدوام: صباحي/مسائي) = ShiftId.</summary>
        public int? ShiftNumber { get; set; }
        /// <summary>اسم الشيفت (مثلاً صباحي / مسائي).</summary>
        public string? ShiftName { get; set; }
        public int CashierShiftId { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        /// <summary>تاريخ فتح الوردية.</summary>
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        /// <summary>الكاشير المسؤول (اللي فتح الوردية).</summary>
        public int ResponsibleEmployeeId { get; set; }
        public string? ResponsibleEmployeeName { get; set; }
        /// <summary>إجمالي مبيعات الوردية (فواتير مدفوعة).</summary>
        public decimal TotalSales { get; set; }
        /// <summary>المدفوع نقداً (كاش).</summary>
        public decimal CashAmount { get; set; }
        /// <summary>المدفوع بطرق أخرى (فيزا).</summary>
        public decimal VisaAmount { get; set; }
        /// <summary>إجمالي الخصومات.</summary>
        public decimal TotalDiscounts { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>ملخص ورديات يوم معيّن: التاريخ + قائمة الورديات مع تفاصيل كل وردية.</summary>
    public class DailyShiftsSummaryResponse
    {
        public DateTime Date { get; set; }
        public List<DailyShiftSummaryItemDto> Shifts { get; set; } = new();
        /// <summary>إجمالي المبيعات لليوم (كل الورديات).</summary>
        public decimal TotalSalesForDay { get; set; }
        public decimal TotalCashForDay { get; set; }
        public decimal TotalVisaForDay { get; set; }
        public decimal TotalDiscountsForDay { get; set; }
    }
}
