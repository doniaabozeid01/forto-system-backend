using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.CashierShifts;
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
