using System.Threading.Tasks;
using Forto.Application.DTOs.CashierShifts;

namespace Forto.Application.Abstractions.Services.CashierShift
{
    /// <summary>شيفت الكاشير: فتح / إغلاق الشيفت، وإرجاع الشيفت النشط للفرع.</summary>
    public interface ICashierShiftService
    {
        /// <summary>الشيفت النشط للفرع (اللي لسه مفتوح). لو مفيش يرجع null.</summary>
        Task<CashierShiftResponse?> GetActiveForBranchAsync(int branchId);

        /// <summary>بدء وردية: يفتح وردية جديدة. اختياري shiftId = شيفت الدوام (صباحي/مسائي) عشان تربط الوردية بالشيفت وتقدر تجيب العمال.</summary>
        Task<CashierShiftResponse> StartShiftAsync(int branchId, int cashierEmployeeId, int? shiftId = null);

        /// <summary>إغلاق الوردية يدوياً. الكاشير اللي فتحها بس يقدر يقفلها.</summary>
        Task<CashierShiftResponse?> CloseShiftAsync(int shiftId, int closedByEmployeeId);

        /// <summary>العمال/المشرفين اللي في الوردية: من جدول الدوام حسب الشيفت (صباحي/مسائي) ويوم فتح الوردية. لو الوردية مش مربوطة بشيفت يرجع قائمة فاضية.</summary>
        Task<IReadOnlyList<CashierShiftEmployeeDto>> GetEmployeesForCashierShiftAsync(int cashierShiftId);
    }
}
