using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Bookings;
using Forto.Domain.Entities.Billings;
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
using Forto.Domain.Entities.Inventory;
using Forto.Application.Abstractions.Services.Bookings.Closing;
using Forto.Application.Abstractions.Services.Schedule;
using Forto.Application.DTOs.Bookings.ClientBooking;
using Forto.Application.DTOs.Ops.Usage;
using Forto.Application.DTOs.Inventory.Materials;

namespace Forto.Application.Abstractions.Services.Bookings
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _uow;
        readonly IInvoiceService _invoiceService;
        private readonly IBookingClosingService _closingService;
        private readonly IEmployeeScheduleService _scheduleService;

        public BookingService(IUnitOfWork uow, IInvoiceService invoiceService, IBookingClosingService closingService, IEmployeeScheduleService scheduleService) {
            _invoiceService = invoiceService;
            _closingService = closingService;
            _scheduleService = scheduleService;
            _uow = uow;
        }


        //public async Task<AvailableSlotsResponse> GetAvailableSlotsAsync(int branchId, DateOnly date, int carId, List<int> serviceIds)
        //{
        //    // Validate branch
        //    var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
        //    if (branch == null || !branch.IsActive)
        //        throw new BusinessException("Branch not found", 404);

        //    // Validate car exists
        //    var car = await _uow.Repository<Car>().GetByIdAsync(carId);
        //    if (car == null)
        //        throw new BusinessException("Car not found", 404);

        //    // Validate services + rates exist for body type
        //    var svcIds = serviceIds.Distinct().ToList();
        //    if (svcIds.Count == 0)
        //        throw new BusinessException("ServiceIds is required", 400);

        //    await EnsureRatesExistForBodyType(svcIds, car.BodyType);

        //    // Staff check (MVP): for each service, at least ONE qualified employee working that hour
        //    // We'll evaluate for each hour slot.

        //    // Build list of hours (example 9 AM -> 9 PM). You can adjust later.
        //    //var hours = Enumerable.Range(9, 12).Select(h => new TimeOnly(h, 0)).ToList(); // 09:00 -> 20:00
        //    // build hours dynamically from schedules+shifts for that date
        //    var dow = date.DayOfWeek;

        //    var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
        //    var shiftRepo = _uow.Repository<Domain.Entities.Employees.Shift>();

        //    var schedules = await scheduleRepo.FindAsync(s => s.DayOfWeek == dow && !s.IsOff);

        //    if (schedules.Count == 0)
        //    {
        //        return new AvailableSlotsResponse
        //        {
        //            Date = date,
        //            CapacityPerHour = branch.CapacityPerHour,
        //            Slots = new List<HourSlotDto>()
        //        };
        //    }

        //    // load shifts referenced
        //    var shiftIds = schedules.Where(s => s.ShiftId.HasValue).Select(s => s.ShiftId!.Value).Distinct().ToList();
        //    var shifts = shiftIds.Count == 0 ? new List<Domain.Entities.Employees.Shift>() : (await shiftRepo.FindAsync(x => shiftIds.Contains(x.Id))).ToList();
        //    var shiftMap = shifts.ToDictionary(x => x.Id, x => x);

        //    TimeOnly? minStart = null;
        //    TimeOnly? maxEnd = null;

        //    foreach (var s in schedules)
        //    {
        //        TimeOnly? start = s.StartTime;
        //        TimeOnly? end = s.EndTime;

        //        if (s.ShiftId.HasValue && shiftMap.TryGetValue(s.ShiftId.Value, out var sh))
        //        {
        //            start ??= sh.StartTime;
        //            end ??= sh.EndTime;
        //        }

        //        if (start == null || end == null) continue;

        //        minStart = minStart == null || start < minStart ? start : minStart;
        //        maxEnd = maxEnd == null || end > maxEnd ? end : maxEnd;
        //    }

        //    if (minStart == null || maxEnd == null || minStart >= maxEnd)
        //    {
        //        return new AvailableSlotsResponse
        //        {
        //            Date = date,
        //            CapacityPerHour = branch.CapacityPerHour,
        //            Slots = new List<HourSlotDto>()
        //        };
        //    }

        //    // hours from minStart to maxEnd (exclusive)
        //    var hours = new List<TimeOnly>();
        //    for (var t = minStart.Value; t < maxEnd.Value; t = t.AddHours(1))
        //        hours.Add(t);

        //    // Get employees working this day (from schedules)
        //    // MVP schedule logic: employee has schedule row for DayOfWeek and IsOff=false and time covers hour
        //    // We'll query schedules per hour.

        //    var result = new AvailableSlotsResponse
        //    {
        //        Date = date,
        //        CapacityPerHour = branch.CapacityPerHour
        //    };

        //    foreach (var hour in hours)
        //    {
        //        var slotStart = date.ToDateTime(hour);

        //        // Capacity check: count bookings in this hour
        //        var bookingRepo = _uow.Repository<Booking>();
        //        var bookedCount = (await bookingRepo.FindAsync(b =>
        //            b.BranchId == branchId &&
        //            b.SlotHourStart == slotStart &&
        //            b.Status != BookingStatus.Cancelled)).Count;

        //        var available = branch.CapacityPerHour - bookedCount;
        //        if (available <= 0)
        //        {
        //            result.Slots.Add(new HourSlotDto { Hour = hour, Booked = bookedCount, Available = 0 });
        //            continue;
        //        }

        //        // Staff qualification check for this slot:
        //        var ok = await HasQualifiedStaffForAllServices(slotStart, svcIds);
        //        if (!ok)
        //        {
        //            // we treat as unavailable
        //            result.Slots.Add(new HourSlotDto { Hour = hour, Booked = bookedCount, Available = 0 });
        //            continue;
        //        }

        //        result.Slots.Add(new HourSlotDto { Hour = hour, Booked = bookedCount, Available = available });
        //    }

        //    return result;
        //}






















        //        public async Task<AvailableSlotsResponse> GetAvailableSlotsAsync(
        //    int branchId,
        //    DateOnly date,
        //    List<int> serviceIds
        //)
        //        {
        //            // 1) Validate branch
        //            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
        //            if (branch == null || !branch.IsActive)
        //                throw new BusinessException("Branch not found", 404);

        //            // 2) Validate services
        //            var svcIds = serviceIds.Distinct().ToList();
        //            if (svcIds.Count == 0)
        //                throw new BusinessException("ServiceIds is required", 400);

        //            // 3) Load schedules for that day
        //            var dow = date.DayOfWeek;

        //            var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
        //            var shiftRepo = _uow.Repository<Domain.Entities.Employees.Shift>();

        //            var schedules = await scheduleRepo.FindAsync(
        //                s => s.DayOfWeek == dow && !s.IsOff
        //            );

        //            if (schedules.Count == 0)
        //            {
        //                return new AvailableSlotsResponse
        //                {
        //                    Date = date,
        //                    CapacityPerHour = branch.CapacityPerHour,
        //                    Slots = new List<HourSlotDto>()
        //                };
        //            }

        //            // 4) Resolve shift times
        //            var shiftIds = schedules
        //                .Where(s => s.ShiftId.HasValue)
        //                .Select(s => s.ShiftId!.Value)
        //                .Distinct()
        //                .ToList();

        //            var shifts = shiftIds.Count == 0
        //                ? new List<Domain.Entities.Employees.Shift>()
        //                : (await shiftRepo.FindAsync(x => shiftIds.Contains(x.Id))).ToList();

        //            var shiftMap = shifts.ToDictionary(x => x.Id, x => x);

        //            TimeOnly? minStart = null;
        //            TimeOnly? maxEnd = null;

        //            foreach (var s in schedules)
        //            {
        //                TimeOnly? start = s.StartTime;
        //                TimeOnly? end = s.EndTime;

        //                if (s.ShiftId.HasValue && shiftMap.TryGetValue(s.ShiftId.Value, out var sh))
        //                {
        //                    start ??= sh.StartTime;
        //                    end ??= sh.EndTime;
        //                }

        //                if (start == null || end == null) continue;

        //                minStart = minStart == null || start < minStart ? start : minStart;
        //                maxEnd = maxEnd == null || end > maxEnd ? end : maxEnd;
        //            }

        //            if (minStart == null || maxEnd == null || minStart >= maxEnd)
        //            {
        //                return new AvailableSlotsResponse
        //                {
        //                    Date = date,
        //                    CapacityPerHour = branch.CapacityPerHour,
        //                    Slots = new List<HourSlotDto>()
        //                };
        //            }

        //            // 5) Build hour slots
        //            var hours = new List<TimeOnly>();
        //            for (var t = minStart.Value; t < maxEnd.Value; t = t.AddHours(1))
        //                hours.Add(t);

        //            var bookingRepo = _uow.Repository<Booking>();

        //            var result = new AvailableSlotsResponse
        //            {
        //                Date = date,
        //                CapacityPerHour = branch.CapacityPerHour,
        //                Slots = new List<HourSlotDto>()
        //            };

        //            // 6) Evaluate each hour
        //            foreach (var hour in hours)
        //            {
        //                var slotStart = date.ToDateTime(hour);

        //                // Capacity check
        //                var bookedCount = (await bookingRepo.FindAsync(b =>
        //                    b.BranchId == branchId &&
        //                    b.SlotHourStart == slotStart &&
        //                    b.Status != BookingStatus.Cancelled
        //                )).Count;

        //                var available = branch.CapacityPerHour - bookedCount;
        //                if (available <= 0)
        //                {
        //                    result.Slots.Add(new HourSlotDto
        //                    {
        //                        Hour = hour,
        //                        Booked = bookedCount,
        //                        Available = 0
        //                    });
        //                    continue;
        //                }

        //                // Staff availability check (core logic)
        //                var hasStaff = await HasQualifiedStaffForAllServices(slotStart, svcIds);
        //                if (!hasStaff)
        //                {
        //                    result.Slots.Add(new HourSlotDto
        //                    {
        //                        Hour = hour,
        //                        Booked = bookedCount,
        //                        Available = 0
        //                    });
        //                    continue;
        //                }

        //                result.Slots.Add(new HourSlotDto
        //                {
        //                    Hour = hour,
        //                    Booked = bookedCount,
        //                    Available = available
        //                });
        //            }

        //            return result;
        //        }



































        public async Task<AvailableSlotsResponse> GetAvailableSlotsAsync(int branchId, DateOnly date, List<int> serviceIds)
        {
            // 1) Validate branch
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            // 2) Validate services (IMPORTANT: null guard)
            if (serviceIds == null || serviceIds.Count == 0)
                throw new BusinessException("ServiceIds is required", 400);

            var svcIds = serviceIds.Distinct().ToList();

            // 3) Always return the full 24 hours — each slot shows how many booked vs available
            var hours = new List<TimeOnly>(24);
            for (var h = 0; h < 24; h++)
                hours.Add(new TimeOnly(h, 0));

            // 4) Load bookings for that day ONCE (performance + avoid random 500)
            var bookingRepo = _uow.Repository<Booking>();

            var dayStart = date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var dayBookings = await bookingRepo.FindAsync(b =>
                b.BranchId == branchId &&
                b.ScheduledStart >= dayStart &&
                b.ScheduledStart < dayEnd &&
                b.Status != BookingStatus.Cancelled);

            // group booked counts by SlotHourStart (guard against null from FindAsync)
            var bookedMap = (dayBookings ?? Enumerable.Empty<Booking>())
                .GroupBy(b => b.SlotHourStart)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new AvailableSlotsResponse
            {
                Date = date,
                CapacityPerHour = branch.CapacityPerHour,
                Slots = new List<HourSlotDto>()
            };

            // 5) Evaluate each hour: booked count, available = capacity - booked (0 if no staff)
            if (hours == null)
                hours = Enumerable.Range(0, 24).Select(h => new TimeOnly(h, 0)).ToList();

            foreach (var hour in hours)
            {
                var slotStart = date.ToDateTime(hour);

                bookedMap.TryGetValue(slotStart, out var bookedCount);

                var available = branch.CapacityPerHour - bookedCount;
                if (available <= 0)
                {
                    result.Slots.Add(new HourSlotDto
                    {
                        Hour = hour,
                        Booked = bookedCount,
                        Available = 0
                    });
                    continue;
                }

                // Staff check for this hour (your existing logic)
                var hasStaff = await HasQualifiedStaffForAllServices(slotStart, svcIds);
                if (!hasStaff)
                {
                    result.Slots.Add(new HourSlotDto
                    {
                        Hour = hour,
                        Booked = bookedCount,
                        Available = 0
                    });
                    continue;
                }

                result.Slots.Add(new HourSlotDto
                {
                    Hour = hour,
                    Booked = bookedCount,
                    Available = available
                });
            }

            return result;
        }




































        //public async Task<BookingResponse> CreateAsync(CreateBookingRequest request)
        //{
        //    // Validate branch
        //    var branch = await _uow.Repository<Branch>().GetByIdAsync(request.BranchId);
        //    if (branch == null || !branch.IsActive)
        //        throw new BusinessException("Branch not found", 404);

        //    // Validate client + car
        //    var client = await _uow.Repository<Client>().GetByIdAsync(request.ClientId);
        //    if (client == null)
        //        throw new BusinessException("Client not found", 404);

        //    var car = await _uow.Repository<Car>().GetByIdAsync(request.CarId);
        //    if (car == null || car.ClientId != client.Id)
        //        throw new BusinessException("Car not found for this client", 404);

        //    // normalize to hour slot
        //    var slotHourStart = new DateTime(request.ScheduledStart.Year, request.ScheduledStart.Month, request.ScheduledStart.Day,
        //        request.ScheduledStart.Hour, 0, 0);

        //    // Enforce hour booking only (optional)
        //    if (request.ScheduledStart.Minute != 0 || request.ScheduledStart.Second != 0)
        //        throw new BusinessException("ScheduledStart must be on the hour (e.g. 14:00)", 400);

        //    // Capacity check
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var bookedCount = (await bookingRepo.FindAsync(b =>
        //        b.BranchId == request.BranchId &&
        //        b.SlotHourStart == slotHourStart &&
        //        b.Status != BookingStatus.Cancelled)).Count;

        //    if (bookedCount >= branch.CapacityPerHour)
        //        throw new BusinessException("No capacity available for this hour", 409);

        //    var serviceIds = request.ServiceIds.Distinct().ToList();
        //    await EnsureRatesExistForBodyType(serviceIds, car.BodyType);

        //    // Staff check: for each service, at least one qualified employee exists working at this hour
        //    var staffOk = await HasQualifiedStaffForAllServices(slotHourStart, serviceIds);
        //    if (!staffOk)
        //        throw new BusinessException("No qualified employees available for one or more selected services at this time", 409);

        //    // Build booking items with snapshots from rates
        //    var rateRepo = _uow.Repository<ServiceRate>();
        //    var rates = await rateRepo.FindAsync(r => r.IsActive && serviceIds.Contains(r.ServiceId) && r.BodyType == car.BodyType);
        //    var rateMap = rates.ToDictionary(r => r.ServiceId, r => r);

        //    var items = new List<BookingItem>();
        //    foreach (var sid in serviceIds)
        //    {
        //        var rate = rateMap[sid];

        //        items.Add(new BookingItem
        //        {
        //            ServiceId = sid,
        //            BodyType = car.BodyType,
        //            UnitPrice = rate.Price,
        //            DurationMinutes = rate.DurationMinutes,
        //            Status = BookingItemStatus.Pending
        //        });
        //    }

        //    var totalPrice = items.Sum(i => i.UnitPrice);
        //    var totalDuration = items.Sum(i => i.DurationMinutes); // MVP: sum

        //    var booking = new Booking
        //    {
        //        BranchId = request.BranchId,
        //        ClientId = client.Id,
        //        CarId = car.Id,
        //        ScheduledStart = request.ScheduledStart,
        //        SlotHourStart = slotHourStart,
        //        TotalPrice = totalPrice,
        //        EstimatedDurationMinutes = totalDuration,
        //        Status = BookingStatus.Pending,
        //        Notes = request.Notes?.Trim(),
        //        Items = items
        //    };

        //    await bookingRepo.AddAsync(booking);
        //    await _uow.SaveChangesAsync();

        //    return Map(booking);
        //}







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

            // ✅ Validate creator (who created this booking)
            int? createdByEmployeeId = null;
            int? createdByClientId = null;

            if (request.CreatedByType == BookingCreatedByType.Employee)
            {
                if (!request.CreatedByEmployeeId.HasValue)
                    throw new BusinessException("CreatedByEmployeeId is required", 400);

                var emp = await _uow.Repository<Employee>().GetByIdAsync(request.CreatedByEmployeeId.Value);
                if (emp == null || !emp.IsActive)
                    throw new BusinessException("Employee not found", 404);

                // Optional: restrict who can create bookings
                if (emp.Role != EmployeeRole.Cashier && emp.Role != EmployeeRole.Supervisor && emp.Role != EmployeeRole.Admin)
                    throw new BusinessException("Employee is not allowed to create bookings", 403);

                createdByEmployeeId = emp.Id;
                createdByClientId = null;
            }
            else if (request.CreatedByType == BookingCreatedByType.Client)
            {
                // لو UI مش بيبعت CreatedByClientId، نقدر نعتبره نفس clientId بتاع الحجز
                // لكن لو تحبي تلزميه ابعتيه، سيبي السطرين دول:
                createdByClientId = request.CreatedByClientId ?? client.Id;
                createdByEmployeeId = null;

                // (اختياري) تأكيد إن CreatedByClientId هو نفس العميل بتاع الحجز
                if (createdByClientId != client.Id)
                    throw new BusinessException("CreatedByClientId must match booking client", 400);
            }
            else
            {
                throw new BusinessException("CreatedByType is required", 400);
            }

            // normalize to hour slot
            var slotHourStart = new DateTime(
                request.ScheduledStart.Year, request.ScheduledStart.Month, request.ScheduledStart.Day,
                request.ScheduledStart.Hour, 0, 0);

            // Enforce hour booking only
            if (request.ScheduledStart.Minute != 0 || request.ScheduledStart.Second != 0)
                throw new BusinessException("ScheduledStart must be on the hour (e.g. 14:00)", 400);

            // Capacity check
            var bookingRepo = _uow.Repository<Booking>();
            //var bookedCount = (await bookingRepo.FindAsync(b =>
            //    b.BranchId == request.BranchId &&
            //    b.SlotHourStart == slotHourStart &&
            //    b.Status != BookingStatus.Cancelled )).Count;
            var bookedCount = (await bookingRepo.FindAsync(b =>
                b.BranchId == request.BranchId &&
                b.SlotHourStart == slotHourStart &&
                b.Status == BookingStatus.InProgress)).Count;

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
            var totalDuration = items.Sum(i => i.DurationMinutes);

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
                Items = items,

                // ✅ creator info stored
                CreatedByType = request.CreatedByType,
                CreatedByEmployeeId = createdByEmployeeId,
                CreatedByClientId = createdByClientId
            };

            await bookingRepo.AddAsync(booking);
            await _uow.SaveChangesAsync();

            return Map(booking);
        }







        // ----- Checkout Create Async
        public async Task<BookingResponse> CheckoutCreateAsync(CreateBookingRequest request)
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

            // ✅ Validate creator (who created this booking)
            int? createdByEmployeeId = null;
            int? createdByClientId = null;

            if (request.CreatedByType == BookingCreatedByType.Employee)
            {
                if (!request.CreatedByEmployeeId.HasValue)
                    throw new BusinessException("CreatedByEmployeeId is required", 400);

                var emp = await _uow.Repository<Employee>().GetByIdAsync(request.CreatedByEmployeeId.Value);
                if (emp == null || !emp.IsActive)
                    throw new BusinessException("Employee not found", 404);

                // Optional: restrict who can create bookings
                if (emp.Role != EmployeeRole.Cashier && emp.Role != EmployeeRole.Supervisor && emp.Role != EmployeeRole.Admin)
                    throw new BusinessException("Employee is not allowed to create bookings", 403);

                createdByEmployeeId = emp.Id;
                createdByClientId = null;
            }
            else if (request.CreatedByType == BookingCreatedByType.Client)
            {
                // لو UI مش بيبعت CreatedByClientId، نقدر نعتبره نفس clientId بتاع الحجز
                // لكن لو تحبي تلزميه ابعتيه، سيبي السطرين دول:
                createdByClientId = request.CreatedByClientId ?? client.Id;
                createdByEmployeeId = null;

                // (اختياري) تأكيد إن CreatedByClientId هو نفس العميل بتاع الحجز
                if (createdByClientId != client.Id)
                    throw new BusinessException("CreatedByClientId must match booking client", 400);
            }
            else
            {
                throw new BusinessException("CreatedByType is required", 400);
            }

            // normalize to hour slot
            var slotHourStart = new DateTime(
                request.ScheduledStart.Year, request.ScheduledStart.Month, request.ScheduledStart.Day,
                request.ScheduledStart.Hour, 0, 0);

            // Enforce hour booking only
            //if (request.ScheduledStart.Minute != 0 || request.ScheduledStart.Second != 0)
            //    throw new BusinessException("ScheduledStart must be on the hour (e.g. 14:00)", 400);

            // Capacity check
            var bookingRepo = _uow.Repository<Booking>();
            //var bookedCount = (await bookingRepo.FindAsync(b =>
            //    b.BranchId == request.BranchId &&
            //    b.SlotHourStart == slotHourStart &&
            //    b.Status != BookingStatus.Cancelled)).Count;

            //if (bookedCount >= branch.CapacityPerHour)
            //    throw new BusinessException("No capacity available for this hour", 409);

            var serviceIds = request.ServiceIds.Distinct().ToList();
            await EnsureRatesExistForBodyType(serviceIds, car.BodyType);

            // Staff check: for each service, at least one qualified employee exists working at this hour
            //var staffOk = await HasQualifiedStaffForAllServices(slotHourStart, serviceIds);
            //if (!staffOk)
            //    throw new BusinessException("No qualified employees available for one or more selected services at this time", 409);

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
            var totalDuration = items.Sum(i => i.DurationMinutes);

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
                Items = items,

                // ✅ creator info stored
                CreatedByType = request.CreatedByType,
                CreatedByEmployeeId = createdByEmployeeId,
                CreatedByClientId = createdByClientId
            };

            await bookingRepo.AddAsync(booking);
            await _uow.SaveChangesAsync();

            return Map(booking);
        }






        //public async Task<BookingResponse?> GetByIdAsync(int bookingId)
        //{
        //    var repo = _uow.Repository<Booking>();
        //    var booking = await repo.GetByIdAsync(bookingId);
        //    if (booking == null) return null;

        //    // GenericRepo ما بيجيبش Items، فهنجيب items query منفصلة
        //    var items = await _uow.Repository<BookingItem>().FindAsync(i => i.BookingId == bookingId);
        //    booking.Items = items.ToList();

        //    return Map(booking);
        //}






        public async Task<BookingResponse?> GetByIdAsync(int bookingId)
        {
            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();
            var serviceRepo = _uow.Repository<Service>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return null;

            // 1) load booking items
            var items = await itemRepo.FindAsync(i => i.BookingId == bookingId);

            // 2) load services names
            var serviceIds = items.Select(i => i.ServiceId).Distinct().ToList();
            var services = await serviceRepo.FindAsync(s => serviceIds.Contains(s.Id));
            var serviceMap = services.ToDictionary(s => s.Id, s => s.Name);

            // 3) map response
            return new BookingResponse
            {
                Id = booking.Id,
                BranchId = booking.BranchId,
                ClientId = booking.ClientId,
                CarId = booking.CarId,
                ScheduledStart = booking.ScheduledStart,
                SlotHourStart = booking.SlotHourStart,
                TotalPrice = booking.TotalPrice,
                EstimatedDurationMinutes = booking.EstimatedDurationMinutes,
                Status = booking.Status,

                Items = items.Select(i =>
                {
                    serviceMap.TryGetValue(i.ServiceId, out var serviceName);

                    return new BookingItemResponse
                    {
                        Id = i.Id,
                        ServiceId = i.ServiceId,
                        ServiceName = serviceName ?? "",   // ✅ هنا الاسم
                        BodyType = i.BodyType,
                        UnitPrice = i.UnitPrice,
                        DurationMinutes = i.DurationMinutes,
                        Status = i.Status,
                        AssignedEmployeeId = i.AssignedEmployeeId
                    };
                }).ToList()
            };
        }




        // before add materials
        //public async Task<BookingItemResponse> StartItemAsync(int itemId, int employeeId)
        //{
        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var bookingRepo = _uow.Repository<Booking>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null)
        //        throw new BusinessException("Booking item not found", 404);

        //    if (item.Status != BookingItemStatus.Pending)
        //        throw new BusinessException("Item cannot be started in its current status", 409);

        //    // Validate employee exists + active
        //    var emp = await _uow.Repository<Employee>().GetByIdAsync(employeeId);
        //    if (emp == null || !emp.IsActive)
        //        throw new BusinessException("Employee not found", 404);

        //    // ✅ أهم شرط عندك: employee must be qualified for this service
        //    var linkRepo = _uow.Repository<EmployeeService>();
        //    var qualified = await linkRepo.AnyAsync(x => x.EmployeeId == employeeId && x.ServiceId == item.ServiceId && x.IsActive);
        //    if (!qualified)
        //        throw new BusinessException("Employee is not qualified for this service", 409);

        //    // Assign + start
        //    item.AssignedEmployeeId = employeeId;
        //    item.Status = BookingItemStatus.InProgress;
        //    item.StartedAt = DateTime.UtcNow;

        //    itemRepo.Update(item);

        //    // Update booking status if needed
        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (booking == null)
        //        throw new BusinessException("Booking not found", 404);

        //    if (booking.Status == BookingStatus.Pending)
        //    {
        //        booking.Status = BookingStatus.InProgress;
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






        //after add materials        
        //    public async Task<BookingItemResponse> StartItemAsync(int itemId, int employeeId)
        //{
        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var bookingRepo = _uow.Repository<Booking>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null)
        //        throw new BusinessException("Booking item not found", 404);

        //    if (item.Status != BookingItemStatus.Pending)
        //        throw new BusinessException("Item cannot be started in its current status", 409);

        //    // employee exists + active (زي ما عندك)
        //    var emp = await _uow.Repository<Employee>().GetByIdAsync(employeeId);
        //    if (emp == null || !emp.IsActive)
        //        throw new BusinessException("Employee not found", 404);

        //    // ✅ employee must be qualified
        //    var linkRepo = _uow.Repository<EmployeeService>();
        //    var qualified = await linkRepo.AnyAsync(x => x.EmployeeId == employeeId && x.ServiceId == item.ServiceId && x.IsActive);
        //    if (!qualified)
        //        throw new BusinessException("Employee is not qualified for this service", 409);

        //    // load booking to know branch
        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (booking == null)
        //        throw new BusinessException("Booking not found", 404);

        //    var branchId = booking.BranchId;

        //    // 1) load recipe for (serviceId, bodyType)
        //    var recipeRepo = _uow.Repository<ServiceMaterialRecipe>();
        //    var recipeRows = await recipeRepo.FindAsync(r =>
        //        r.IsActive && r.ServiceId == item.ServiceId && r.BodyType == item.BodyType);

        //    if (recipeRows.Count == 0)
        //        throw new BusinessException("Missing recipe for this service and car type", 409,
        //            new Dictionary<string, string[]>
        //            {
        //                ["recipe"] = new[] { $"No recipe for serviceId={item.ServiceId} bodyType={item.BodyType}" }
        //            });

        //    // 2) load material definitions (cost/charge/unit)
        //    var materialRepo = _uow.Repository<Material>();
        //    var materialIds = recipeRows.Select(r => r.MaterialId).Distinct().ToList();
        //    var materials = await materialRepo.FindAsync(m => materialIds.Contains(m.Id) && m.IsActive);
        //    var matMap = materials.ToDictionary(m => m.Id, m => m);

        //    var missingMat = materialIds.Where(id => !matMap.ContainsKey(id)).ToList();
        //    if (missingMat.Any())
        //        throw new BusinessException("Some materials in recipe are missing/inactive", 409);

        //    // 3) load branch stocks TRACKING (important)
        //    var stockRepo = _uow.Repository<BranchMaterialStock>();
        //    var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
        //    var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

        //    // if stock row missing => treat as 0 available
        //    foreach (var mid in materialIds)
        //        if (!stockMap.ContainsKey(mid))
        //            stockMap[mid] = null; // marker

        //    // 4) check availability against Available = OnHand - Reserved
        //    var missing = new List<string>();

        //    foreach (var row in recipeRows)
        //    {
        //        var req = row.DefaultQty;
        //        stockMap.TryGetValue(row.MaterialId, out var stock);

        //        var onHand = stock?.OnHandQty ?? 0m;
        //        var reserved = stock?.ReservedQty ?? 0m;
        //        var available = onHand - reserved;
        //        if (available < 0) available = 0;

        //        if (available < req)
        //        {
        //            var mat = matMap[row.MaterialId];
        //            missing.Add($"{mat.Name}: need {req} {mat.Unit}, available {available} {mat.Unit}");
        //        }
        //    }

        //    if (missing.Any())
        //        throw new BusinessException("Not enough stock to start this service", 409,
        //            new Dictionary<string, string[]> { ["materials"] = missing.ToArray() });

        //    // 5) reserve + create usage rows (Default=Reserved=Actual initially)
        //    var usageRepo = _uow.Repository<BookingItemMaterialUsage>();

        //    foreach (var row in recipeRows)
        //    {
        //        var mat = matMap[row.MaterialId];
        //        var qty = row.DefaultQty;

        //        // update branch stock reserved
        //        var stock = stockMap[row.MaterialId];
        //        if (stock == null)
        //        {
        //            // لو عايزة تمنعي إنشاء stock تلقائيًا: ارمي error
        //            // أنا أفضل نرمي error عشان المخزون لازم يتسجل
        //            throw new BusinessException("Stock row missing for material in this branch", 409,
        //                new Dictionary<string, string[]>
        //                {
        //                    ["material"] = new[] { $"MaterialId={row.MaterialId} has no stock row in branch {branchId}" }
        //                });
        //        }

        //        stock.ReservedQty += qty;
        //        stockRepo.Update(stock);

        //        // create usage record
        //        await usageRepo.AddAsync(new BookingItemMaterialUsage
        //        {
        //            BookingItemId = item.Id,
        //            MaterialId = row.MaterialId,
        //            DefaultQty = qty,
        //            ReservedQty = qty,
        //            ActualQty = qty,
        //            UnitCost = mat.CostPerUnit,
        //            UnitCharge = mat.ChargePerUnit,
        //            ExtraCharge = 0,
        //            RecordedByEmployeeId = employeeId,
        //            RecordedAt = DateTime.UtcNow
        //        });
        //    }

        //    // 6) mark item started
        //    item.AssignedEmployeeId = employeeId;
        //    item.Status = BookingItemStatus.InProgress;
        //    item.StartedAt = DateTime.UtcNow;
        //    itemRepo.Update(item);

        //    // booking status -> InProgress if was pending
        //    if (booking.Status == BookingStatus.Pending)
        //    {
        //        booking.Status = BookingStatus.InProgress;
        //        bookingRepo.Update(booking);
        //    }

        //    // ✅ ONE SAVE (with concurrency handling)
        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("Stock was modified by another operation. Please retry.", 409);
        //    }

        //    return MapItem(item);
        //}



        public async Task<BookingItemResponse> StartItemByCashierAsync(int itemId, int cashierId)
        {
            await RequireCashierAsync(cashierId);

            var itemRepo = _uow.Repository<BookingItem>();
            var bookingRepo = _uow.Repository<Booking>();
            var linkRepo = _uow.Repository<EmployeeService>();
            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var recipeRepo = _uow.Repository<ServiceMaterialRecipe>();
            var materialRepo = _uow.Repository<Material>();
            var empRepo = _uow.Repository<Employee>();

            var item = await itemRepo.GetByIdAsync(itemId);
            if (item == null)
                throw new BusinessException("Booking item not found", 404);

            if (item.Status != BookingItemStatus.Pending)
                throw new BusinessException("Item cannot be started in its current status", 409);

            if (!item.AssignedEmployeeId.HasValue)
                throw new BusinessException("Item must be assigned to an employee before starting", 409);

            var employeeId = item.AssignedEmployeeId.Value;

            var emp = await empRepo.GetByIdAsync(employeeId);
            if (emp == null || !emp.IsActive)
                throw new BusinessException("Assigned employee not found", 404);

            // qualification
            var qualified = await linkRepo.AnyAsync(x => x.EmployeeId == employeeId && x.ServiceId == item.ServiceId && x.IsActive);
            if (!qualified)
                throw new BusinessException("Assigned employee is not qualified for this service", 409);

            var booking = await bookingRepo.GetByIdAsync(item.BookingId);
            if (booking == null)
                throw new BusinessException("Booking not found", 404);

            var branchId = booking.BranchId;

            // recipe
            var recipeRows = await recipeRepo.FindAsync(r =>
                r.IsActive && r.ServiceId == item.ServiceId && r.BodyType == item.BodyType);

            if (recipeRows.Count == 0)
                throw new BusinessException("Missing recipe for this service and car type", 409,
                    new Dictionary<string, string[]>
                    {
                        ["recipe"] = new[] { $"No recipe for serviceId={item.ServiceId} bodyType={item.BodyType}" }
                    });

            // materials definitions
            var materialIds = recipeRows.Select(r => r.MaterialId).Distinct().ToList();
            var materials = await materialRepo.FindAsync(m => materialIds.Contains(m.Id) && m.IsActive);
            var matMap = materials.ToDictionary(m => m.Id, m => m);

            var missingMat = materialIds.Where(id => !matMap.ContainsKey(id)).ToList();
            if (missingMat.Any())
                throw new BusinessException("Some materials in recipe are missing/inactive", 409);

            // branch stock tracking
            var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
            var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

            // check availability
            var missing = new List<string>();
            foreach (var row in recipeRows)
            {
                stockMap.TryGetValue(row.MaterialId, out var stock);
                var onHand = stock?.OnHandQty ?? 0m;
                var reserved = stock?.ReservedQty ?? 0m;
                var available = onHand - reserved;
                if (available < 0) available = 0;

                if (available < row.DefaultQty)
                {
                    var mat = matMap[row.MaterialId];
                    missing.Add($"{mat.Name}: need {row.DefaultQty} {mat.Unit}, available {available} {mat.Unit}");
                }
            }

            if (missing.Any())
                throw new BusinessException("Not enough stock to start this service", 409,
                    new Dictionary<string, string[]> { ["materials"] = missing.ToArray() });

            // reserve + create usage rows
            foreach (var row in recipeRows)
            {
                var mat = matMap[row.MaterialId];
                var qty = row.DefaultQty;

                if (!stockMap.TryGetValue(row.MaterialId, out var stock) || stock == null)
                    throw new BusinessException("Stock row missing for material in this branch", 409);

                stock.ReservedQty += qty;
                stockRepo.Update(stock);

                await usageRepo.AddAsync(new BookingItemMaterialUsage
                {
                    BookingItemId = item.Id,
                    MaterialId = row.MaterialId,
                    DefaultQty = qty,
                    ReservedQty = qty,
                    ActualQty = qty,
                    UnitCost = mat.CostPerUnit,
                    UnitCharge = mat.ChargePerUnit,
                    ExtraCharge = 0,
                    RecordedByEmployeeId = cashierId, // ✅ الكاشير اللي سجّل
                    RecordedAt = DateTime.UtcNow
                });
            }

            // mark started
            item.Status = BookingItemStatus.InProgress;
            item.StartedAt = DateTime.UtcNow;
            itemRepo.Update(item);

            if (booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.InProgress;
                bookingRepo.Update(booking);
            }

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException("Stock was modified by another operation. Please retry.", 409);
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










        // before material

        //public async Task<BookingItemResponse> CompleteItemAsync(int itemId, int employeeId)
        //{
        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var bookingRepo = _uow.Repository<Booking>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null)
        //        throw new BusinessException("Booking item not found", 404);

        //    if (item.Status != BookingItemStatus.InProgress)
        //        throw new BusinessException("Item cannot be completed in its current status", 409);

        //    if (item.AssignedEmployeeId != employeeId)
        //        throw new BusinessException("Only the assigned employee can complete this item", 403);

        //    // ✅ mark done
        //    item.Status = BookingItemStatus.Done;
        //    item.CompletedAt = DateTime.UtcNow;
        //    itemRepo.Update(item);

        //    // load booking + all items
        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (booking == null)
        //        throw new BusinessException("Booking not found", 404);

        //    var allItems = await itemRepo.FindAsync(i => i.BookingId == item.BookingId);

        //    // ✅ condition: Done OR Cancelled
        //    var completed = allItems.All(i =>
        //        i.Id == item.Id
        //            ? true
        //            : (i.Status == BookingItemStatus.Done || i.Status == BookingItemStatus.Cancelled)
        //    );

        //    if (completed)
        //    {
        //        booking.Status = BookingStatus.Completed;
        //        booking.CompletedAt = DateTime.UtcNow;
        //        bookingRepo.Update(booking);
        //    }

        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("This service item was already updated by another employee. Please retry.", 409);
        //    }

        //    // ✅ after save: if booking completed -> create invoice (idempotent)
        //    if (completed)
        //    {
        //        await _invoiceService.EnsureInvoiceForBookingAsync(item.BookingId);
        //    }

        //    return MapItem(item);
        //}

















        // after material and before material movements



        //    public async Task<BookingItemResponse> CompleteItemAsync(int itemId, int employeeId)
        //{
        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
        //    var stockRepo = _uow.Repository<BranchMaterialStock>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null) throw new BusinessException("Booking item not found", 404);

        //    if (item.Status != BookingItemStatus.InProgress)
        //        throw new BusinessException("Item cannot be completed in its current status", 409);

        //    if (item.AssignedEmployeeId != employeeId)
        //        throw new BusinessException("Only the assigned employee can complete this item", 403);

        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (booking == null) throw new BusinessException("Booking not found", 404);

        //    var branchId = booking.BranchId;

        //    // ✅ load usages tracking
        //    var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == item.Id);
        //    if (usages.Count == 0)
        //        throw new BusinessException("No reserved materials found for this item (start it first)", 409);

        //    var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();

        //    // ✅ load stocks tracking
        //    var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
        //    var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

        //    // 1) final check + consume + release reserved
        //    foreach (var u in usages)
        //    {
        //        if (!stockMap.TryGetValue(u.MaterialId, out var stock))
        //            throw new BusinessException("Stock row missing for a material in this branch", 409);

        //        // available if we release this item's reserved first:
        //        // availableNow = onHand - reserved + u.ReservedQty
        //        var availableNow = stock.OnHandQty - stock.ReservedQty + u.ReservedQty;
        //        if (availableNow < 0) availableNow = 0;

        //        if (availableNow < u.ActualQty)
        //        {
        //            throw new BusinessException("Not enough stock to complete this service", 409,
        //                new Dictionary<string, string[]>
        //                {
        //                    ["materials"] = new[]
        //                    {
        //                    $"MaterialId={u.MaterialId}: need {u.ActualQty}, available {availableNow}"
        //                    }
        //                });
        //        }

        //        // release reservation
        //        stock.ReservedQty -= u.ReservedQty;
        //        if (stock.ReservedQty < 0) stock.ReservedQty = 0;

        //        // consume actual
        //        stock.OnHandQty -= u.ActualQty;
        //        if (stock.OnHandQty < 0) stock.OnHandQty = 0; // safety, but check above should prevent

        //        stockRepo.Update(stock);

        //        // optional: mark reservation released at usage row level
        //        u.ReservedQty = 0;
        //        usageRepo.Update(u);
        //    }

        //    // 2) compute total adjustment for this item
        //    var adjustment = usages.Sum(u => u.ExtraCharge); // now can be +/-
        //    item.MaterialAdjustment = adjustment;

        //    // 3) set done
        //    item.Status = BookingItemStatus.Done;
        //    item.CompletedAt = DateTime.UtcNow;
        //    itemRepo.Update(item);

        //    // 4) update booking totals (optional now, لكن مفيد)
        //    // Total = sum(UnitPrice + MaterialAdjustment) for not-cancelled items
        //    // هنسيبها دلوقتي لمرحلة invoice recalculation أو تعملها هنا لو تحبي

        //    // 5) auto close booking (Done/Cancelled => Completed, all Cancelled => Cancelled)
        //    await _closingService.TryAutoCloseBookingAsync(item.BookingId, save: false);

        //    // ✅ ONE SAVE
        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("This record was modified by another operation. Please retry.", 409);
        //    }

        //    // بعد الحفظ: لو booking Completed -> ensure invoice
        //    var freshBooking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (freshBooking != null && freshBooking.Status == BookingStatus.Completed)
        //    {
        //        await _invoiceService.EnsureInvoiceForBookingAsync(item.BookingId);
        //    }

        //    return MapItem(item);
        //}

        public async Task<BookingResponse> QuickCreateAsync(QuickCreateBookingRequest request, bool quickCHeckout)
        {
            // 0) normalize
            var phone = request.Client.PhoneNumber.Trim();
            var plate = request.Car.PlateNumber.Trim().ToUpperInvariant();

            // 1) Get or create client by phone
            var clientRepo = _uow.Repository<Client>();
            var clients = await clientRepo.FindAsync(c => c.PhoneNumber == phone);
            var client = clients.FirstOrDefault();

            if (client == null)
            {
                client = new Client
                {
                    PhoneNumber = phone,
                    FullName = (request.Client.FullName ?? "New Client").Trim(),
                    Email = request.Client.Email?.Trim(),
                    IsActive = true
                };
                await clientRepo.AddAsync(client);
                await _uow.SaveChangesAsync();
            }

            // 2) Get or create car
            var carRepo = _uow.Repository<Car>();
            var cars = await carRepo.FindAsync(c => c.ClientId == client.Id && c.PlateNumber == plate);
            var car = cars.FirstOrDefault();

            if (car == null)
            {
                car = new Car
                {
                    ClientId = client.Id,
                    PlateNumber = plate,
                    BodyType = request.Car.BodyType,
                    Brand = request.Car.Brand?.Trim(),
                    Model = request.Car.Model?.Trim(),
                    Color = request.Car.Color?.Trim(),
                    Year = request.Car.Year,
                    IsDefault = request.Car.IsDefault
                };
                await carRepo.AddAsync(car);

                if (request.Car.IsDefault)
                {
                    var otherCars = await carRepo.FindAsync(c => c.ClientId == client.Id);
                    foreach (var oc in otherCars.Where(x => x.Id != car.Id && x.IsDefault))
                    {
                        oc.IsDefault = false;
                        carRepo.Update(oc);
                    }
                }

                await _uow.SaveChangesAsync();
            }

            // 3) Create booking
            var create = new CreateBookingRequest
            {
                BranchId = request.BranchId,
                ClientId = client.Id,
                CarId = car.Id,
                ScheduledStart = request.ScheduledStart,
                ServiceIds = request.ServiceIds,
                Notes = request.Notes,

                CreatedByType = request.CreatedByType,
                CreatedByEmployeeId = request.CreatedByEmployeeId,
                CreatedByClientId = request.CreatedByType == BookingCreatedByType.Client ? client.Id : request.CreatedByClientId
            };
            var bookingResponse = new BookingResponse();
            if (!quickCHeckout)
            {
                bookingResponse = await CreateAsync(create);
            }
            else
            {
                bookingResponse = await CheckoutCreateAsync(create);
            }

            // 4) assign مباشر لو موجود
            // ✅ assign per service (optional)
            if (request.ServiceAssignments != null && request.ServiceAssignments.Count > 0)
            {
                var bookingItemRepo = _uow.Repository<BookingItem>();
                var employeeServiceRepo = _uow.Repository<EmployeeService>();

                // serviceId -> employeeId
                var map = request.ServiceAssignments
                    .GroupBy(x => x.ServiceId)
                    .ToDictionary(g => g.Key, g => g.First().EmployeeId);

                // هات كل booking items tracking
                var items = await bookingItemRepo.FindTrackingAsync(i => i.BookingId == bookingResponse.Id);

                foreach (var item in items)
                {
                    // لو مش معمول assignment للخدمة دي، سيبيها null
                    if (!map.TryGetValue(item.ServiceId, out var employeeId))
                        continue;

                    // ✅ شغال في الوقت ده؟
                    var isWorking = await _scheduleService.IsEmployeeWorkingAsync(employeeId, request.ScheduledStart);
                    if (!isWorking)
                        throw new BusinessException($"Employee {employeeId} is not working at this time", 409);

                    // ✅ مؤهل للخدمة؟
                    var qualified = await employeeServiceRepo.AnyAsync(es =>
                        es.EmployeeId == employeeId &&
                        es.ServiceId == item.ServiceId &&
                        es.IsActive);

                    if (!qualified)
                        throw new BusinessException($"Employee {employeeId} is not qualified for service {item.ServiceId}", 409);

                    // assign
                    item.AssignedEmployeeId = employeeId;
                    bookingItemRepo.Update(item);

                    // ✅ عدّل response كمان (عشان يرجع فورًا)
                    var respItem = bookingResponse.Items.FirstOrDefault(x => x.Id == item.Id);
                    if (respItem != null)
                        respItem.AssignedEmployeeId = employeeId;
                }

                await _uow.SaveChangesAsync();
            }

            return bookingResponse;

        }



         



















        //public async Task<BookingResponse> CheckOutQuickCreateAsync(QuickCreateBookingRequest request)
        //{
        //    // 0) normalize
        //    var phone = request.Client.PhoneNumber.Trim();
        //    var plate = request.Car.PlateNumber.Trim().ToUpperInvariant();

        //    // 1) Get or create client by phone
        //    var clientRepo = _uow.Repository<Client>();
        //    var clients = await clientRepo.FindAsync(c => c.PhoneNumber == phone);
        //    var client = clients.FirstOrDefault();

        //    if (client == null)
        //    {
        //        client = new Client
        //        {
        //            PhoneNumber = phone,
        //            FullName = (request.Client.FullName ?? "New Client").Trim(),
        //            Email = request.Client.Email?.Trim(),
        //            IsActive = true
        //        };
        //        await clientRepo.AddAsync(client);
        //        await _uow.SaveChangesAsync();
        //    }

        //    // 2) Get or create car
        //    var carRepo = _uow.Repository<Car>();
        //    var cars = await carRepo.FindAsync(c => c.ClientId == client.Id && c.PlateNumber == plate);
        //    var car = cars.FirstOrDefault();

        //    if (car == null)
        //    {
        //        car = new Car
        //        {
        //            ClientId = client.Id,
        //            PlateNumber = plate,
        //            BodyType = request.Car.BodyType,
        //            Brand = request.Car.Brand?.Trim(),
        //            Model = request.Car.Model?.Trim(),
        //            Color = request.Car.Color?.Trim(),
        //            Year = request.Car.Year,
        //            IsDefault = request.Car.IsDefault
        //        };
        //        await carRepo.AddAsync(car);

        //        if (request.Car.IsDefault)
        //        {
        //            var otherCars = await carRepo.FindAsync(c => c.ClientId == client.Id);
        //            foreach (var oc in otherCars.Where(x => x.Id != car.Id && x.IsDefault))
        //            {
        //                oc.IsDefault = false;
        //                carRepo.Update(oc);
        //            }
        //        }

        //        await _uow.SaveChangesAsync();
        //    }

        //    // 3) Create booking
        //    var create = new CreateBookingRequest
        //    {
        //        BranchId = request.BranchId,
        //        ClientId = client.Id,
        //        CarId = car.Id,
        //        ScheduledStart = request.ScheduledStart,
        //        ServiceIds = request.ServiceIds,
        //        Notes = request.Notes,

        //        CreatedByType = request.CreatedByType,
        //        CreatedByEmployeeId = request.CreatedByEmployeeId,
        //        CreatedByClientId = request.CreatedByType == BookingCreatedByType.Client ? client.Id : request.CreatedByClientId
        //    };


        //    var bookingResponse = await CreateAsync(create);

        //    // 4) assign مباشر لو موجود
        //    // ✅ assign per service (optional)
        //    if (request.ServiceAssignments != null && request.ServiceAssignments.Count > 0)
        //    {
        //        var bookingItemRepo = _uow.Repository<BookingItem>();
        //        var employeeServiceRepo = _uow.Repository<EmployeeService>();

        //        // serviceId -> employeeId
        //        var map = request.ServiceAssignments
        //            .GroupBy(x => x.ServiceId)
        //            .ToDictionary(g => g.Key, g => g.First().EmployeeId);

        //        // هات كل booking items tracking
        //        var items = await bookingItemRepo.FindTrackingAsync(i => i.BookingId == bookingResponse.Id);

        //        foreach (var item in items)
        //        {
        //            // لو مش معمول assignment للخدمة دي، سيبيها null
        //            if (!map.TryGetValue(item.ServiceId, out var employeeId))
        //                continue;

        //            // ✅ شغال في الوقت ده؟
        //            var isWorking = await _scheduleService.IsEmployeeWorkingAsync(employeeId, request.ScheduledStart);
        //            if (!isWorking)
        //                throw new BusinessException($"Employee {employeeId} is not working at this time", 409);

        //            // ✅ مؤهل للخدمة؟
        //            var qualified = await employeeServiceRepo.AnyAsync(es =>
        //                es.EmployeeId == employeeId &&
        //                es.ServiceId == item.ServiceId &&
        //                es.IsActive);

        //            if (!qualified)
        //                throw new BusinessException($"Employee {employeeId} is not qualified for service {item.ServiceId}", 409);

        //            // assign
        //            item.AssignedEmployeeId = employeeId;
        //            bookingItemRepo.Update(item);

        //            // ✅ عدّل response كمان (عشان يرجع فورًا)
        //            var respItem = bookingResponse.Items.FirstOrDefault(x => x.Id == item.Id);
        //            if (respItem != null)
        //                respItem.AssignedEmployeeId = employeeId;
        //        }

        //        await _uow.SaveChangesAsync();
        //    }

        //    return bookingResponse;

        //}



















        //public async Task<BookingItemResponse> CompleteItemAsync (int itemId, int employeeId)
        //{
        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
        //    var stockRepo = _uow.Repository<BranchMaterialStock>();
        //    var movementRepo = _uow.Repository<MaterialMovement>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null) throw new BusinessException("Booking item not found", 404);

        //    if (item.Status != BookingItemStatus.InProgress)
        //        throw new BusinessException("Item cannot be completed in its current status", 409);

        //    if (item.AssignedEmployeeId != employeeId)
        //        throw new BusinessException("Only the assigned employee can complete this item", 403);

        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (booking == null) throw new BusinessException("Booking not found", 404);

        //    var branchId = booking.BranchId;

        //    // ✅ load usages tracking
        //    var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == item.Id);
        //    if (usages.Count == 0)
        //        throw new BusinessException("No reserved materials found for this item (start it first)", 409);

        //    var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();

        //    // ✅ load stocks tracking
        //    var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
        //    var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

        //    // ✅ Idempotency: لو movements اتسجلت قبل كده لنفس item/material -> متضيفهاش تاني
        //    // (يحميك من retry / double request)
        //    var existingConsume = await movementRepo.FindAsync(m =>
        //        m.BookingItemId == item.Id &&
        //        m.MovementType == MaterialMovementType.Consume);

        //    var alreadyLogged = existingConsume
        //        .Select(m => m.MaterialId)
        //        .ToHashSet();

        //    var occurredAt = DateTime.UtcNow;

        //    // 1) final check + consume + release reserved + record movements
        //    foreach (var u in usages)
        //    {
        //        if (!stockMap.TryGetValue(u.MaterialId, out var stock))
        //            throw new BusinessException("Stock row missing for a material in this branch", 409);

        //        // available if we release this item's reserved first:
        //        var availableNow = stock.OnHandQty - stock.ReservedQty + u.ReservedQty;
        //        if (availableNow < 0) availableNow = 0;

        //        if (availableNow < u.ActualQty)
        //        {
        //            throw new BusinessException("Not enough stock to complete this service", 409,
        //                new Dictionary<string, string[]>
        //                {
        //                    ["materials"] = new[]
        //                    {
        //                $"MaterialId={u.MaterialId}: need {u.ActualQty}, available {availableNow}"
        //                    }
        //                });
        //        }

        //        // ✅ release reservation in branch stock
        //        stock.ReservedQty -= u.ReservedQty;
        //        if (stock.ReservedQty < 0) stock.ReservedQty = 0;

        //        // ✅ consume actual from onhand
        //        stock.OnHandQty -= u.ActualQty;
        //        if (stock.OnHandQty < 0) stock.OnHandQty = 0;

        //        stockRepo.Update(stock);

        //        // optional: mark reservation released at usage row level
        //        u.ReservedQty = 0;
        //        usageRepo.Update(u);

        //        // ✅ record movement (CONSUME) once
        //        if (!alreadyLogged.Contains(u.MaterialId) && u.ActualQty > 0)
        //        {
        //            await movementRepo.AddAsync(new MaterialMovement
        //            {
        //                BranchId = branchId,
        //                MaterialId = u.MaterialId,
        //                MovementType = MaterialMovementType.Consume,
        //                Qty = u.ActualQty,
        //                UnitCostSnapshot = u.UnitCost,
        //                TotalCost = u.ActualQty * u.UnitCost,
        //                OccurredAt = occurredAt,
        //                BookingId = booking.Id,
        //                BookingItemId = item.Id,
        //                RecordedByEmployeeId = employeeId,
        //                Notes = "Consume on complete"
        //            });
        //        }
        //    }

        //    // 2) compute total adjustment for this item (can be +/-)
        //    var adjustment = usages.Sum(u => u.ExtraCharge);
        //    item.MaterialAdjustment = adjustment;

        //    // 3) set done
        //    item.Status = BookingItemStatus.Done;
        //    item.CompletedAt = occurredAt;
        //    itemRepo.Update(item);

        //    // 4) auto close booking (Completed/Cancelled)
        //    await _closingService.TryAutoCloseBookingAsync(item.BookingId, save: false);

        //    // ✅ ONE SAVE
        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("This record was modified by another operation. Please retry.", 409);
        //    }

        //    // بعد الحفظ: لو booking Completed -> ensure invoice
        //    var freshBooking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (freshBooking != null && freshBooking.Status == BookingStatus.Completed)
        //    {
        //        await _invoiceService.EnsureInvoiceForBookingAsync(item.BookingId);
        //    }

        //    return MapItem(item);
        //}
















        //public async Task<BookingItemResponse> CompleteItemAsync(int itemId, int employeeId)
        //{
        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
        //    var stockRepo = _uow.Repository<BranchMaterialStock>();
        //    var movementRepo = _uow.Repository<MaterialMovement>();
        //    var reqRepo = _uow.Repository<BookingItemMaterialChangeRequest>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null) throw new BusinessException("Booking item not found", 404);

        //    if (item.Status != BookingItemStatus.InProgress)
        //        throw new BusinessException("Item cannot be completed in its current status", 409);

        //    if (item.AssignedEmployeeId != employeeId)
        //        throw new BusinessException("Only the assigned employee can complete this item", 403);

        //    // ✅ block complete if there is a pending material change request
        //    var hasPending = await reqRepo.AnyAsync(r =>
        //        r.BookingItemId == itemId &&
        //        r.Status == MaterialChangeRequestStatus.Pending);

        //    if (hasPending)
        //        throw new BusinessException(
        //            "Cannot complete this service while there is a pending material request. Please wait for cashier approval.",
        //            409
        //        );

        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (booking == null) throw new BusinessException("Booking not found", 404);

        //    var branchId = booking.BranchId;

        //    // ✅ load usages tracking
        //    var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == item.Id);
        //    if (usages.Count == 0)
        //        throw new BusinessException("No reserved materials found for this item (start it first)", 409);

        //    var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();

        //    // ✅ load stocks tracking
        //    var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
        //    var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

        //    // ✅ Idempotency: prevent duplicate consume movements
        //    var existingConsume = await movementRepo.FindAsync(m =>
        //        m.BookingItemId == item.Id &&
        //        m.MovementType == MaterialMovementType.Consume);

        //    var alreadyLogged = existingConsume.Select(m => m.MaterialId).ToHashSet();

        //    var occurredAt = DateTime.UtcNow;

        //    // 1) final check + consume + release reserved + record movements
        //    foreach (var u in usages)
        //    {
        //        if (!stockMap.TryGetValue(u.MaterialId, out var stock))
        //            throw new BusinessException("Stock row missing for a material in this branch", 409);

        //        // available if we release this item's reserved first:
        //        var availableNow = stock.OnHandQty - stock.ReservedQty + u.ReservedQty;
        //        if (availableNow < 0) availableNow = 0;

        //        if (availableNow < u.ActualQty)
        //        {
        //            throw new BusinessException("Not enough stock to complete this service", 409,
        //                new Dictionary<string, string[]>
        //                {
        //                    ["materials"] = new[]
        //                    {
        //                $"MaterialId={u.MaterialId}: need {u.ActualQty}, available {availableNow}"
        //                    }
        //                });
        //        }

        //        // release reservation
        //        stock.ReservedQty -= u.ReservedQty;
        //        if (stock.ReservedQty < 0) stock.ReservedQty = 0;

        //        // consume actual
        //        stock.OnHandQty -= u.ActualQty;
        //        if (stock.OnHandQty < 0) stock.OnHandQty = 0;

        //        stockRepo.Update(stock);

        //        // mark reservation released
        //        u.ReservedQty = 0;
        //        usageRepo.Update(u);

        //        // record consume movement once
        //        if (!alreadyLogged.Contains(u.MaterialId) && u.ActualQty > 0)
        //        {
        //            await movementRepo.AddAsync(new MaterialMovement
        //            {
        //                BranchId = branchId,
        //                MaterialId = u.MaterialId,
        //                MovementType = MaterialMovementType.Consume,
        //                Qty = u.ActualQty,
        //                UnitCostSnapshot = u.UnitCost,
        //                TotalCost = u.ActualQty * u.UnitCost,
        //                OccurredAt = occurredAt,
        //                BookingId = booking.Id,
        //                BookingItemId = item.Id,
        //                RecordedByEmployeeId = employeeId,
        //                Notes = "Consume on complete"
        //            });
        //        }
        //    }

        //    // 2) compute total adjustment for this item (can be +/-)
        //    item.MaterialAdjustment = usages.Sum(u => u.ExtraCharge);

        //    // 3) set done
        //    item.Status = BookingItemStatus.Done;
        //    item.CompletedAt = occurredAt;
        //    itemRepo.Update(item);

        //    // 4) auto close booking
        //    await _closingService.TryAutoCloseBookingAsync(item.BookingId, save: false);

        //    // ✅ ONE SAVE
        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("This record was modified by another operation. Please retry.", 409);
        //    }

        //    // after save: if booking completed -> ensure invoice
        //    var freshBooking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (freshBooking != null && freshBooking.Status == BookingStatus.Completed)
        //    {
        //        await _invoiceService.EnsureInvoiceForBookingAsync(item.BookingId);
        //    }

        //    return MapItem(item);
        //}















        //public async Task<BookingItemResponse> CompleteItemByCashierAsync(int itemId, int cashierId)
        //{
        //    await RequireCashierAsync(cashierId);

        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
        //    var stockRepo = _uow.Repository<BranchMaterialStock>();
        //    var movementRepo = _uow.Repository<MaterialMovement>();

        //    var item = await itemRepo.GetByIdAsync(itemId);
        //    if (item == null) throw new BusinessException("Booking item not found", 404);

        //    if (item.Status != BookingItemStatus.InProgress)
        //        throw new BusinessException("Item cannot be completed in its current status", 409);

        //    if (!item.AssignedEmployeeId.HasValue)
        //        throw new BusinessException("Item must be assigned to an employee", 409);

        //    var employeeId = item.AssignedEmployeeId.Value;

        //    var booking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (booking == null) throw new BusinessException("Booking not found", 404);

        //    var branchId = booking.BranchId;

        //    var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == item.Id);
        //    if (usages.Count == 0)
        //        throw new BusinessException("No reserved materials found for this item (start it first)", 409);

        //    var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();
        //    var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
        //    var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

        //    var existingConsume = await movementRepo.FindAsync(m =>
        //        m.BookingItemId == item.Id &&
        //        m.MovementType == MaterialMovementType.Consume);

        //    var alreadyLogged = existingConsume.Select(m => m.MaterialId).ToHashSet();

        //    var occurredAt = DateTime.UtcNow;

        //    foreach (var u in usages)
        //    {
        //        if (!stockMap.TryGetValue(u.MaterialId, out var stock))
        //            throw new BusinessException("Stock row missing for a material in this branch", 409);

        //        var availableNow = stock.OnHandQty - stock.ReservedQty + u.ReservedQty;
        //        if (availableNow < 0) availableNow = 0;

        //        if (availableNow < u.ActualQty)
        //            throw new BusinessException("Not enough stock to complete this service", 409);

        //        stock.ReservedQty -= u.ReservedQty;
        //        if (stock.ReservedQty < 0) stock.ReservedQty = 0;

        //        stock.OnHandQty -= u.ActualQty;
        //        if (stock.OnHandQty < 0) stock.OnHandQty = 0;

        //        stockRepo.Update(stock);

        //        u.ReservedQty = 0;
        //        usageRepo.Update(u);

        //        if (!alreadyLogged.Contains(u.MaterialId) && u.ActualQty > 0)
        //        {
        //            await movementRepo.AddAsync(new MaterialMovement
        //            {
        //                BranchId = branchId,
        //                MaterialId = u.MaterialId,
        //                MovementType = MaterialMovementType.Consume,
        //                Qty = u.ActualQty,
        //                UnitCostSnapshot = u.UnitCost,
        //                TotalCost = u.ActualQty * u.UnitCost,
        //                OccurredAt = occurredAt,
        //                BookingId = booking.Id,
        //                BookingItemId = item.Id,
        //                RecordedByEmployeeId = employeeId, // ✅ العامل اللي نفّذ (للتقارير)
        //                Notes = $"Completed by cashier {cashierId}"
        //            });
        //        }
        //    }

        //    item.MaterialAdjustment = usages.Sum(u => u.ExtraCharge);
        //    item.Status = BookingItemStatus.Done;
        //    item.CompletedAt = occurredAt;
        //    itemRepo.Update(item);

        //    await _closingService.TryAutoCloseBookingAsync(item.BookingId, save: false);

        //    try
        //    {
        //        await _uow.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        throw new BusinessException("This record was modified by another operation. Please retry.", 409);
        //    }

        //    var freshBooking = await bookingRepo.GetByIdAsync(item.BookingId);
        //    if (freshBooking != null && freshBooking.Status == BookingStatus.Completed)
        //    {
        //        await _invoiceService.EnsureInvoiceForBookingAsync(item.BookingId);
        //    }

        //    return MapItem(item);
        //}


        public async Task<BookingItemResponse> CompleteItemByCashierAsync(int itemId, int cashierId)
        {
            await RequireCashierAsync(cashierId);

            var itemRepo = _uow.Repository<BookingItem>();
            var bookingRepo = _uow.Repository<Booking>();
            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var movementRepo = _uow.Repository<MaterialMovement>();

            var item = await itemRepo.GetByIdAsync(itemId);
            if (item == null) throw new BusinessException("Booking item not found", 404);

            if (item.Status != BookingItemStatus.InProgress)
                throw new BusinessException("Item cannot be completed in its current status", 409);

            if (!item.AssignedEmployeeId.HasValue)
                throw new BusinessException("Item must be assigned to an employee", 409);

            var employeeId = item.AssignedEmployeeId.Value;

            var booking = await bookingRepo.GetByIdAsync(item.BookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            var branchId = booking.BranchId;

            var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == item.Id);
            if (usages.Count == 0)
                throw new BusinessException("No reserved materials found for this item (start it first)", 409);

            var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();
            var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
            var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

            var existingConsume = await movementRepo.FindAsync(m =>
                m.BookingItemId == item.Id &&
                m.MovementType == MaterialMovementType.Consume);

            var alreadyLogged = existingConsume.Select(m => m.MaterialId).ToHashSet();

            var occurredAt = DateTime.UtcNow;

            foreach (var u in usages)
            {
                if (!stockMap.TryGetValue(u.MaterialId, out var stock))
                    throw new BusinessException("Stock row missing for a material in this branch", 409);

                var availableNow = stock.OnHandQty - stock.ReservedQty + u.ReservedQty;
                if (availableNow < 0) availableNow = 0;

                if (availableNow < u.ActualQty)
                    throw new BusinessException("Not enough stock to complete this service", 409);

                stock.ReservedQty -= u.ReservedQty;
                if (stock.ReservedQty < 0) stock.ReservedQty = 0;

                stock.OnHandQty -= u.ActualQty;
                if (stock.OnHandQty < 0) stock.OnHandQty = 0;

                stockRepo.Update(stock);

                u.ReservedQty = 0;
                usageRepo.Update(u);

                if (!alreadyLogged.Contains(u.MaterialId) && u.ActualQty > 0)
                {
                    await movementRepo.AddAsync(new MaterialMovement
                    {
                        BranchId = branchId,
                        MaterialId = u.MaterialId,
                        MovementType = MaterialMovementType.Consume,
                        Qty = u.ActualQty,
                        UnitCostSnapshot = u.UnitCost,
                        TotalCost = u.ActualQty * u.UnitCost,
                        OccurredAt = occurredAt,
                        BookingId = booking.Id,
                        BookingItemId = item.Id,
                        Notes = $"Completed by cashier {cashierId}"
                    });
                }
            }

            item.MaterialAdjustment = usages.Sum(u => u.ExtraCharge);

            // سعر الخدمة للفاتورة
            var serviceCharge = item.UnitPrice + item.MaterialAdjustment;
            if (serviceCharge < 0) serviceCharge = 0;
            await movementRepo.AddAsync(new MaterialMovement
            {
                BranchId = branchId,
                MaterialId = null,
                MovementType = MaterialMovementType.ServiceCharge,
                Qty = 1,
                UnitCostSnapshot = 0,
                TotalCost = 0,
                UnitCharge = serviceCharge,
                TotalCharge = serviceCharge,
                OccurredAt = occurredAt,
                BookingId = booking.Id,
                BookingItemId = item.Id,
                Notes = "Service completed - charge"
            });
            item.Status = BookingItemStatus.Done;
            item.CompletedAt = occurredAt;
            itemRepo.Update(item);

            await _closingService.TryAutoCloseBookingAsync(item.BookingId, save: false);

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException("This record was modified by another operation. Please retry.", 409);
            }

            var freshBooking = await bookingRepo.GetByIdAsync(item.BookingId);
            if (freshBooking != null && freshBooking.Status == BookingStatus.Completed)
            {
                await _invoiceService.EnsureInvoiceForBookingAsync(item.BookingId);
            }

            return MapItem(item);
        }



        public async Task<BookingItemMaterialsResponse> UpdateActualByCashierAsync(int bookingItemId, UpdateBookingItemMaterialsByCashierRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var itemRepo = _uow.Repository<BookingItem>();
            var bookingRepo = _uow.Repository<Booking>();

            var item = await itemRepo.GetByIdAsync(bookingItemId);
            if (item == null) throw new BusinessException("Booking item not found", 404);

            if (item.Status != BookingItemStatus.InProgress)
                throw new BusinessException("Materials can be updated only while service is InProgress", 409);

            var booking = await bookingRepo.GetByIdAsync(item.BookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            var branchId = booking.BranchId;

            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
            var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == bookingItemId);
            if (usages.Count == 0)
                throw new BusinessException("No materials were reserved for this item (start the service first)", 409);

            if (request.Materials == null || request.Materials.Count == 0)
                throw new BusinessException("Materials is required", 400);

            // validate material ids
            var usageMaterialIds = usages.Select(u => u.MaterialId).ToHashSet();
            var invalid = request.Materials.Where(m => !usageMaterialIds.Contains(m.MaterialId)).Select(m => m.MaterialId).ToList();
            if (invalid.Any())
                throw new BusinessException("Some materials are not part of this service recipe", 400);

            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();
            var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
            var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

            var materialRepo = _uow.Repository<Material>();
            var mats = await materialRepo.FindAsync(m => materialIds.Contains(m.Id));
            var matMap = mats.ToDictionary(m => m.Id, m => m);

            foreach (var req in request.Materials)
            {
                var usage = usages.First(u => u.MaterialId == req.MaterialId);

                var newActual = req.ActualQty;
                if (newActual < 0) newActual = 0;

                var currentReserved = usage.ReservedQty;
                var delta = newActual - currentReserved;

                if (!stockMap.TryGetValue(req.MaterialId, out var stock))
                    throw new BusinessException("Stock row missing for a material in this branch", 409);

                if (delta > 0)
                {
                    var available = stock.OnHandQty - stock.ReservedQty;
                    if (available < 0) available = 0;

                    if (available < delta)
                    {
                        var matName = matMap.TryGetValue(req.MaterialId, out var mm) ? mm.Name : $"MaterialId={req.MaterialId}";
                        throw new BusinessException("Not enough stock for this increase", 409,
                            new Dictionary<string, string[]> { ["materials"] = new[] { $"{matName}: need extra {delta}, available {available}" } });
                    }

                    stock.ReservedQty += delta;
                    usage.ReservedQty += delta;
                    stockRepo.Update(stock);
                }
                else if (delta < 0)
                {
                    var release = -delta;

                    stock.ReservedQty -= release;
                    if (stock.ReservedQty < 0) stock.ReservedQty = 0;

                    usage.ReservedQty -= release;
                    if (usage.ReservedQty < 0) usage.ReservedQty = 0;

                    stockRepo.Update(stock);
                }

                usage.ActualQty = newActual;

                var diffQty = usage.ActualQty - usage.DefaultQty; // ممكن بالسالب
                usage.ExtraCharge = diffQty * usage.UnitCharge;

                usage.RecordedByEmployeeId = request.CashierId; // ✅ الكاشير اللي سجّل
                usage.RecordedAt = DateTime.UtcNow;
                usageRepo.Update(usage);
            }

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException("Stock was modified by another operation. Please retry.", 409);
            }

            return MapMaterials(bookingItemId, usages, matMap);
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





        private static BookingItemMaterialsResponse MapMaterials(
            int bookingItemId,
            IReadOnlyList<BookingItemMaterialUsage> usages,
            IReadOnlyDictionary<int, Material> matMap)
        {
            return new BookingItemMaterialsResponse
            {
                BookingItemId = bookingItemId,
                Materials = usages.Select(u =>
                {
                    matMap.TryGetValue(u.MaterialId, out var mat);

                    return new BookingItemMaterialDto
                    {
                        MaterialId = u.MaterialId,
                        MaterialName = mat?.Name ?? "",
                        Unit = mat?.Unit.ToString() ?? "",
                        DefaultQty = u.DefaultQty,
                        ActualQty = u.ActualQty,
                        ReservedQty = u.ReservedQty,
                        UnitCharge = u.UnitCharge,
                        ExtraCharge = u.ExtraCharge
                    };
                }).ToList()
            };
        }






        public async Task<TodayBookingsResponse> GetTodayAsync(int branchId, DateOnly date)
        {
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            var dayStart = date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var bookingRepo = _uow.Repository<Booking>();
            var bookings = await bookingRepo.FindAsync(b =>
                b.BranchId == branchId &&
                b.ScheduledStart >= dayStart &&
                b.ScheduledStart < dayEnd);

            if (bookings.Count == 0)
                return new TodayBookingsResponse { BranchId = branchId, Date = date };

            var bookingIds = bookings.Select(b => b.Id).ToList();

            // invoices by booking (InvoiceId + IsPaid)
            var invRepo = _uow.Repository<Invoice>();
            var invoices = await invRepo.FindAsync(inv => inv.BookingId != null && bookingIds.Contains(inv.BookingId.Value));
            var invoiceByBooking = invoices
                .Where(inv => inv.BookingId.HasValue)
                .ToDictionary(inv => inv.BookingId!.Value, inv => new { inv.Id, IsPaid = inv.Status == InvoiceStatus.Paid });

            // clients + cars
            var clientIds = bookings.Select(b => b.ClientId).Distinct().ToList();
            var carIds = bookings.Select(b => b.CarId).Distinct().ToList();

            var clients = await _uow.Repository<Client>().FindAsync(c => clientIds.Contains(c.Id));
            var cars = await _uow.Repository<Car>().FindAsync(c => carIds.Contains(c.Id));

            var clientMap = clients.ToDictionary(c => c.Id, c => c);
            var carMap = cars.ToDictionary(c => c.Id, c => c);

            // items count
            var items = await _uow.Repository<BookingItem>().FindAsync(i => bookingIds.Contains(i.BookingId) &&
    i.Status != BookingItemStatus.Cancelled);
            var countMap = items.GroupBy(i => i.BookingId).ToDictionary(g => g.Key, g => g.Count());

            // items by booking
            var itemsByBooking = items
                .GroupBy(i => i.BookingId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // services names
            var serviceIds = items.Select(i => i.ServiceId).Distinct().ToList();
            var services = await _uow.Repository<Service>().FindAsync(s => serviceIds.Contains(s.Id) );
            var svcMap = services.ToDictionary(s => s.Id, s => s.Name);


            DTOs.Bookings.BookingListItemDto Map(Booking b)
            {
                clientMap.TryGetValue(b.ClientId, out var cl);
                carMap.TryGetValue(b.CarId, out var car);

                itemsByBooking.TryGetValue(b.Id, out var bookingItems);
                bookingItems ??= new List<BookingItem>();

                var servicesLines = bookingItems.Select(i =>
                {
                    svcMap.TryGetValue(i.ServiceId, out var svcName);

                    return new BookingServiceLineDto
                    {
                        BookingItemId = i.Id,
                        ServiceId = i.ServiceId,
                        ServiceName = svcName ?? "",
                        UnitPrice = i.UnitPrice,
                        DurationMinutes = i.DurationMinutes,
                        Status = i.Status,
                        AssignedEmployeeId = i.AssignedEmployeeId
                    };
                }).ToList();

                invoiceByBooking.TryGetValue(b.Id, out var invInfo);

                return new DTOs.Bookings.BookingListItemDto
                {
                    BookingId = b.Id,
                    ScheduledStart = b.ScheduledStart,
                    Status = b.Status,
                    ClientId = b.ClientId,
                    ClientName = cl?.FullName ?? "",
                    PhoneNumber = cl?.PhoneNumber ?? "",
                    CarId = b.CarId,
                    PlateNumber = car?.PlateNumber ?? "",
                    CarModel = car?.Model ?? "",
                    TotalPrice = b.TotalPrice,
                    ServicesCount = servicesLines.Count,
                    Services = servicesLines,
                    InvoiceId = invInfo?.Id,
                    IsInvoicePaid = invInfo?.IsPaid ?? false
                };
            }

            var resp = new TodayBookingsResponse { BranchId = branchId, Date = date };

            foreach (var b in bookings.OrderBy(x => x.ScheduledStart))
            {
                var dto = Map(b);

                switch (b.Status)
                {
                    case BookingStatus.Pending:
                        resp.Pending.Add(dto);
                        break;
                    case BookingStatus.InProgress:
                        resp.Active.Add(dto);
                        break;
                    case BookingStatus.Completed:
                        resp.Completed.Add(dto);
                        break;
                    case BookingStatus.Cancelled:
                        resp.Cancelled.Add(dto);
                        break;
                }
            }

            return resp;
        }






        private async Task RequireCashierAsync(int cashierId)
        {
            var empRepo = _uow.Repository<Employee>();
            var emp = await empRepo.GetByIdAsync(cashierId);

            if (emp == null || !emp.IsActive)
                throw new BusinessException("Cashier not found", 404);

            if (emp.Role != EmployeeRole.Cashier &&
                emp.Role != EmployeeRole.Supervisor &&
                emp.Role != EmployeeRole.Admin)
                throw new BusinessException("Not allowed", 403);
        }







        public async Task<BookingServiceOptionsResponse> GetServiceOptionsAsync(int bookingId)
        {
            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();
            var serviceRepo = _uow.Repository<Service>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            // booking items (exclude cancelled)
            var items = await itemRepo.FindAsync(i => i.BookingId == bookingId && i.Status != BookingItemStatus.Cancelled);

            var inServiceIds = items.Select(i => i.ServiceId).Distinct().ToHashSet();

            // all active services
            var allServices = await serviceRepo.FindAsync(s => s.IsActive);
            var serviceMap = allServices.ToDictionary(s => s.Id, s => s.Name);

            // build "in booking" list
            var inBooking = items.Select(i =>
            {
                serviceMap.TryGetValue(i.ServiceId, out var name);

                return new ServiceInBookingDto
                {
                    BookingItemId = i.Id,
                    ServiceId = i.ServiceId,
                    ServiceName = name ?? "",

                    Status = i.Status,
                    UnitPrice = i.UnitPrice,
                    DurationMinutes = i.DurationMinutes,
                    AssignedEmployeeId = i.AssignedEmployeeId
                };
            })
            .OrderBy(x => x.ServiceName)
            .ToList();

            // build "not in booking" list
            var notInBooking = allServices
                .Where(s => !inServiceIds.Contains(s.Id))
                .OrderBy(s => s.Name)
                .Select(s => new ServiceOptionDto
                {
                    ServiceId = s.Id,
                    ServiceName = s.Name
                })
                .ToList();

            return new BookingServiceOptionsResponse
            {
                BookingId = bookingId,
                InBooking = inBooking,
                NotInBooking = notInBooking
            };
        }






    }

}