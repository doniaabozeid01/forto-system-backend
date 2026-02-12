using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.CashierShifts;
using Forto.Domain.Entities.Billings;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using CashierShiftEntity = Forto.Domain.Entities.Ops.CashierShift;
using HrShift = Forto.Domain.Entities.Employees.Shift;

namespace Forto.Application.Abstractions.Services.CashierShift
{
    public class CashierShiftService : ICashierShiftService
    {
        private readonly IUnitOfWork _uow;

        public CashierShiftService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<CashierShiftResponse?> GetActiveForBranchAsync(int branchId)
        {
            var repo = _uow.Repository<CashierShiftEntity>();
            var shift = (await repo.FindAsync(s =>
                s.BranchId == branchId && s.ClosedAt == null && !s.IsDeleted)).FirstOrDefault();
            if (shift == null) return null;
            return await MapToResponseAsync(shift);
        }

        public async Task<CashierShiftResponse> StartShiftAsync(int branchId, int cashierEmployeeId, int? shiftId = null)
        {
            await RequireCashierAsync(cashierEmployeeId);

            var branchRepo = _uow.Repository<Branch>();
            var branch = await branchRepo.GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            if (shiftId.HasValue)
            {
                var hrShiftRepo = _uow.Repository<HrShift>();
                var hrShift = await hrShiftRepo.GetByIdAsync(shiftId.Value);
                if (hrShift == null || hrShift.IsDeleted)
                    throw new BusinessException("Shift (صباحي/مسائي) not found", 404);
            }

            var repo = _uow.Repository<CashierShiftEntity>();

            // ممنوع تبدأ وردية جديدة لو فيه وردية تانية مفتوحة لنفس الفرع — لازم تقفلها الأول
            var hasActive = await repo.AnyAsync(s =>
                s.BranchId == branchId && s.ClosedAt == null && !s.IsDeleted);
            if (hasActive)
                throw new BusinessException("لا يمكن بدء وردية جديدة. يوجد وردية مفتوحة لهذا الفرع، يجب إغلاقها أولاً.", 409);

            var now = DateTime.UtcNow;
            var newShift = new CashierShiftEntity
            {
                BranchId = branchId,
                ShiftId = shiftId,
                OpenedByEmployeeId = cashierEmployeeId,
                OpenedAt = now
            };
            await repo.AddAsync(newShift);
            await _uow.SaveChangesAsync();

            return (await MapToResponseAsync(newShift))!;
        }

        public async Task<CashierShiftResponse?> CloseShiftAsync(int shiftId, int closedByEmployeeId)
        {
            await RequireCashierAsync(closedByEmployeeId);

            var repo = _uow.Repository<CashierShiftEntity>();
            var shift = await repo.GetByIdAsync(shiftId);
            if (shift == null) return null;
            if (shift.ClosedAt.HasValue)
                throw new BusinessException("Shift is already closed", 409);

            // يقفل الوردية بس الكاشير اللي فتحها
            if (shift.OpenedByEmployeeId != closedByEmployeeId)
                throw new BusinessException("لا يمكن إغلاق الوردية. فقط الكاشير الذي فتح الوردية يمكنه إغلاقها.", 403);

            var now = DateTime.UtcNow;
            shift.ClosedAt = now;
            shift.ClosedByEmployeeId = closedByEmployeeId;
            repo.Update(shift);
            await _uow.SaveChangesAsync();

            return await MapToResponseAsync(shift);
        }

        public async Task<IReadOnlyList<CashierShiftEmployeeDto>> GetEmployeesForCashierShiftAsync(int cashierShiftId)
        {
            var repo = _uow.Repository<CashierShiftEntity>();
            var cs = await repo.GetByIdAsync(cashierShiftId);
            if (cs == null) return Array.Empty<CashierShiftEmployeeDto>();
            if (!cs.ShiftId.HasValue) return Array.Empty<CashierShiftEmployeeDto>();

            var dayOfWeek = cs.OpenedAt.DayOfWeek; // نفس يوم فتح الوردية
            var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
            var schedules = await scheduleRepo.FindAsync(s =>
                s.ShiftId == cs.ShiftId.Value && s.DayOfWeek == dayOfWeek && !s.IsOff && !s.IsDeleted);
            var employeeIds = schedules.Select(s => s.EmployeeId).Distinct().ToList();
            if (employeeIds.Count == 0) return Array.Empty<CashierShiftEmployeeDto>();

            var empRepo = _uow.Repository<Employee>();
            var employees = await empRepo.FindAsync(e => employeeIds.Contains(e.Id) && !e.IsDeleted);
            return employees.Select(e => new CashierShiftEmployeeDto
            {
                EmployeeId = e.Id,
                Name = e.Name,
                Role = (int)e.Role,
                RoleName = e.Role.ToString()
            }).ToList();
        }

        private async Task<CashierShiftResponse?> MapToResponseAsync(CashierShiftEntity entity)
        {
            if (entity == null) return null;
            var empRepo = _uow.Repository<Employee>();
            var openedBy = await empRepo.GetByIdAsync(entity.OpenedByEmployeeId);
            Employee? closedBy = null;
            if (entity.ClosedByEmployeeId.HasValue)
                closedBy = await empRepo.GetByIdAsync(entity.ClosedByEmployeeId.Value);

            int? shiftId = entity.ShiftId;
            string? shiftName = null;
            if (entity.ShiftId.HasValue)
            {
                var hrShiftRepo = _uow.Repository<HrShift>();
                var hrShift = await hrShiftRepo.GetByIdAsync(entity.ShiftId.Value);
                shiftName = hrShift?.Name;
            }

            return new CashierShiftResponse
            {
                Id = entity.Id,
                BranchId = entity.BranchId,
                ShiftId = shiftId,
                ShiftName = shiftName,
                OpenedByEmployeeId = entity.OpenedByEmployeeId,
                OpenedByEmployeeName = openedBy?.Name,
                OpenedAt = entity.OpenedAt,
                ClosedByEmployeeId = entity.ClosedByEmployeeId,
                ClosedByEmployeeName = closedBy?.Name,
                ClosedAt = entity.ClosedAt,
                IsActive = !entity.ClosedAt.HasValue
            };
        }

        public async Task<DailyShiftsSummaryResponse> GetDailyShiftsSummaryAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var shiftRepo = _uow.Repository<CashierShiftEntity>();
            var shifts = await shiftRepo.FindAsync(s =>
                !s.IsDeleted && s.OpenedAt >= startOfDay && s.OpenedAt < endOfDay);
            var shiftList = shifts.ToList();
            if (shiftList.Count == 0)
                return new DailyShiftsSummaryResponse { Date = startOfDay };

            var shiftIds = shiftList.Select(s => s.Id).ToList();
            var invoiceRepo = _uow.Repository<Invoice>();
            var invoices = await invoiceRepo.FindAsync(i =>
                i.CashierShiftId != null && shiftIds.Contains(i.CashierShiftId.Value) &&
                i.Status == InvoiceStatus.Paid && !i.IsDeleted);
            var invoiceList = invoices.ToList();

            var byShift = invoiceList
                .GroupBy(i => i.CashierShiftId!.Value)
                .ToDictionary(g => g.Key, g => new
                {
                    TotalSales = g.Sum(i => i.Total),
                    Cash = g.Sum(i => i.CashAmount ?? 0),
                    Visa = g.Sum(i => i.VisaAmount ?? 0),
                    Discounts = g.Sum(i => i.Discount)
                });

            var empRepo = _uow.Repository<Employee>();
            var branchRepo = _uow.Repository<Branch>();
            var hrShiftRepo = _uow.Repository<HrShift>();
            var result = new DailyShiftsSummaryResponse { Date = startOfDay };
            decimal totalSales = 0, totalCash = 0, totalVisa = 0, totalDiscounts = 0;

            foreach (var s in shiftList.OrderBy(s => s.OpenedAt))
            {
                var stats = byShift.GetValueOrDefault(s.Id);
                var totalSalesS = stats?.TotalSales ?? 0;
                var cashS = stats?.Cash ?? 0;
                var visaS = stats?.Visa ?? 0;
                var discountsS = stats?.Discounts ?? 0;
                totalSales += totalSalesS;
                totalCash += cashS;
                totalVisa += visaS;
                totalDiscounts += discountsS;

                var openedBy = await empRepo.GetByIdAsync(s.OpenedByEmployeeId);
                var branch = await branchRepo.GetByIdAsync(s.BranchId);
                HrShift? hrShift = null;
                if (s.ShiftId.HasValue)
                    hrShift = await hrShiftRepo.GetByIdAsync(s.ShiftId.Value);
                result.Shifts.Add(new DailyShiftSummaryItemDto
                {
                    ShiftNumber = s.ShiftId,
                    ShiftName = hrShift?.Name,
                    CashierShiftId = s.Id,
                    BranchId = s.BranchId,
                    BranchName = branch?.Name,
                    OpenedAt = s.OpenedAt,
                    ClosedAt = s.ClosedAt,
                    ResponsibleEmployeeId = s.OpenedByEmployeeId,
                    ResponsibleEmployeeName = openedBy?.Name,
                    TotalSales = totalSalesS,
                    CashAmount = cashS,
                    VisaAmount = visaS,
                    TotalDiscounts = discountsS,
                    IsActive = !s.ClosedAt.HasValue
                });
            }

            result.TotalSalesForDay = totalSales;
            result.TotalCashForDay = totalCash;
            result.TotalVisaForDay = totalVisa;
            result.TotalDiscountsForDay = totalDiscounts;
            return result;
        }

        private async Task RequireCashierAsync(int employeeId)
        {
            var empRepo = _uow.Repository<Employee>();
            var emp = await empRepo.GetByIdAsync(employeeId);
            if (emp == null || !emp.IsActive)
                throw new BusinessException("Cashier not found", 404);
            if (emp.Role != EmployeeRole.Cashier && emp.Role != EmployeeRole.Supervisor && emp.Role != EmployeeRole.Admin)
                throw new BusinessException("Not allowed. Only cashier/supervisor/admin can manage shifts", 403);
        }
    }
}
