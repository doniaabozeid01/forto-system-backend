using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Catalog.Recipes;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Catalogs.Recipes.MaterialApprove
{
    public class MaterialApprovalService : IMaterialApprovalService
    {
        readonly IUnitOfWork _uow;
        public MaterialApprovalService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<int> CreateRequestAsync(int bookingItemId, CreateMaterialChangeRequestDto dto)
        {
            var itemRepo = _uow.Repository<BookingItem>();
            var reqRepo = _uow.Repository<BookingItemMaterialChangeRequest>();
            var lineRepo = _uow.Repository<BookingItemMaterialChangeRequestLine>();
            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();

            var item = await itemRepo.GetByIdAsync(bookingItemId);
            if (item == null) throw new BusinessException("Booking item not found", 404);

            if (item.Status != BookingItemStatus.InProgress)
                throw new BusinessException("Can request change only while InProgress", 409);

            if (item.AssignedEmployeeId != dto.EmployeeId)
                throw new BusinessException("Only assigned employee can request", 403);

            // prevent multiple pending
            var existingPending = (await reqRepo.FindAsync(r => r.BookingItemId == bookingItemId && r.Status == MaterialChangeRequestStatus.Pending))
                .FirstOrDefault();
            if (existingPending != null)
                throw new BusinessException("There is already a pending material request for this service", 409);

            var usages = await usageRepo.FindAsync(u => u.BookingItemId == bookingItemId);
            if (usages.Count == 0)
                throw new BusinessException("No reserved materials found (start first)", 409);

            var validMaterialIds = usages.Select(u => u.MaterialId).ToHashSet();

            if (dto.Materials == null || dto.Materials.Count == 0)
                throw new BusinessException("Materials is required", 400);

            // validate material ids exist
            var invalid = dto.Materials.Where(x => !validMaterialIds.Contains(x.MaterialId)).Select(x => x.MaterialId).ToList();
            if (invalid.Any())
                throw new BusinessException("Some materials are not part of this service recipe", 400);

            // create request header
            var req = new BookingItemMaterialChangeRequest
            {
                BookingItemId = bookingItemId,
                Status = MaterialChangeRequestStatus.Pending,
                RequestedByEmployeeId = dto.EmployeeId,
                RequestedAt = DateTime.UtcNow
            };
            await reqRepo.AddAsync(req);
            await _uow.SaveChangesAsync(); // get req.Id

            // lines
            foreach (var m in dto.Materials)
            {
                var proposed = m.ProposedActualQty;
                if (proposed < 0) proposed = 0;

                await lineRepo.AddAsync(new BookingItemMaterialChangeRequestLine
                {
                    RequestId = req.Id,
                    MaterialId = m.MaterialId,
                    ProposedActualQty = proposed
                });
            }

            await _uow.SaveChangesAsync();
            return req.Id;
        }



        public async Task ApproveAsync(int requestId, ReviewMaterialChangeRequestDto dto)
        {
            await RequireCashierAsync(dto.CashierId);

            var reqRepo = _uow.Repository<BookingItemMaterialChangeRequest>();
            var lineRepo = _uow.Repository<BookingItemMaterialChangeRequestLine>();
            var itemRepo = _uow.Repository<BookingItem>();
            var bookingRepo = _uow.Repository<Booking>();
            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
            var stockRepo = _uow.Repository<BranchMaterialStock>();

            var req = await reqRepo.GetByIdAsync(requestId);
            if (req == null) throw new BusinessException("Request not found", 404);

            if (req.Status != MaterialChangeRequestStatus.Pending)
                throw new BusinessException("Request is not pending", 409);

            var item = await itemRepo.GetByIdAsync(req.BookingItemId);
            if (item == null) throw new BusinessException("Booking item not found", 404);

            if (item.Status != BookingItemStatus.InProgress)
                throw new BusinessException("Cannot approve unless service is InProgress", 409);

            var booking = await bookingRepo.GetByIdAsync(item.BookingId);
            if (booking == null) throw new BusinessException("Booking not found", 404);

            var branchId = booking.BranchId;

            var lines = await lineRepo.FindAsync(l => l.RequestId == requestId);
            if (lines.Count == 0) throw new BusinessException("Request has no lines", 409);

            // tracking usages
            var usages = await usageRepo.FindTrackingAsync(u => u.BookingItemId == req.BookingItemId);
            var usageMap = usages.ToDictionary(u => u.MaterialId, u => u);

            var materialIds = lines.Select(l => l.MaterialId).Distinct().ToList();

            // tracking stocks
            var stocks = await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
            var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

            foreach (var line in lines)
            {
                if (!usageMap.TryGetValue(line.MaterialId, out var usage))
                    throw new BusinessException("Material not found in usage", 409);

                if (!stockMap.TryGetValue(line.MaterialId, out var stock))
                    throw new BusinessException("Stock row missing for a material in this branch", 409);

                var newActual = line.ProposedActualQty;
                if (newActual < 0) newActual = 0;

                var currentReserved = usage.ReservedQty;
                var delta = newActual - currentReserved;

                if (delta > 0)
                {
                    var available = stock.OnHandQty - stock.ReservedQty;
                    if (available < 0) available = 0;

                    if (available < delta)
                        throw new BusinessException("Not enough stock to approve this increase", 409);

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

                // Apply actual now
                usage.ActualQty = newActual;

                // your “net adjustment”
                var diffQty = usage.ActualQty - usage.DefaultQty;
                usage.ExtraCharge = diffQty * usage.UnitCharge;

                usage.RecordedByEmployeeId = req.RequestedByEmployeeId; // worker is owner
                usage.RecordedAt = DateTime.UtcNow;

                usageRepo.Update(usage);
            }

            // mark request approved
            req.Status = MaterialChangeRequestStatus.Approved;
            req.ReviewedByCashierId = dto.CashierId;
            req.ReviewedAt = DateTime.UtcNow;
            req.ReviewNote = dto.Note;
            reqRepo.Update(req);

            await _uow.SaveChangesAsync();
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



        public async Task RejectAsync(int requestId, ReviewMaterialChangeRequestDto dto)
        {
            await RequireCashierAsync(dto.CashierId);

            var reqRepo = _uow.Repository<BookingItemMaterialChangeRequest>();
            var req = await reqRepo.GetByIdAsync(requestId);

            if (req == null)
                throw new BusinessException("Request not found", 404);

            if (req.Status != MaterialChangeRequestStatus.Pending)
                throw new BusinessException("Request is not pending", 409);

            req.Status = MaterialChangeRequestStatus.Rejected;
            req.ReviewedByCashierId = dto.CashierId;
            req.ReviewedAt = DateTime.UtcNow;
            req.ReviewNote = dto.Note;

            reqRepo.Update(req);
            await _uow.SaveChangesAsync();
        }






        public async Task<IReadOnlyList<PendingMaterialRequestDto>> ListPendingAsync(int branchId, DateOnly date)
        {
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            var dayStart = date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var reqRepo = _uow.Repository<BookingItemMaterialChangeRequest>();
            var lineRepo = _uow.Repository<BookingItemMaterialChangeRequestLine>();
            var itemRepo = _uow.Repository<BookingItem>();
            var bookingRepo = _uow.Repository<Booking>();
            var empRepo = _uow.Repository<Employee>();
            var usageRepo = _uow.Repository<BookingItemMaterialUsage>();
            var materialRepo = _uow.Repository<Material>();

            // bookings in branch on that day
            var bookings = await bookingRepo.FindAsync(b =>
                b.BranchId == branchId &&
                b.ScheduledStart >= dayStart &&
                b.ScheduledStart < dayEnd);

            if (bookings.Count == 0)
                return new List<PendingMaterialRequestDto>();

            var bookingIds = bookings.Select(b => b.Id).ToHashSet();
            var bookingMap = bookings.ToDictionary(b => b.Id, b => b);

            // booking items for those bookings
            var bookingItems = await itemRepo.FindAsync(i => bookingIds.Contains(i.BookingId));
            var bookingItemMap = bookingItems.ToDictionary(i => i.Id, i => i);

            // pending requests for those booking items
            var bookingItemIds = bookingItems.Select(i => i.Id).ToList();
            var pendingRequests = await reqRepo.FindAsync(r =>
                bookingItemIds.Contains(r.BookingItemId) &&
                r.Status == MaterialChangeRequestStatus.Pending);

            if (pendingRequests.Count == 0)
                return new List<PendingMaterialRequestDto>();

            var reqIds = pendingRequests.Select(r => r.Id).ToList();

            var reqLines = await lineRepo.FindAsync(l => reqIds.Contains(l.RequestId));
            var linesByReq = reqLines.GroupBy(l => l.RequestId).ToDictionary(g => g.Key, g => g.ToList());

            // employees (requested by)
            var empIds = pendingRequests.Select(r => r.RequestedByEmployeeId).Distinct().ToList();
            var emps = await empRepo.FindAsync(e => empIds.Contains(e.Id));
            var empMap = emps.ToDictionary(e => e.Id, e => e.Name);

            // usages (for default + current actual)
            var usageBookingItemIds = pendingRequests.Select(r => r.BookingItemId).Distinct().ToList();
            var usages = await usageRepo.FindAsync(u => usageBookingItemIds.Contains(u.BookingItemId));
            var usageMap = usages.ToDictionary(u => (u.BookingItemId, u.MaterialId), u => u);

            // materials names
            var materialIds = reqLines.Select(l => l.MaterialId).Distinct().ToList();
            var mats = await materialRepo.FindAsync(m => materialIds.Contains(m.Id));
            var matMap = mats.ToDictionary(m => m.Id, m => m.Name);

            return pendingRequests
                .OrderByDescending(r => r.RequestedAt)
                .Select(r =>
                {
                    bookingItemMap.TryGetValue(r.BookingItemId, out var bi);
                    var booking = (bi != null && bookingMap.TryGetValue(bi.BookingId, out var bk)) ? bk : null;

                    linesByReq.TryGetValue(r.Id, out var myLines);
                    myLines ??= new List<BookingItemMaterialChangeRequestLine>();

                    var lineDtos = myLines.Select(l =>
                    {
                        usageMap.TryGetValue((r.BookingItemId, l.MaterialId), out var u);
                        matMap.TryGetValue(l.MaterialId, out var matName);

                        return new PendingMaterialRequestLineDto
                        {
                            MaterialId = l.MaterialId,
                            MaterialName = matName ?? "",
                            DefaultQty = u?.DefaultQty ?? 0m,
                            CurrentActualQty = u?.ActualQty ?? 0m,
                            ProposedActualQty = l.ProposedActualQty
                        };
                    }).ToList();

                    empMap.TryGetValue(r.RequestedByEmployeeId, out var empName);

                    return new PendingMaterialRequestDto
                    {
                        RequestId = r.Id,
                        BookingItemId = r.BookingItemId,
                        BookingId = booking?.Id ?? 0,
                        ScheduledStart = booking?.ScheduledStart ?? DateTime.MinValue,
                        RequestedByEmployeeId = r.RequestedByEmployeeId,
                        RequestedByEmployeeName = empName ?? "",
                        RequestedAt = r.RequestedAt,
                        Lines = lineDtos
                    };
                })
                .ToList();
        }


    }

}
