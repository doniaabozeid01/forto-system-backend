using System;
using Forto.Domain.Entities.Employees;

namespace Forto.Domain.Entities.Ops
{
    /// <summary>
    /// جلسة شيفت كاشير (وردية): من فتح الوردية لحد ما يقفلها.
    /// يُسجّل مين فتح، مين قفل، والساعة. لو مُربوطة بشيفت دوام (صباحي/مسائي) نقدر نجيب العمال اللي في الشيفت ده.
    /// </summary>
    public class CashierShift : BaseEntity
    {
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        /// <summary>شيفت الدوام (صباحي/مسائي) — لو محدد، نقدر نجيب العمال من جدول الدوام اللي شغالين الشيفت ده.</summary>
        public int? ShiftId { get; set; }
        public Shift? Shift { get; set; }

        /// <summary>الكاشير اللي فتح الوردية.</summary>
        public int OpenedByEmployeeId { get; set; }
        public Employee OpenedByEmployee { get; set; } = null!;

        /// <summary>وقت فتح الوردية.</summary>
        public DateTime OpenedAt { get; set; }

        /// <summary>الكاشير اللي قفل الشيفت (لو قُفل يدوي أو تلقائي عند فتح شيفت تاني).</summary>
        public int? ClosedByEmployeeId { get; set; }
        public Employee? ClosedByEmployee { get; set; }

        /// <summary>وقت إغلاق الشيفت. لو null فالشيفت لسه شغال.</summary>
        public DateTime? ClosedAt { get; set; }
    }
}
