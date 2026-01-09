using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Employees.Tasks;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Clients;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Employees.Tasks
{
    public class EmployeeTaskService : IEmployeeTaskService
    {
        private readonly IUnitOfWork _uow;

        public EmployeeTaskService(IUnitOfWork uow) => _uow = uow;

        public async Task<EmployeeTasksPageResponse> GetTasksAsync(int employeeId, DateOnly date)
        {
            var empRepo = _uow.Repository<Employee>();
            var employee = await empRepo.GetByIdAsync(employeeId);
            if (employee == null || !employee.IsActive)
                throw new BusinessException("Employee not found", 404);

            var dow = date.DayOfWeek;
            var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
            var schedules = await scheduleRepo.FindAsync(s => s.EmployeeId == employeeId && s.DayOfWeek == dow && !s.IsOff);

            if (schedules.Count == 0)
                return new EmployeeTasksPageResponse { EmployeeId = employeeId, Date = date };

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

            if (start == null || end == null)
                return new EmployeeTasksPageResponse { EmployeeId = employeeId, Date = date };

            var linkRepo = _uow.Repository<Domain.Entities.Employees.EmployeeService>();
            var links = await linkRepo.FindAsync(l => l.EmployeeId == employeeId && l.IsActive);
            var qualifiedServiceIds = links.Select(l => l.ServiceId).Distinct().ToHashSet();

            if (qualifiedServiceIds.Count == 0)
                return new EmployeeTasksPageResponse { EmployeeId = employeeId, Date = date };

            var bookingRepo = _uow.Repository<Booking>();
            var dayStart = date.ToDateTime(new TimeOnly(0, 0));
            var dayEnd = dayStart.AddDays(1);

            var bookings = await bookingRepo.FindAsync(b =>
                b.ScheduledStart >= dayStart &&
                b.ScheduledStart < dayEnd &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Completed);

            if (bookings.Count == 0)
                return new EmployeeTasksPageResponse { EmployeeId = employeeId, Date = date };

            var bookingIds = bookings.Select(b => b.Id).ToList();

            var itemRepo = _uow.Repository<BookingItem>();

            // ✅ pending not taken OR inprogress assigned to me
            var items = await itemRepo.FindAsync(i =>
                bookingIds.Contains(i.BookingId) &&
                qualifiedServiceIds.Contains(i.ServiceId) &&
                (
                    (i.Status == BookingItemStatus.Pending && i.AssignedEmployeeId == null)
                    ||
                    (i.Status == BookingItemStatus.InProgress && i.AssignedEmployeeId == employeeId)
                )
            );

            if (items.Count == 0)
                return new EmployeeTasksPageResponse { EmployeeId = employeeId, Date = date };

            var bookingMap = bookings.ToDictionary(b => b.Id, b => b);

            // shift window filter
            var filtered = items.Where(i =>
            {
                if (!bookingMap.TryGetValue(i.BookingId, out var b)) return false;
                var hour = TimeOnly.FromDateTime(b.ScheduledStart);
                return hour >= start.Value && hour < end.Value;
            }).ToList();

            if (filtered.Count == 0)
                return new EmployeeTasksPageResponse { EmployeeId = employeeId, Date = date };

            var carRepo = _uow.Repository<Car>();
            var serviceRepo = _uow.Repository<Service>();

            var carIds = bookings.Select(b => b.CarId).Distinct().ToList();
            var cars = await carRepo.FindAsync(c => carIds.Contains(c.Id));
            var carMap = cars.ToDictionary(c => c.Id, c => c);

            var serviceIds = filtered.Select(i => i.ServiceId).Distinct().ToList();
            var services = await serviceRepo.FindAsync(s => serviceIds.Contains(s.Id));
            var svcMap = services.ToDictionary(s => s.Id, s => s);

            List<EmployeeTaskResponse> MapItems(IEnumerable<BookingItem> src)
                => src.OrderBy(i => bookingMap[i.BookingId].ScheduledStart)
                      .Select(i =>
                      {
                          var b = bookingMap[i.BookingId];
                          carMap.TryGetValue(b.CarId, out var car);
                          svcMap.TryGetValue(i.ServiceId, out var svc);

                          return new EmployeeTaskResponse
                          {
                              BookingItemId = i.Id,
                              BookingId = i.BookingId,
                              ScheduledStart = b.ScheduledStart,
                              ClientId = b.ClientId,
                              CarId = b.CarId,
                              PlateNumber = car?.PlateNumber ?? "",
                              ServiceId = i.ServiceId,
                              ServiceName = svc?.Name ?? "",
                              ItemStatus = i.Status
                          };
                      })
                      .ToList();

            var availableItems = filtered.Where(i => i.Status == BookingItemStatus.Pending && i.AssignedEmployeeId == null);
            var myActiveItems = filtered.Where(i => i.Status == BookingItemStatus.InProgress && i.AssignedEmployeeId == employeeId);

            return new EmployeeTasksPageResponse
            {
                EmployeeId = employeeId,
                Date = date,
                Available = MapItems(availableItems),
                MyActive = MapItems(myActiveItems)
            };
        }


    }

}
