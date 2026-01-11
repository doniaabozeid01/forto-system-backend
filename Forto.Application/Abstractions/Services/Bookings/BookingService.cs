using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Bookings;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Clients;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Forto.Application.Abstractions.Services.Invoices;
using Forto.Application.DTOs.Billings;

namespace Forto.Application.Abstractions.Services.Bookings
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _uow;
        readonly IInvoiceService _invoiceService;

        public BookingService(IUnitOfWork uow, IInvoiceService invoiceService) {
            _invoiceService = invoiceService;
            _uow = uow;
        }

        public async Task<AvailableSlotsResponse> GetAvailableSlotsAsync(int branchId, DateOnly date, int carId, List<int> serviceIds)
        {
            // Validate branch
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            // Validate car exists
            var car = await _uow.Repository<Car>().GetByIdAsync(carId);
            if (car == null)
                throw new BusinessException("Car not found", 404);

            // Validate services + rates exist for body type
            var svcIds = serviceIds.Distinct().ToList();
            if (svcIds.Count == 0)
                throw new BusinessException("ServiceIds is required", 400);

            await EnsureRatesExistForBodyType(svcIds, car.BodyType);

            // Staff check (MVP): for each service, at least ONE qualified employee working that hour
            // We'll evaluate for each hour slot.

            // Build list of hours (example 9 AM -> 9 PM). You can adjust later.
            //var hours = Enumerable.Range(9, 12).Select(h => new TimeOnly(h, 0)).ToList(); // 09:00 -> 20:00
            // build hours dynamically from schedules+shifts for that date
            var dow = date.DayOfWeek;

            var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
            var shiftRepo = _uow.Repository<Domain.Entities.Employees.Shift>();

            var schedules = await scheduleRepo.FindAsync(s => s.DayOfWeek == dow && !s.IsOff);

            if (schedules.Count == 0)
            {
                return new AvailableSlotsResponse
                {
                    Date = date,
                    CapacityPerHour = branch.CapacityPerHour,
                    Slots = new List<HourSlotDto>()
                };
            }

            // load shifts referenced
            var shiftIds = schedules.Where(s => s.ShiftId.HasValue).Select(s => s.ShiftId!.Value).Distinct().ToList();
            var shifts = shiftIds.Count == 0 ? new List<Domain.Entities.Employees.Shift>() : (await shiftRepo.FindAsync(x => shiftIds.Contains(x.Id))).ToList();
            var shiftMap = shifts.ToDictionary(x => x.Id, x => x);

            TimeOnly? minStart = null;
            TimeOnly? maxEnd = null;

            foreach (var s in schedules)
            {
                TimeOnly? start = s.StartTime;
                TimeOnly? end = s.EndTime;

                if (s.ShiftId.HasValue && shiftMap.TryGetValue(s.ShiftId.Value, out var sh))
                {
                    start ??= sh.StartTime;
                    end ??= sh.EndTime;
                }

                if (start == null || end == null) continue;

                minStart = minStart == null || start < minStart ? start : minStart;
                maxEnd = maxEnd == null || end > maxEnd ? end : maxEnd;
            }

            if (minStart == null || maxEnd == null || minStart >= maxEnd)
            {
                return new AvailableSlotsResponse
                {
                    Date = date,
                    CapacityPerHour = branch.CapacityPerHour,
                    Slots = new List<HourSlotDto>()
                };
            }

            // hours from minStart to maxEnd (exclusive)
            var hours = new List<TimeOnly>();
            for (var t = minStart.Value; t < maxEnd.Value; t = t.AddHours(1))
                hours.Add(t);

            // Get employees working this day (from schedules)
            // MVP schedule logic: employee has schedule row for DayOfWeek and IsOff=false and time covers hour
            // We'll query schedules per hour.

            var result = new AvailableSlotsResponse
            {
                Date = date,
                CapacityPerHour = branch.CapacityPerHour
            };

            foreach (var hour in hours)
            {
                var slotStart = date.ToDateTime(hour);

                // Capacity check: count bookings in this hour
                var bookingRepo = _uow.Repository<Booking>();
                var bookedCount = (await bookingRepo.FindAsync(b =>
                    b.BranchId == branchId &&
                    b.SlotHourStart == slotStart &&
                    b.Status != BookingStatus.Cancelled)).Count;

                var available = branch.CapacityPerHour - bookedCount;
                if (available <= 0)
                {
                    result.Slots.Add(new HourSlotDto { Hour = hour, Booked = bookedCount, Available = 0 });
                    continue;
                }

                // Staff qualification check for this slot:
                var ok = await HasQualifiedStaffForAllServices(slotStart, svcIds);
                if (!ok)
                {
                    // we treat as unavailable
                    result.Slots.Add(new HourSlotDto { Hour = hour, Booked = bookedCount, Available = 0 });
                    continue;
                }

                result.Slots.Add(new HourSlotDto { Hour = hour, Booked = bookedCount, Available = available });
            }

            return result;
        }

        public async Task<BookingResponse> CreateAsync(CreateBookingRequest request)
        {
            // Validate branch
            var branch = await _uow.Repository<Branch>().GetByIdAsync(request.BranchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            // Validate client + car
            var client = await _uow.Repository<Client>().GetByIdAsync(request.ClientId);
            if (client == null)
                throw new BusinessException("Client not found", 404);

            var car = await _uow.Repository<Car>().GetByIdAsync(request.CarId);
            if (car == null || car.ClientId != client.Id)
                throw new BusinessException("Car not found for this client", 404);

            // normalize to hour slot
            var slotHourStart = new DateTime(request.ScheduledStart.Year, request.ScheduledStart.Month, request.ScheduledStart.Day,
                request.ScheduledStart.Hour, 0, 0);

            // Enforce hour booking only (optional)
            if (request.ScheduledStart.Minute != 0 || request.ScheduledStart.Second != 0)
                throw new BusinessException("ScheduledStart must be on the hour (e.g. 14:00)", 400);

            // Capacity check
            var bookingRepo = _uow.Repository<Booking>();
            var bookedCount = (await bookingRepo.FindAsync(b =>
                b.BranchId == request.BranchId &&
                b.SlotHourStart == slotHourStart &&
                b.Status != BookingStatus.Cancelled)).Count;

            if (bookedCount >= branch.CapacityPerHour)
                throw new BusinessException("No capacity available for this hour", 409);

            var serviceIds = request.ServiceIds.Distinct().ToList();
            await EnsureRatesExistForBodyType(serviceIds, car.BodyType);

            // Staff check: for each service, at least one qualified employee exists working at this hour
            var staffOk = await HasQualifiedStaffForAllServices(slotHourStart, serviceIds);
            if (!staffOk)
                throw new BusinessException("No qualified employees available for one or more selected services at this time", 409);

            // Build booking items with snapshots from rates
            var rateRepo = _uow.Repository<ServiceRate>();
            var rates = await rateRepo.FindAsync(r => r.IsActive && serviceIds.Contains(r.ServiceId) && r.BodyType == car.BodyType);
            var rateMap = rates.ToDictionary(r => r.ServiceId, r => r);

            var items = new List<BookingItem>();
            foreach (var sid in serviceIds)
            {
                var rate = rateMap[sid];

                items.Add(new BookingItem
                {
                    ServiceId = sid,
                    BodyType = car.BodyType,
                    UnitPrice = rate.Price,
                    DurationMinutes = rate.DurationMinutes,
                    Status = BookingItemStatus.Pending
                });
            }

            var totalPrice = items.Sum(i => i.UnitPrice);
            var totalDuration = items.Sum(i => i.DurationMinutes); // MVP: sum

            var booking = new Booking
            {
                BranchId = request.BranchId,
                ClientId = client.Id,
                CarId = car.Id,
                ScheduledStart = request.ScheduledStart,
                SlotHourStart = slotHourStart,
                TotalPrice = totalPrice,
                EstimatedDurationMinutes = totalDuration,
                Status = BookingStatus.Pending,
                Notes = request.Notes?.Trim(),
                Items = items
            };

            await bookingRepo.AddAsync(booking);
            await _uow.SaveChangesAsync();

            return Map(booking);
        }

        public async Task<BookingResponse?> GetByIdAsync(int bookingId)
        {
            var repo = _uow.Repository<Booking>();
            var booking = await repo.GetByIdAsync(bookingId);
            if (booking == null) return null;

            // GenericRepo ما بيجيبش Items، فهنجيب items query منفصلة
            var items = await _uow.Repository<BookingItem>().FindAsync(i => i.BookingId == bookingId);
            booking.Items = items.ToList();

            return Map(booking);
        }

        public async Task<BookingItemResponse> StartItemAsync(int itemId, int employeeId)
        {
            var itemRepo = _uow.Repository<BookingItem>();
            var bookingRepo = _uow.Repository<Booking>();

            var item = await itemRepo.GetByIdAsync(itemId);
            if (item == null)
                throw new BusinessException("Booking item not found", 404);

            if (item.Status != BookingItemStatus.Pending)
                throw new BusinessException("Item cannot be started in its current status", 409);

            // Validate employee exists + active
            var emp = await _uow.Repository<Employee>().GetByIdAsync(employeeId);
            if (emp == null || !emp.IsActive)
                throw new BusinessException("Employee not found", 404);

            // ✅ أهم شرط عندك: employee must be qualified for this service
            var linkRepo = _uow.Repository<EmployeeService>();
            var qualified = await linkRepo.AnyAsync(x => x.EmployeeId == employeeId && x.ServiceId == item.ServiceId && x.IsActive);
            if (!qualified)
                throw new BusinessException("Employee is not qualified for this service", 409);

            // Assign + start
            item.AssignedEmployeeId = employeeId;
            item.Status = BookingItemStatus.InProgress;
            item.StartedAt = DateTime.UtcNow;

            itemRepo.Update(item);

            // Update booking status if needed
            var booking = await bookingRepo.GetByIdAsync(item.BookingId);
            if (booking == null)
                throw new BusinessException("Booking not found", 404);

            if (booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.InProgress;
                bookingRepo.Update(booking);
            }

            //await _uow.SaveChangesAsync();
            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException("This service item was already taken/updated by another employee.", 409);
            }


            return MapItem(item);
        }

        //public async Task<BookingItemResponse> CompleteItemAsync(int itemId, int employeeId)
        //{
        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var bookingRepo = _uow.Repository<Booking>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null)
        //        throw new BusinessException("Booking item not found", 404);

        //    if (item.Status != BookingItemStatus.InProgress)
        //        throw new BusinessException("Item cannot be completed in its current status", 409);

        //    // only the assigned employee can complete (or admin later)
        //    if (item.AssignedEmployeeId != employeeId)
        //        throw new BusinessException("Only the assigned employee can complete this item", 403);

        //    item.Status = BookingItemStatus.Done;
        //    item.CompletedAt = DateTime.UtcNow;

        //    itemRepo.Update(item);

        //    // If all items done -> booking completed
        //    var allItems = await itemRepo.FindAsync(i => i.BookingId == item.BookingId);
        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (booking == null)
        //        throw new BusinessException("Booking not found", 404);

        //    if (allItems.All(i => i.Id == item.Id ? true : i.Status == BookingItemStatus.Done))
        //    {
        //        booking.Status = BookingStatus.Completed;
        //        booking.CompletedAt = DateTime.UtcNow;
        //        bookingRepo.Update(booking);
        //    }

        //    //await _uow.SaveChangesAsync();

        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("This service item was already taken/updated by another employee.", 409);
        //    }


        //    return MapItem(item);
        //}











public async Task<BookingItemResponse> CompleteItemAsync(int itemId, int employeeId)
    {
        var itemRepo = _uow.Repository<BookingItem>();
        var bookingRepo = _uow.Repository<Booking>();

        var item = await itemRepo.GetByIdAsync(itemId);
        if (item == null)
            throw new BusinessException("Booking item not found", 404);

        if (item.Status != BookingItemStatus.InProgress)
            throw new BusinessException("Item cannot be completed in its current status", 409);

        if (item.AssignedEmployeeId != employeeId)
            throw new BusinessException("Only the assigned employee can complete this item", 403);

        // ✅ mark done
        item.Status = BookingItemStatus.Done;
        item.CompletedAt = DateTime.UtcNow;
        itemRepo.Update(item);

        // load booking + all items
        var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        if (booking == null)
            throw new BusinessException("Booking not found", 404);

        var allItems = await itemRepo.FindAsync(i => i.BookingId == item.BookingId);

        // ✅ condition: Done OR Cancelled
        var completed = allItems.All(i =>
            i.Id == item.Id
                ? true
                : (i.Status == BookingItemStatus.Done || i.Status == BookingItemStatus.Cancelled)
        );

        if (completed)
        {
            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = DateTime.UtcNow;
            bookingRepo.Update(booking);
        }

        try
        {
            await _uow.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessException("This service item was already updated by another employee. Please retry.", 409);
        }

        // ✅ after save: if booking completed -> create invoice (idempotent)
        if (completed)
        {
            await _invoiceService.EnsureInvoiceForBookingAsync(item.BookingId);
        }

        return MapItem(item);
    }


    // ---------------- helpers ----------------

    private async Task EnsureRatesExistForBodyType(List<int> serviceIds, CarBodyType bodyType)
        {
            var rateRepo = _uow.Repository<ServiceRate>();
            var rates = await rateRepo.FindAsync(r => r.IsActive && serviceIds.Contains(r.ServiceId) && r.BodyType == bodyType);

            var found = rates.Select(r => r.ServiceId).ToHashSet();
            var missing = serviceIds.Where(id => !found.Contains(id)).ToList();
            if (missing.Any())
                throw new BusinessException("Missing service rates for this car type", 400,
                    new Dictionary<string, string[]>
                    {
                        ["serviceIds"] = missing.Select(x => $"No rate for serviceId {x} with bodyType {bodyType}").ToArray()
                    });
        }

        private async Task<bool> HasQualifiedStaffForAllServices(DateTime slotStart, List<int> serviceIds)
        {
            // 1) get employees working at this hour
            var day = slotStart.DayOfWeek;

            var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
            var empRepo = _uow.Repository<Employee>();

            var schedules = await scheduleRepo.FindAsync(s =>
                s.DayOfWeek == day &&
                !s.IsOff);

            // filter schedules that cover this hour
            // If schedule has ShiftId -> use shift time, else Start/End
            var shiftRepo = _uow.Repository<Domain.Entities.Employees.Shift>();
            var shiftIds = schedules.Where(s => s.ShiftId.HasValue).Select(s => s.ShiftId!.Value).Distinct().ToList();
            var shifts = shiftIds.Count == 0 ? new List<Domain.Entities.Employees.Shift>() : (await shiftRepo.FindAsync(x => shiftIds.Contains(x.Id))).ToList();
            var shiftMap = shifts.ToDictionary(x => x.Id, x => x);

            var hour = TimeOnly.FromDateTime(slotStart);
            var workingEmployeeIds = new HashSet<int>();

            foreach (var s in schedules)
            {
                TimeOnly? start = s.StartTime;
                TimeOnly? end = s.EndTime;

                if (s.ShiftId.HasValue && shiftMap.TryGetValue(s.ShiftId.Value, out var sh))
                {
                    start ??= sh.StartTime;
                    end ??= sh.EndTime;
                }

                if (start == null || end == null) continue;

                // MVP: assume shifts do NOT cross midnight for now
                if (hour >= start && hour < end)
                    workingEmployeeIds.Add(s.EmployeeId);
            }

            if (workingEmployeeIds.Count == 0)
                return false;

            // 2) from those employees, check qualification for each service
            var linkRepo = _uow.Repository<EmployeeService>();
            var links = await linkRepo.FindAsync(l => l.IsActive && workingEmployeeIds.Contains(l.EmployeeId));

            var map = links.GroupBy(l => l.ServiceId).ToDictionary(g => g.Key, g => g.Select(x => x.EmployeeId).ToHashSet());

            foreach (var sid in serviceIds)
            {
                if (!map.TryGetValue(sid, out var empsForService) || empsForService.Count == 0)
                    return false;
            }

            return true;
        }

        private static BookingResponse Map(Booking b) => new()
        {
            Id = b.Id,
            BranchId = b.BranchId,
            ClientId = b.ClientId,
            CarId = b.CarId,
            ScheduledStart = b.ScheduledStart,
            SlotHourStart = b.SlotHourStart,
            TotalPrice = b.TotalPrice,
            EstimatedDurationMinutes = b.EstimatedDurationMinutes,
            Status = b.Status,
            Items = b.Items.Select(MapItem).ToList()
        };

        private static BookingItemResponse MapItem(BookingItem i) => new()
        {
            Id = i.Id,
            ServiceId = i.ServiceId,
            BodyType = i.BodyType,
            UnitPrice = i.UnitPrice,
            DurationMinutes = i.DurationMinutes,
            Status = i.Status,
            AssignedEmployeeId = i.AssignedEmployeeId
        };





















    }

}