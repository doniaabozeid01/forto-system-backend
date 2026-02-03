using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Billings.Gifts;
using Forto.Application.DTOs.Catalog;
using Forto.Application.DTOs.Catalog.Services;
using Forto.Application.DTOs.Employees;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Catalogs.Service
{
    public class CatalogService : ICatalogService
    {
        private readonly IUnitOfWork _uow;

        public CatalogService(IUnitOfWork uow) => _uow = uow;

        public async Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request)
        {
            // optional: validate category exists later لو حابة
            var repo = _uow.Repository<Domain.Entities.Catalog.Service>();

            var entity = new Domain.Entities.Catalog.Service
            {
                CategoryId = request.CategoryId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                IsActive = true
            };

            await repo.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return new ServiceResponse
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                Name = entity.Name,
                Description = entity.Description,
                Rates = new()
            };
        }

        public async Task<ServiceResponse?> GetServiceAsync(int id)
        {
            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var rateRepo = _uow.Repository<ServiceRate>();

            var s = await serviceRepo.GetByIdAsync(id);
            if (s == null) return null;

            var rates = await rateRepo.FindAsync(r => r.ServiceId == id);

            return Map(s, rates);
        }

        public async Task<IReadOnlyList<ServiceResponse>> GetServicesAsync(int? categoryId = null)
        {
            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var rateRepo = _uow.Repository<ServiceRate>();

            var services = categoryId.HasValue
                ? await serviceRepo.FindAsync(s => s.CategoryId == categoryId.Value)
                : await serviceRepo.GetAllAsync();

            // جمع rates لكل الخدمات (simple version)
            var serviceIds = services.Select(s => s.Id).ToList();
            var rates = serviceIds.Count == 0
                ? new List<ServiceRate>()
                : (await rateRepo.FindAsync(r => serviceIds.Contains(r.ServiceId))).ToList();

            var ratesGrouped = rates.GroupBy(r => r.ServiceId).ToDictionary(g => g.Key, g => g.ToList());

            return services.Select(s =>
            {
                ratesGrouped.TryGetValue(s.Id, out var r);
                return Map(s, r ?? new List<ServiceRate>());
            }).ToList();
        }

        public async Task<ServiceResponse?> UpsertRatesAsync(int serviceId, UpsertServiceRatesRequest request)
        {
            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var rateRepo = _uow.Repository<ServiceRate>();

            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null) return null;

            // منع تكرار BodyType في نفس الطلب
            var dups = request.Rates.GroupBy(x => x.BodyType).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (dups.Any())
                throw new BusinessException("Duplicate body types in rates", 400,
                    new Dictionary<string, string[]>
                    {
                        ["rates"] = dups.Select(x => $"{x} duplicated").ToArray()
                    });

            var existing = await rateRepo.FindAsync(r => r.ServiceId == serviceId);
            var map = existing.ToDictionary(x => x.BodyType, x => x);

            foreach (var r in request.Rates)
            {
                if (r.DurationMinutes <= 0)
                    throw new BusinessException("DurationMinutes must be > 0", 400);

                if (map.TryGetValue(r.BodyType, out var row))
                {
                    row.Price = r.Price;
                    row.DurationMinutes = r.DurationMinutes;
                    row.IsActive = true;
                    rateRepo.Update(row);
                }
                else
                {
                    await rateRepo.AddAsync(new ServiceRate
                    {
                        ServiceId = serviceId,
                        BodyType = r.BodyType,
                        Price = r.Price,
                        DurationMinutes = r.DurationMinutes,
                        IsActive = true
                    });
                }
            }

            await _uow.SaveChangesAsync();

            var after = await rateRepo.FindAsync(x => x.ServiceId == serviceId);
            return Map(service, after);
        }

        public async Task<bool> DeleteServiceAsync(int id)
        {
            var repo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var s = await repo.GetByIdAsync(id);
            if (s == null) return false;

            repo.Delete(s);
            await _uow.SaveChangesAsync();
            return true;
        }

        private static ServiceResponse Map(Domain.Entities.Catalog.Service s, IReadOnlyList<ServiceRate> rates)
        {
            return new ServiceResponse
            {
                Id = s.Id,
                CategoryId = s.CategoryId,
                Name = s.Name,
                Description = s.Description,
                Rates = rates
                    .OrderBy(x => (int)x.BodyType)
                    .Select(x => new ServiceRateResponse
                    {
                        Id = x.Id,
                        BodyType = x.BodyType,
                        Price = x.Price,
                        DurationMinutes = x.DurationMinutes
                    }).ToList()
            };
        }



        public async Task<IReadOnlyList<EmployeeResponse>> GetEmployeesForServiceAsync(int serviceId)
        {
            // تأكد الخدمة موجودة (اختياري بس لطيف)
            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null)
                throw new BusinessException("Service not found", 404);

            var linkRepo = _uow.Repository<EmployeeService>();
            var employeeRepo = _uow.Repository<Employee>();

            // هات الروابط الفعالة
            var links = await linkRepo.FindAsync(x => x.ServiceId == serviceId && x.IsActive);

            var employeeIds = links.Select(x => x.EmployeeId).Distinct().ToList();
            if (employeeIds.Count == 0)
                return new List<EmployeeResponse>();

            var employees = await employeeRepo.FindAsync(e => employeeIds.Contains(e.Id) && e.IsActive);

            return employees
                .OrderBy(e => e.Name)
                .Select(e => new EmployeeResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    PhoneNumber = e.PhoneNumber,
                    IsActive = e.IsActive
                })
                .ToList();
        }





        //public async Task<IReadOnlyList<EmployeeResponse>> GetEmployeesForServiceAtAsync(
        //int serviceId,
        //DateTime scheduledStart,
        //int branchId)
        //{
        //    // 0) validate inputs
        //    var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
        //    if (branch == null || !branch.IsActive)
        //        throw new BusinessException("Branch not found", 404);

        //    // (اختياري) validate service exists
        //    var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
        //    var service = await serviceRepo.GetByIdAsync(serviceId);
        //    if (service == null)
        //        throw new BusinessException("Service not found", 404);

        //    // normalize to slot hour start (your MVP rule)
        //    var slotHourStart = new DateTime(
        //        scheduledStart.Year, scheduledStart.Month, scheduledStart.Day,
        //        scheduledStart.Hour, 0, 0);

        //    // 1) qualified employees for this service
        //    var linkRepo = _uow.Repository<EmployeeService>();
        //    var links = await linkRepo.FindAsync(x => x.ServiceId == serviceId && x.IsActive);

        //    var employeeIds = links.Select(x => x.EmployeeId).Distinct().ToList();
        //    if (employeeIds.Count == 0)
        //        return new List<EmployeeResponse>();

        //    // 2) filter employees who are working at that time (schedule/shift)
        //    var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
        //    var shiftRepo = _uow.Repository<Domain.Entities.Employees.Shift>();

        //    var dow = DateOnly.FromDateTime(slotHourStart).DayOfWeek;
        //    var schedules = await scheduleRepo.FindAsync(s =>
        //        employeeIds.Contains(s.EmployeeId) &&
        //        s.DayOfWeek == dow &&
        //        !s.IsOff);

        //    if (schedules.Count == 0)
        //        return new List<EmployeeResponse>();

        //    // load shifts referenced
        //    var shiftIds = schedules.Where(s => s.ShiftId.HasValue).Select(s => s.ShiftId!.Value).Distinct().ToList();
        //    var shifts = shiftIds.Count == 0
        //        ? new List<Domain.Entities.Employees.Shift>()
        //        : (await shiftRepo.FindAsync(x => shiftIds.Contains(x.Id))).ToList();

        //    var shiftMap = shifts.ToDictionary(x => x.Id, x => x);

        //    var hour = TimeOnly.FromDateTime(slotHourStart);

        //    bool IsWorking(EmployeeWorkSchedule s)
        //    {
        //        TimeOnly? start = s.StartTime;
        //        TimeOnly? end = s.EndTime;

        //        if (s.ShiftId.HasValue && shiftMap.TryGetValue(s.ShiftId.Value, out var sh))
        //        {
        //            start ??= sh.StartTime;
        //            end ??= sh.EndTime;
        //        }

        //        if (start == null || end == null) return false;

        //        // MVP: shift does not cross midnight
        //        return hour >= start.Value && hour < end.Value;
        //    }

        //    var workingEmployeeIds = schedules
        //        .Where(IsWorking)
        //        .Select(s => s.EmployeeId)
        //        .Distinct()
        //        .ToHashSet();

        //    if (workingEmployeeIds.Count == 0)
        //        return new List<EmployeeResponse>();

        //    // 3) filter out busy employees in same slot (branch + hour)
        //    // busy = they already have a booking item assigned in another booking at that hour (not cancelled/completed)
        //    var bookingRepo = _uow.Repository<Booking>();
        //    var itemRepo = _uow.Repository<BookingItem>();

        //    var bookingsInSlot = await bookingRepo.FindAsync(b =>
        //        b.BranchId == branchId &&
        //        b.SlotHourStart == slotHourStart &&
        //        b.Status != BookingStatus.Cancelled &&
        //        b.Status != BookingStatus.Completed);

        //    if (bookingsInSlot.Count > 0)
        //    {
        //        var bookingIds = bookingsInSlot.Select(b => b.Id).ToList();

        //        var busyItems = await itemRepo.FindAsync(i =>
        //            bookingIds.Contains(i.BookingId) &&
        //            i.AssignedEmployeeId != null &&
        //            workingEmployeeIds.Contains(i.AssignedEmployeeId.Value) &&
        //            i.Status != BookingItemStatus.Cancelled &&
        //            i.Status != BookingItemStatus.Done);

        //        var busyEmployeeIds = busyItems
        //            .Select(i => i.AssignedEmployeeId!.Value)
        //            .Distinct()
        //            .ToHashSet();

        //        // available = working - busy
        //        workingEmployeeIds.ExceptWith(busyEmployeeIds);
        //    }

        //    if (workingEmployeeIds.Count == 0)
        //        return new List<EmployeeResponse>();

        //    // 4) load employees details
        //    var employeeRepo = _uow.Repository<Employee>();
        //    var employees = await employeeRepo.FindAsync(e =>
        //        workingEmployeeIds.Contains(e.Id) && e.IsActive);

        //    return employees
        //        .OrderBy(e => e.Name)
        //        .Select(e => new EmployeeResponse
        //        {
        //            Id = e.Id,
        //            Name = e.Name,
        //            PhoneNumber = e.PhoneNumber,
        //            IsActive = e.IsActive
        //        })
        //        .ToList();
        //}



        public async Task<EmployeeAvailabilityResponse> GetEmployeesForServiceAtAsync(
    int bookingId,
    int serviceId,
    DateTime scheduledStart)
        {
            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();
            var linkRepo = _uow.Repository<EmployeeService>();
            var empRepo = _uow.Repository<Employee>();
            var scheduleRepo = _uow.Repository<EmployeeWorkSchedule>();
            var shiftRepo = _uow.Repository<Domain.Entities.Employees.Shift>();

            var booking = await bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            var branchId = booking.BranchId;

            // normalize to hour slot
            var slotHourStart = new DateTime(
                scheduledStart.Year, scheduledStart.Month, scheduledStart.Day,
                scheduledStart.Hour, 0, 0);

            // 1) qualified employees for service
            var links = await linkRepo.FindAsync(x => x.ServiceId == serviceId && x.IsActive);
            var qualifiedIds = links.Select(x => x.EmployeeId).Distinct().ToList();
            if (qualifiedIds.Count == 0)
                return new EmployeeAvailabilityResponse { BookingId = bookingId, ServiceId = serviceId, SlotHourStart = slotHourStart };

            // 2) working at that time (schedule/shift)
            var dow = DateOnly.FromDateTime(slotHourStart).DayOfWeek;
            var schedules = await scheduleRepo.FindAsync(s =>
                qualifiedIds.Contains(s.EmployeeId) &&
                s.DayOfWeek == dow &&
                !s.IsOff);

            if (schedules.Count == 0)
                return new EmployeeAvailabilityResponse { BookingId = bookingId, ServiceId = serviceId, SlotHourStart = slotHourStart };

            var shiftIds = schedules.Where(s => s.ShiftId.HasValue).Select(s => s.ShiftId!.Value).Distinct().ToList();
            var shifts = shiftIds.Count == 0 ? new List<Domain.Entities.Employees.Shift>() : (await shiftRepo.FindAsync(x => shiftIds.Contains(x.Id))).ToList();
            var shiftMap = shifts.ToDictionary(x => x.Id, x => x);

            var hour = TimeOnly.FromDateTime(slotHourStart);

            bool IsWorking(EmployeeWorkSchedule s)
            {
                TimeOnly? start = s.StartTime;
                TimeOnly? end = s.EndTime;

                if (s.ShiftId.HasValue && shiftMap.TryGetValue(s.ShiftId.Value, out var sh))
                {
                    start ??= sh.StartTime;
                    end ??= sh.EndTime;
                }

                if (start == null || end == null) return false;
                return hour >= start.Value && hour < end.Value;
            }

            var workingIds = schedules.Where(IsWorking).Select(s => s.EmployeeId).Distinct().ToHashSet();
            if (workingIds.Count == 0)
                return new EmployeeAvailabilityResponse { BookingId = bookingId, ServiceId = serviceId, SlotHourStart = slotHourStart };

            // 3) find employees busy at same slot in same branch (BUT ignore same bookingId)
            var bookingsInSlot = await bookingRepo.FindAsync(b =>
                b.BranchId == branchId &&
                b.SlotHourStart == slotHourStart &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Completed);

            var bookingIdsInSlot = bookingsInSlot.Select(b => b.Id).ToList();

            // get items assigned to these employees within those bookings
            var itemsInSlot = await itemRepo.FindAsync(i =>
                bookingIdsInSlot.Contains(i.BookingId) &&
                i.AssignedEmployeeId != null &&
                workingIds.Contains(i.AssignedEmployeeId.Value) &&
                i.Status != BookingItemStatus.Cancelled &&
                i.Status != BookingItemStatus.Done);

            // Busy if BookingId != current bookingId
            var busyMap = itemsInSlot
                .Where(i => i.BookingId != bookingId)
                .GroupBy(i => i.AssignedEmployeeId!.Value)
                .ToDictionary(g => g.Key, g => g.First().BookingId);

            // 4) load employee names
            var employees = await empRepo.FindAsync(e => workingIds.Contains(e.Id) && e.IsActive);
            var empNameMap = employees.ToDictionary(e => e.Id, e => e.Name);

            var resp = new EmployeeAvailabilityResponse
            {
                BookingId = bookingId,
                ServiceId = serviceId,
                SlotHourStart = slotHourStart
            };

            foreach (var empId in workingIds.OrderBy(x => x))
            {
                empNameMap.TryGetValue(empId, out var name);
                name ??= "";

                if (busyMap.TryGetValue(empId, out var busyBookingId))
                {
                    resp.BusyEmployees.Add(new BusyEmployeeDto
                    {
                        EmployeeId = empId,
                        Name = name,
                        BusyBookingId = busyBookingId,
                        Reason = $"Busy in another booking (#{busyBookingId}) at {hour:HH\\:mm}"
                    });
                }
                else
                {
                    resp.AvailableEmployees.Add(new EmployeeSimpleDto
                    {
                        EmployeeId = empId,
                        Name = name
                    });
                }
            }

            return resp;
        }

        public async Task<GiftOptionsByServicesResponse> GetGiftOptionsByServiceIdsAsync(IReadOnlyList<int> serviceIds, int? branchId = null)
        {
            var resp = new GiftOptionsByServicesResponse
            {
                ServiceIds = serviceIds?.ToList() ?? new List<int>(),
                BranchId = branchId
            };
            if (serviceIds == null || serviceIds.Count == 0)
                return resp;

            var giftOptRepo = _uow.Repository<ServiceGiftOption>();
            var giftOptions = await giftOptRepo.FindAsync(o => o.IsActive && serviceIds.Contains(o.ServiceId));
            var productIds = giftOptions.Select(o => o.ProductId).Distinct().ToList();
            if (productIds.Count == 0)
                return resp;

            var productRepo = _uow.Repository<Product>();
            var products = await productRepo.FindAsync(p => productIds.Contains(p.Id) && p.IsActive);
            var productMap = products.ToDictionary(p => p.Id, p => p);

            var options = new List<GiftOptionDto>();
            if (branchId.HasValue)
            {
                var stockRepo = _uow.Repository<BranchProductStock>();
                var stocks = await stockRepo.FindAsync(s => s.BranchId == branchId.Value && productIds.Contains(s.ProductId));
                var stockMap = stocks.ToDictionary(s => s.ProductId, s => s);
                foreach (var pid in productIds)
                {
                    productMap.TryGetValue(pid, out var p);
                    stockMap.TryGetValue(pid, out var st);
                    var onHand = st?.OnHandQty ?? 0m;
                    var reserved = st?.ReservedQty ?? 0m;
                    var available = onHand - reserved;
                    if (available < 0) available = 0;
                    options.Add(new GiftOptionDto
                    {
                        ProductId = pid,
                        ProductName = p?.Name ?? "",
                        Sku = p?.Sku,
                        AvailableQty = available
                    });
                }
            }
            else
            {
                foreach (var pid in productIds)
                {
                    productMap.TryGetValue(pid, out var p);
                    options.Add(new GiftOptionDto
                    {
                        ProductId = pid,
                        ProductName = p?.Name ?? "",
                        Sku = p?.Sku,
                        AvailableQty = 0
                    });
                }
            }

            resp.Options = options.OrderByDescending(x => x.AvailableQty).ThenBy(x => x.ProductName).ToList();
            return resp;
        }

        public async Task<IReadOnlyList<ServiceGiftOptionDto>> GetGiftOptionsForServiceAsync(int serviceId)
        {
            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null)
                throw new BusinessException("Service not found", 404);

            var giftOptRepo = _uow.Repository<ServiceGiftOption>();
            var productRepo = _uow.Repository<Product>();
            var opts = await giftOptRepo.FindAsync(o => o.ServiceId == serviceId);
            var productIds = opts.Select(o => o.ProductId).Distinct().ToList();
            var products = productIds.Count == 0 ? new List<Product>() : (await productRepo.FindAsync(p => productIds.Contains(p.Id))).ToList();
            var productMap = products.ToDictionary(p => p.Id, p => p);

            return opts.Select(o =>
            {
                productMap.TryGetValue(o.ProductId, out var p);
                return new ServiceGiftOptionDto
                {
                    Id = o.Id,
                    ServiceId = o.ServiceId,
                    ProductId = o.ProductId,
                    ProductName = p?.Name ?? "",
                    ProductSku = p?.Sku,
                    IsActive = o.IsActive
                };
            }).OrderBy(x => x.ProductName).ToList();
        }

        public async Task<IReadOnlyList<ServiceGiftOptionDto>> AddGiftOptionsToServiceAsync(int serviceId, IReadOnlyList<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                return new List<ServiceGiftOptionDto>();

            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null)
                throw new BusinessException("Service not found", 404);

            var giftOptRepo = _uow.Repository<ServiceGiftOption>();
            var productRepo = _uow.Repository<Product>();
            var existing = await giftOptRepo.FindAsync(o => o.ServiceId == serviceId);
            var existingProductIds = existing.Select(o => o.ProductId).ToHashSet();
            var distinctIds = productIds.Where(id => id > 0).Distinct().ToList();
            var result = new List<ServiceGiftOptionDto>();

            foreach (var productId in distinctIds)
            {
                if (existingProductIds.Contains(productId))
                    continue;
                var product = await productRepo.GetByIdAsync(productId);
                if (product == null || !product.IsActive)
                    continue;
                var entity = new ServiceGiftOption
                {
                    ServiceId = serviceId,
                    ProductId = productId,
                    IsActive = true
                };
                await giftOptRepo.AddAsync(entity);
                await _uow.SaveChangesAsync();
                existingProductIds.Add(productId);
                result.Add(new ServiceGiftOptionDto
                {
                    Id = entity.Id,
                    ServiceId = entity.ServiceId,
                    ProductId = entity.ProductId,
                    ProductName = product.Name,
                    ProductSku = product.Sku,
                    IsActive = entity.IsActive
                });
            }

            return result;
        }

        public async Task<int> RemoveGiftOptionsFromServiceAsync(int serviceId, IReadOnlyList<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                return 0;
            var giftOptRepo = _uow.Repository<ServiceGiftOption>();
            var ids = productIds.Where(id => id > 0).Distinct().ToList();
            var opts = await giftOptRepo.FindTrackingAsync(o => o.ServiceId == serviceId && ids.Contains(o.ProductId));
            var count = opts.Count;
            foreach (var opt in opts)
                giftOptRepo.Delete(opt);
            if (count > 0)
                await _uow.SaveChangesAsync();
            return count;
        }
    }
}