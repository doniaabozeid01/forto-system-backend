using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Schedule;
using Forto.Domain.Entities.Employees;

namespace Forto.Application.Abstractions.Services.Schedule
{
    public class EmployeeScheduleService : IEmployeeScheduleService
    {
        private readonly IUnitOfWork _uow;

        public EmployeeScheduleService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<EmployeeScheduleResponse> GetWeekAsync(int employeeId)
        {
            // تأكد employee موجود
            var employeeRepo = _uow.Repository<Employee>();
            var employee = await employeeRepo.GetByIdAsync(employeeId);
            if (employee == null)
                throw new BusinessException("Employee not found", 404);

            var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
            var shiftRepo = _uow.Repository<Domain.Entities.Employees.Shift>();

            var schedules = await scheduleRepo.FindAsync(x => x.EmployeeId == employeeId);

            // نجيب الشيفتات المطلوبة مرة واحدة
            var shiftIds = schedules.Where(s => s.ShiftId.HasValue).Select(s => s.ShiftId!.Value).Distinct().ToList();
            var shifts = shiftIds.Count == 0
                ? new List<Domain.Entities.Employees.Shift>()
                : (await shiftRepo.FindAsync(s => shiftIds.Contains(s.Id))).ToList();

            var shiftMap = shifts.ToDictionary(s => s.Id, s => s);

            var response = new EmployeeScheduleResponse
            {
                EmployeeId = employeeId,
                Days = schedules
                    .OrderBy(s => (int)s.DayOfWeek)
                    .Select(s =>
                    {
                        shiftMap.TryGetValue(s.ShiftId ?? -1, out var shift);

                        // لو مربوط بـ Shift وخزّنا Start/End null، نعرض وقت الشيفت
                        var start = s.StartTime ?? shift?.StartTime;
                        var end = s.EndTime ?? shift?.EndTime;

                        return new DayScheduleResponse
                        {
                            DayOfWeek = (int)s.DayOfWeek,
                            IsOff = s.IsOff,
                            ShiftId = s.ShiftId,
                            ShiftName = shift?.Name,
                            StartTime = start,
                            EndTime = end
                        };
                    })
                    .ToList()
            };

            return response;
        }

        public async Task<EmployeeScheduleResponse> UpsertWeekAsync(int employeeId, UpsertEmployeeScheduleRequest request)
        {
            // 1) employee موجود؟
            var employeeRepo = _uow.Repository<Employee>();
            var employee = await employeeRepo.GetByIdAsync(employeeId);
            if (employee == null)
                throw new BusinessException("Employee not found", 404);

            // 2) تأكد مفيش day مكرر
            var duplicates = request.Days
                .GroupBy(d => d.DayOfWeek)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
                throw new BusinessException(
                    "Duplicate days in schedule request",
                    400,
                    new Dictionary<string, string[]>
                    {
                        ["days"] = duplicates.Select(d => $"DayOfWeek {d} is duplicated.").ToArray()
                    });

            var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
            var shiftRepo = _uow.Repository<Domain.Entities.Employees.Shift>();

            // 3) validate shift ids موجودة
            var requestedShiftIds = request.Days.Where(d => d.ShiftId.HasValue).Select(d => d.ShiftId!.Value).Distinct().ToList();
            if (requestedShiftIds.Any())
            {
                var found = await shiftRepo.FindAsync(s => requestedShiftIds.Contains(s.Id));
                var foundIds = found.Select(s => s.Id).ToHashSet();

                var missing = requestedShiftIds.Where(id => !foundIds.Contains(id)).ToList();
                if (missing.Any())
                    throw new BusinessException(
                        "Some shifts do not exist",
                        400,
                        new Dictionary<string, string[]>
                        {
                            ["shiftId"] = missing.Select(x => $"ShiftId {x} not found.").ToArray()
                        });
            }

            // 4) load existing schedules for employee
            var existing = await scheduleRepo.FindAsync(x => x.EmployeeId == employeeId);
            var existingMap = existing.ToDictionary(x => x.DayOfWeek, x => x);

            // 5) upsert each day
            foreach (var day in request.Days)
            {
                // قواعد اليوم:
                // - لو IsOff=true => ShiftId/Start/End لازم يبقوا null (هنصفرهم)
                // - لو ShiftId موجود => Start/End نخليهم null (وقت الشيفت هو اللي يتعرض)
                // - لو ShiftId null و IsOff false => لازم StartTime و EndTime موجودين ومش متساويين

                if (!Enum.IsDefined(typeof(DayOfWeek), day.DayOfWeek))
                    throw new BusinessException("Invalid DayOfWeek. Use 0..6", 400);

                var dayEnum = (DayOfWeek)day.DayOfWeek;

                if (day.IsOff)
                {
                    day.ShiftId = null;
                    day.StartTime = null;
                    day.EndTime = null;
                }
                else
                {
                    if (day.ShiftId.HasValue)
                    {
                        // نعتمد على وقت الشيفت
                        day.StartTime = null;
                        day.EndTime = null;
                    }
                    else
                    {
                        if (day.StartTime == null || day.EndTime == null)
                            throw new BusinessException(
                                "StartTime and EndTime are required when ShiftId is not provided",
                                400,
                                new Dictionary<string, string[]>
                                {
                                    ["startTime"] = new[] { "Required when shiftId is null." },
                                    ["endTime"] = new[] { "Required when shiftId is null." }
                                });

                        if (day.StartTime == day.EndTime)
                            throw new BusinessException(
                                "StartTime and EndTime cannot be the same",
                                400,
                                new Dictionary<string, string[]>
                                {
                                    ["endTime"] = new[] { "EndTime must be different from StartTime." }
                                });
                    }
                }

                if (existingMap.TryGetValue(dayEnum, out var row))
                {
                    // Update
                    row.IsOff = day.IsOff;
                    row.ShiftId = day.ShiftId;
                    row.StartTime = day.StartTime;
                    row.EndTime = day.EndTime;

                    scheduleRepo.Update(row);
                }
                else
                {
                    // Insert
                    var newRow = new EmployeeWorkSchedule
                    {
                        EmployeeId = employeeId,
                        DayOfWeek = dayEnum,
                        IsOff = day.IsOff,
                        ShiftId = day.ShiftId,
                        StartTime = day.StartTime,
                        EndTime = day.EndTime
                    };

                    await scheduleRepo.AddAsync(newRow);
                }
            }

            await _uow.SaveChangesAsync();

            // رجّع الجدول بعد التعديل
            return await GetWeekAsync(employeeId);
        }


        public async Task<bool> IsEmployeeWorkingAsync(int employeeId, DateTime dateTime)
        {
            var dow = dateTime.DayOfWeek;
            var hour = TimeOnly.FromDateTime(dateTime);

            var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
            var schedules = await scheduleRepo.FindAsync(s =>
                s.EmployeeId == employeeId &&
                s.DayOfWeek == dow &&
                !s.IsOff);

            if (schedules.Count == 0) return false;

            var schedule = schedules.First();

            TimeOnly? start = schedule.StartTime;
            TimeOnly? end = schedule.EndTime;

            if (schedule.ShiftId.HasValue)
            {
                var shiftRepo = _uow.Repository<Domain.Entities.Employees.Shift>();
                var shift = await shiftRepo.GetByIdAsync(schedule.ShiftId.Value);
                if (shift != null)
                {
                    start ??= shift.StartTime;
                    end ??= shift.EndTime;
                }
            }

            if (start == null || end == null) return false;

            // MVP: shift لا يعبر منتصف الليل
            return hour >= start.Value && hour < end.Value;
        }
    }


}
