using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.Abstractions.Services.Ops.Usage;
using Forto.Application.DTOs.Ops.Usage;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace Forto.Application.Abstractions.Services.Ops.Usage
{
    public class BookingItemMaterialsService : IBookingItemMaterialsService
    {
        private readonly IUnitOfWork _uow;



        public BookingItemMaterialsService(IUnitOfWork uow) => _uow = uow;



        //public async Task<BookingItemMaterialsResponse> GetAsync(int bookingItemId, int employeeId)
        //{
        //    // item exists
        //    var itemRepo = _uow.Repository<BookingItem>();
        //    var item = await itemRepo.GetByIdAsync(bookingItemId);
        //    if (item == null) throw new BusinessException("Booking item not found", 404);

        //    // must be assigned to employee (or cashier/supervisor later)
        //    if (item.AssignedEmployeeId != employeeId)
        //        throw new BusinessException("Not allowed", 403);

        //    var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
        //    var usages = await usageRepo.FindAsync(u => u.BookingItemId == bookingItemId);
        //    if (usages.Count == 0)
        //        throw new BusinessException("No materials were reserved for this item (start the service first)", 409);

        //    var materialRepo = _uow.Repository<Material>();
        //    var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();
        //    var mats = await materialRepo.FindAsync(m => materialIds.Contains(m.Id));
        //    var matMap = mats.ToDictionary(m => m.Id, m => m);

        //    return Map(bookingItemId, usages, matMap);
        //}




















        public async Task<BookingItemMaterialsResponse> GetAsync(int bookingItemId)
        {
            // item exists
            var itemRepo = _uow.Repository<BookingItem>();
            var item = await itemRepo.GetByIdAsync(bookingItemId);
            if (item == null) throw new BusinessException("Booking item not found", 404);


            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
            var usages = await usageRepo.FindAsync(u => u.BookingItemId == bookingItemId);
            if (usages.Count == 0)
                throw new BusinessException("No materials were reserved for this item (start the service first)", 409);

            var materialRepo = _uow.Repository<Material>();
            var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();
            var mats = await materialRepo.FindAsync(m => materialIds.Contains(m.Id));
            var matMap = mats.ToDictionary(m => m.Id, m => m);

            return Map(bookingItemId, usages, matMap);
        }



        public async Task<BookingItemMaterialsResponse> UpdateActualAsync(int bookingItemId, UpdateBookingItemMaterialsRequest request)
        {
            var employeeId = request.EmployeeId;

            var itemRepo = _uow.Repository<BookingItem>();
            var bookingRepo = _uow.Repository<Booking>();

            var item = await itemRepo.GetByIdAsync(bookingItemId);
            if (item == null) throw new BusinessException("Booking item not found", 404);

            // must be in progress
            if (item.Status != BookingItemStatus.InProgress)
                throw new BusinessException("Materials can be updated only while service is InProgress", 409);

            // only assigned employee can update
            if (item.AssignedEmployeeId != employeeId)
                throw new BusinessException("Not allowed", 403);

            var booking = await bookingRepo.GetByIdAsync(item.BookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            var branchId = booking.BranchId;

            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
            // ✅ Tracking fetch (important)
            var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == bookingItemId);
            if (usages.Count == 0)
                throw new BusinessException("No materials were reserved for this item (start the service first)", 409);

            // Validate request material ids exist in this booking item usage
            var usageMaterialIds = usages.Select(u => u.MaterialId).ToHashSet();
            var reqDups = request.Materials.GroupBy(x => x.MaterialId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (reqDups.Any())
                throw new BusinessException("Duplicate materialId in request", 400);

            var invalid = request.Materials.Where(m => !usageMaterialIds.Contains(m.MaterialId)).Select(m => m.MaterialId).ToList();
            if (invalid.Any())
                throw new BusinessException("Some materials are not part of this service recipe", 400,
                    new Dictionary<string, string[]>
                    {
                        ["materialId"] = invalid.Select(x => $"MaterialId {x} not in this booking item").ToArray()
                    });

            // Load branch stock tracking
            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var materialIds = usages.Select(u => u.MaterialId).Distinct().ToList();
            var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
            var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

            // Load materials names/units for response
            var materialRepo = _uow.Repository<Material>();
            var mats = await materialRepo.FindAsync(m => materialIds.Contains(m.Id));
            var matMap = mats.ToDictionary(m => m.Id, m => m);

            // Apply updates one by one
            foreach (var req in request.Materials)
            {
                var usage = usages.First(u => u.MaterialId == req.MaterialId);

                var newActual = req.ActualQty;
                if (newActual < 0) newActual = 0;

                var currentReserved = usage.ReservedQty; // reserved currently equals the last actual we ensured
                var delta = newActual - currentReserved;

                // If increasing: need available in stock
                if (delta > 0)
                {
                    if (!stockMap.TryGetValue(req.MaterialId, out var stock))
                        throw new BusinessException("Stock row missing for a material in this branch", 409);

                    var available = stock.OnHandQty - stock.ReservedQty;
                    if (available < 0) available = 0;

                    if (available < delta)
                    {
                        var matName = matMap.TryGetValue(req.MaterialId, out var mm) ? mm.Name : $"MaterialId={req.MaterialId}";
                        throw new BusinessException("Not enough stock for this increase", 409,
                            new Dictionary<string, string[]>
                            {
                                ["materials"] = new[] { $"{matName}: need extra {delta}, available {available}" }
                            });
                    }

                    // reserve the delta
                    stock.ReservedQty += delta;
                    stockRepo.Update(stock);

                    usage.ReservedQty += delta;
                }
                else if (delta < 0)
                {
                    // decreasing: release (-delta)
                    if (!stockMap.TryGetValue(req.MaterialId, out var stock))
                        throw new BusinessException("Stock row missing for a material in this branch", 409);

                    var release = -delta;

                    // release reserved
                    stock.ReservedQty -= release;
                    if (stock.ReservedQty < 0) stock.ReservedQty = 0;
                    stockRepo.Update(stock);

                    usage.ReservedQty -= release;
                    if (usage.ReservedQty < 0) usage.ReservedQty = 0;
                }


                // update actual
                usage.ActualQty = newActual;

                // تسجيل سعر التكلفة وسعر البيع وقت التسجيل (دلوقتي)
                if (matMap.TryGetValue(usage.MaterialId, out var mat))
                {
                    usage.UnitCost = mat.CostPerUnit;
                    usage.UnitCharge = mat.ChargePerUnit;
                }

                var diffQty = usage.ActualQty - usage.DefaultQty; // ممكن بالسالب
                usage.ExtraCharge = diffQty * usage.UnitCharge;

                usage.RecordedByEmployeeId = employeeId;
                usage.RecordedAt = DateTime.UtcNow;

                usageRepo.Update(usage);
            }

            // ✅ ONE SAVE
            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException("Stock was modified by another operation. Please retry.", 409);
            }

            // Return updated view
            return Map(bookingItemId, usages, matMap);
        }



        private static BookingItemMaterialsResponse Map(
            int bookingItemId,
            IReadOnlyList<BookingItemMaterialUsage> usages,
            Dictionary<int, Material> matMap)
        {
            var list = usages.Select(u =>
            {
                matMap.TryGetValue(u.MaterialId, out var mat);
                return new BookingItemMaterialDto
                {
                    MaterialId = u.MaterialId,
                    MaterialName = mat?.Name ?? "",
                    Unit = mat?.Unit.ToString() ?? "",
                    DefaultQty = u.DefaultQty,
                    ReservedQty = u.ReservedQty,
                    ActualQty = u.ActualQty,
                    UnitCost = u.UnitCost,
                    UnitCharge = u.UnitCharge,
                    ExtraCharge = u.ExtraCharge
                };
            }).ToList();

            return new BookingItemMaterialsResponse
            {
                BookingItemId = bookingItemId,
                Materials = list,
                TotalExtraCharge = list.Sum(x => x.ExtraCharge)
            };
        }
    }
}