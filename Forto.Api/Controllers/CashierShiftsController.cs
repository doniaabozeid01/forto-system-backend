using Forto.Api.Common;
using Forto.Application.Abstractions.Services.CashierShift;
using Forto.Application.DTOs.CashierShifts;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    /// <summary>شيفت الكاشير: بدء الشيفت، إغلاقه، ومعرفة الشيفت النشط للفرع.</summary>
    [Route("api/cashier-shifts")]
    public class CashierShiftsController : BaseApiController
    {
        private readonly ICashierShiftService _cashierShiftService;

        public CashierShiftsController(ICashierShiftService cashierShiftService)
        {
            _cashierShiftService = cashierShiftService;
        }

        /// <summary>ملخص ورديات يوم معيّن: ابعت التاريخ (مثلاً date=2026-02-12) يجيب كل الورديات اللي اتفتحت النهارده، مين مسئول عن كل وردية، وإجمالي المبيعات والكاش والفيزا والخصومات لكل وردية. لو متبعتش date يستخدم تاريخ اليوم.</summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetDailySummary([FromQuery] DateTime? date)
        {
            var targetDate = date?.Date ?? DateTime.UtcNow.Date;
            var summary = await _cashierShiftService.GetDailyShiftsSummaryAsync(targetDate);
            return OkResponse(summary, "OK");
        }

        /// <summary>جلب الشيفت النشط للفرع (لو الكاشير مسجل دخول وعايز يعرف هل فيه شيفت مفتوح ويظهر زرار "ابدأ الشيفت" أو حالة الشيفت).</summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActive([FromQuery] int branchId)
        {
            var shift = await _cashierShiftService.GetActiveForBranchAsync(branchId);
            if (shift == null)
                return OkResponse<CashierShiftResponse?>(null, "No active shift");
            return OkResponse(shift, "OK");
        }

        /// <summary>بدء وردية. Body: { "branchId", "cashierEmployeeId", "shiftId" (اختياري = شيفت صباحي/مسائي عشان تربط الوردية بالشيفت وتجيب العمال).</summary>
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartCashierShiftRequest request)
        {
            if (request == null)
                return FailResponse("Request is required", 400);
            var shift = await _cashierShiftService.StartShiftAsync(request.BranchId, request.CashierEmployeeId, request.ShiftId);
            return OkResponse(shift, "Shift started");
        }

        /// <summary>العمال/المشرفين اللي في الوردية (من جدول الدوام حسب الشيفت صباحي/مسائي). لو الوردية مش مربوطة بشيفت ترجع قائمة فاضية.</summary>
        [HttpGet("{cashierShiftId:int}/employees")]
        public async Task<IActionResult> GetEmployees(int cashierShiftId)
        {
            var list = await _cashierShiftService.GetEmployeesForCashierShiftAsync(cashierShiftId);
            return OkResponse(list, "OK");
        }

        /// <summary>إغلاق الشيفت يدوياً. Body: { "closedByEmployeeId" }.</summary>
        [HttpPost("{shiftId:int}/close")]
        public async Task<IActionResult> Close(int shiftId, [FromBody] CloseCashierShiftRequest request)
        {
            if (request == null)
                return FailResponse("Request is required", 400);
            var shift = await _cashierShiftService.CloseShiftAsync(shiftId, request.ClosedByEmployeeId);
            if (shift == null)
                return FailResponse("Shift not found", 404);
            return OkResponse(shift, "Shift closed");
        }
    }

    public class StartCashierShiftRequest
    {
        public int BranchId { get; set; }
        public int CashierEmployeeId { get; set; }
        /// <summary>اختياري: شيفت الدوام (صباحي/مسائي) عشان تربط الوردية بالشيفت وتجيب العمال من جدول الدوام.</summary>
        public int? ShiftId { get; set; }
    }

    public class CloseCashierShiftRequest
    {
        public int ClosedByEmployeeId { get; set; }
    }
}
