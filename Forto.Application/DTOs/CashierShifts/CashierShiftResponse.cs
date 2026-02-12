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
}
