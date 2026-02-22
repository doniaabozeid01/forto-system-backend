using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Dashboard;
using Forto.Domain.Entities.Billings;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;

namespace Forto.Application.Abstractions.Services.Dashboard
{

    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;

        public DashboardService(IUnitOfWork uow) => _uow = uow;

        //public async Task<DashboardSummaryResponse> GetSummaryAsync(int branchId, DateOnly from, DateOnly to)
        //{
        //    if (to < from)
        //        throw new BusinessException("Invalid date range", 400);

        //    // DateOnly range: [from, to] inclusive -> convert to [fromStart, toEndExclusive)
        //    var fromStart = from.ToDateTime(TimeOnly.MinValue);
        //    var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        //    // validate branch
        //    var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
        //    if (branch == null || !branch.IsActive)
        //        throw new BusinessException("Branch not found", 404);

        //    // -------- Revenue (Paid invoices) --------
        //    var invoiceRepo = _uow.Repository<Invoice>();

        //    var paidInvoices = await invoiceRepo.FindAsync(i =>
        //        i.BranchId == branchId &&
        //        i.Status == Domain.Enum.InvoiceStatus.Paid &&
        //        i.PaidAt != null &&
        //        i.PaidAt >= fromStart &&
        //        i.PaidAt < toEndExclusive);

        //    var paidRevenue = paidInvoices.Sum(i => i.Total);

        //    // -------- Materials costs (Movements) --------
        //    var matMoveRepo = _uow.Repository<MaterialMovement>();
        //    var matMoves = await matMoveRepo.FindAsync(m =>
        //        m.BranchId == branchId &&
        //        m.OccurredAt >= fromStart &&
        //        m.OccurredAt < toEndExclusive);

        //    var materialsConsumeCost = matMoves
        //        .Where(m => m.MovementType == Domain.Enum.MaterialMovementType.Consume)
        //        .Sum(m => m.TotalCost);

        //    var materialsWasteCost = matMoves
        //        .Where(m => m.MovementType == Domain.Enum.MaterialMovementType.Waste)
        //        .Sum(m => m.TotalCost);

        //    var materialsAdjustNet = matMoves
        //        .Where(m => m.MovementType == Domain.Enum.MaterialMovementType.Adjust)
        //        .Sum(m => m.TotalCost); // can be +/-

        //    // -------- Products costs (Movements) --------
        //    var prodMoveRepo = _uow.Repository<ProductMovement>();
        //    var prodMoves = await prodMoveRepo.FindAsync(m =>
        //        m.BranchId == branchId &&
        //        m.OccurredAt >= fromStart &&
        //        m.OccurredAt < toEndExclusive);

        //    var productsSoldCost = prodMoves
        //        .Where(m => m.MovementType == Domain.Enum.ProductMovementType.Sell)
        //        .Sum(m => m.TotalCost);

        //    var giftsCost = prodMoves
        //        .Where(m => m.MovementType == Domain.Enum.ProductMovementType.Gift)
        //        .Sum(m => m.TotalCost);

        //    var productsAdjustNet = prodMoves
        //        .Where(m => m.MovementType == Domain.Enum.ProductMovementType.Adjust)
        //        .Sum(m => m.TotalCost); // can be +/-

        //    // -------- Net Profit --------
        //    // NetProfit = PaidRevenue
        //    //          - ConsumeCost - WasteCost - ProductSellCost - GiftCost
        //    //          + AdjustNet(materials+products)
        //    var totalCosts = materialsConsumeCost + materialsWasteCost + productsSoldCost + giftsCost;
        //    var netProfit = paidRevenue - totalCosts + (materialsAdjustNet + productsAdjustNet);

        //    return new DashboardSummaryResponse
        //    {
        //        BranchId = branchId,
        //        From = from,
        //        To = to,

        //        PaidRevenue = paidRevenue,

        //        MaterialsConsumeCost = materialsConsumeCost,
        //        MaterialsWasteCost = materialsWasteCost,
        //        MaterialsAdjustNet = materialsAdjustNet,

        //        ProductsSoldCost = productsSoldCost,
        //        GiftsCost = giftsCost,
        //        ProductsAdjustNet = productsAdjustNet,

        //        TotalCosts = totalCosts,
        //        NetProfit = netProfit
        //    };
        //}








        public async Task<DashboardSummaryResponse> GetSummaryAsync(int branchId, DateOnly from, DateOnly to)
        {
            if (to < from)
                throw new BusinessException("Invalid date range", 400);

            // DateOnly range: [from, to] inclusive -> convert to [fromStart, toEndExclusive)
            var fromStart = from.ToDateTime(TimeOnly.MinValue);
            var toEndExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

            // validate branch
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            // -------- Revenue (Paid invoices) --------
            var invoiceRepo = _uow.Repository<Invoice>();

            var paidInvoices = await invoiceRepo.FindAsync(i =>
                i.BranchId == branchId &&
                i.Status == Domain.Enum.InvoiceStatus.Paid &&
                i.PaidAt != null &&
                i.PaidAt >= fromStart &&
                i.PaidAt < toEndExclusive);

            var paidRevenue = paidInvoices.Sum(i => i.Total);

            // -------- Materials costs (Movements) --------
            var matMoveRepo = _uow.Repository<MaterialMovement>();
            var matMoves = await matMoveRepo.FindAsync(m =>
                m.BranchId == branchId &&
                m.OccurredAt >= fromStart &&
                m.OccurredAt < toEndExclusive);

            var materialsConsumeCost = matMoves
                .Where(m => m.MovementType == Domain.Enum.MaterialMovementType.Consume)
                .Sum(m => m.TotalCost);

            var materialsWasteCost = matMoves
                .Where(m => m.MovementType == Domain.Enum.MaterialMovementType.Waste)
                .Sum(m => m.TotalCost);

            var materialsAdjustNet = matMoves
                .Where(m => m.MovementType == Domain.Enum.MaterialMovementType.Adjust)
                .Sum(m => m.TotalCost); // can be +/-

            // -------- Products costs (Movements) --------
            var prodMoveRepo = _uow.Repository<ProductMovement>();
            var prodMoves = await prodMoveRepo.FindAsync(m =>
                m.BranchId == branchId &&
                m.OccurredAt >= fromStart &&
                m.OccurredAt < toEndExclusive);

            var productsSoldCost = prodMoves
                .Where(m => m.MovementType == Domain.Enum.ProductMovementType.Sell)
                .Sum(m => m.TotalCost);

            var giftsCost = prodMoves
                .Where(m => m.MovementType == Domain.Enum.ProductMovementType.Gift)
                .Sum(m => m.TotalCost);

            var productsAdjustNet = prodMoves
                .Where(m => m.MovementType == Domain.Enum.ProductMovementType.Adjust)
                .Sum(m => m.TotalCost); // can be +/-

            // -------- Profit structure --------
            // OperatingProfit = ربح التشغيل (الإيراد − التكاليف فقط، بدون فروقات الجرد)
            // InventoryVariance = فروقات الجرد (معروضة منفصلة)
            // FinalAccountingNet = ربح التشغيل + فروقات الجرد (اختياري)
            var totalCosts = materialsConsumeCost + materialsWasteCost + productsSoldCost + giftsCost;
            var operatingProfit = paidRevenue - totalCosts;
            var inventoryVariance = materialsAdjustNet + productsAdjustNet;
            var finalAccountingNet = operatingProfit + inventoryVariance;

            return new DashboardSummaryResponse
            {
                BranchId = branchId,
                From = from,
                To = to,

                PaidRevenue = paidRevenue,

                MaterialsConsumeCost = materialsConsumeCost,
                MaterialsWasteCost = materialsWasteCost,
                MaterialsAdjustNet = materialsAdjustNet,

                ProductsSoldCost = productsSoldCost,
                GiftsCost = giftsCost,
                ProductsAdjustNet = productsAdjustNet,

                TotalCosts = totalCosts,

                OperatingProfit = operatingProfit,
                InventoryVariance = inventoryVariance,
                FinalAccountingNet = finalAccountingNet,
                NetProfit = operatingProfit
            };
        }















        public async Task<AnalyticsResponse> GetTopServicesAsync(int branchId, DateOnly from, DateOnly to)
        {
            if (to < from) throw new BusinessException("Invalid date range", 400);

            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive) throw new BusinessException("Branch not found", 404);

            var fromStart = from.ToDateTime(TimeOnly.MinValue);
            var toEnd = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();
            var serviceRepo = _uow.Repository<Service>();

            // bookings in branch (filter by scheduled start)
            var bookings = await bookingRepo.FindAsync(b =>
                b.BranchId == branchId &&
                b.ScheduledStart >= fromStart &&
                b.ScheduledStart < toEnd);

            if (bookings.Count == 0)
                return new AnalyticsResponse { BranchId = branchId, From = from, To = to, TotalDoneItems = 0 };

            var bookingIds = bookings.Select(b => b.Id).ToList();

            // done items only
            var doneItems = await itemRepo.FindAsync(i =>
                bookingIds.Contains(i.BookingId) &&
                i.Status == BookingItemStatus.Done);

            var total = doneItems.Count;
            if (total == 0)
                return new AnalyticsResponse { BranchId = branchId, From = from, To = to, TotalDoneItems = 0 };

            var grouped = doneItems
                .GroupBy(i => i.ServiceId)
                .Select(g => new { ServiceId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var serviceIds = grouped.Select(x => x.ServiceId).ToList();
            var services = await serviceRepo.FindAsync(s => serviceIds.Contains(s.Id));
            var svcMap = services.ToDictionary(s => s.Id, s => s.Name);

            var items = grouped.Select(x =>
            {
                var name = svcMap.TryGetValue(x.ServiceId, out var n) ? n : "";
                var percent = Math.Round((decimal)x.Count * 100m / total, 2);

                return new AnalyticsSliceDto
                {
                    Id = x.ServiceId,
                    Name = name,
                    Count = x.Count,
                    Percent = percent
                };
            }).ToList();

            return new AnalyticsResponse
            {
                BranchId = branchId,
                From = from,
                To = to,
                TotalDoneItems = total,
                Items = items
            };
        }












        public async Task<AnalyticsResponse> GetTopEmployeesAsync(int branchId, DateOnly from, DateOnly to)
        {
            if (to < from) throw new BusinessException("Invalid date range", 400);

            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive) throw new BusinessException("Branch not found", 404);

            var fromStart = from.ToDateTime(TimeOnly.MinValue);
            var toEnd = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var bookingRepo = _uow.Repository<Booking>();
            var itemRepo = _uow.Repository<BookingItem>();
            var empRepo = _uow.Repository<Employee>();

            var bookings = await bookingRepo.FindAsync(b =>
                b.BranchId == branchId &&
                b.ScheduledStart >= fromStart &&
                b.ScheduledStart < toEnd);

            if (bookings.Count == 0)
                return new AnalyticsResponse { BranchId = branchId, From = from, To = to, TotalDoneItems = 0 };

            var bookingIds = bookings.Select(b => b.Id).ToList();

            // done items with assigned employee
            var doneItems = await itemRepo.FindAsync(i =>
                bookingIds.Contains(i.BookingId) &&
                i.Status == BookingItemStatus.Done &&
                i.AssignedEmployeeId != null);

            var total = doneItems.Count;
            if (total == 0)
                return new AnalyticsResponse { BranchId = branchId, From = from, To = to, TotalDoneItems = 0 };

            var grouped = doneItems
                .GroupBy(i => i.AssignedEmployeeId!.Value)
                .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var empIds = grouped.Select(x => x.EmployeeId).ToList();
            var emps = await empRepo.FindAsync(e => empIds.Contains(e.Id));
            var empMap = emps.ToDictionary(e => e.Id, e => e.Name);

            var items = grouped.Select(x =>
            {
                var name = empMap.TryGetValue(x.EmployeeId, out var n) ? n : "";
                var percent = Math.Round((decimal)x.Count * 100m / total, 2);

                return new AnalyticsSliceDto
                {
                    Id = x.EmployeeId,
                    Name = name,
                    Count = x.Count,
                    Percent = percent
                };
            }).ToList();

            return new AnalyticsResponse
            {
                BranchId = branchId,
                From = from,
                To = to,
                TotalDoneItems = total,
                Items = items
            };
        }

        public async Task<TopEmployeesWithServicesResponse> GetTopEmployeesWithServicesAsync(int branchId, DateOnly from, DateOnly to, int? role = null)
        {
            if (to < from) throw new BusinessException("Invalid date range", 400);

            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive) throw new BusinessException("Branch not found", 404);

            var fromStart = from.ToDateTime(TimeOnly.MinValue);
            var toEnd = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var empRepo = _uow.Repository<Employee>();
            var invRepo = _uow.Repository<Invoice>();

            var empQuery = await empRepo.FindAsync(e => e.IsActive);
            var allEmps = role.HasValue && Enum.IsDefined(typeof(EmployeeRole), role.Value)
                ? empQuery.Where(e => (int)e.Role == role.Value).ToList()
                : empQuery.ToList();

            // الواقع: فواتير مدفوعة في الفترة في الفرع — ككاشير وكمشرف
            var invoices = await invRepo.FindAsync(inv =>
                inv.BranchId == branchId &&
                inv.PaidAt != null &&
                inv.PaidAt >= fromStart &&
                inv.PaidAt < toEnd);

            var byCashier = invoices
                .Where(inv => inv.PaidByEmployeeId != null)
                .GroupBy(inv => inv.PaidByEmployeeId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
            var bySupervisor = invoices
                .Where(inv => inv.SupervisorId != null)
                .GroupBy(inv => inv.SupervisorId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
            var totalInvoicesAsCashier = byCashier.Values.Sum();
            var totalInvoicesAsSupervisor = bySupervisor.Values.Sum();

            int total = 0;
            var byEmployee = new Dictionary<int, List<BookingItem>>();
            var serviceMap = new Dictionary<int, string>();


                var bookingRepo = _uow.Repository<Booking>();
                var itemRepo = _uow.Repository<BookingItem>();
                var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();

                // حجوزات مكتملة فقط (Complete) — مش أي حجز في الفترة
                var bookings = await bookingRepo.FindAsync(b =>
                    b.BranchId == branchId &&
                    b.Status == BookingStatus.Completed &&
                    b.ScheduledStart >= fromStart &&
                    b.ScheduledStart < toEnd);
                var bookingIds = bookings.Select(b => b.Id).ToList();
                var doneItems = bookingIds.Count == 0
                    ? new List<BookingItem>()
                    : await itemRepo.FindAsync(i =>
                        bookingIds.Contains(i.BookingId) &&
                        i.Status == BookingItemStatus.Done &&
                        i.AssignedEmployeeId != null);
                total = doneItems.Count;
                byEmployee = doneItems
                    .GroupBy(i => i.AssignedEmployeeId!.Value)
                    .ToDictionary(g => g.Key, g => g.ToList());
                var serviceIds = doneItems.Select(i => i.ServiceId).Distinct().ToList();
                var services = serviceIds.Count == 0 ? new List<Domain.Entities.Catalog.Service>() : await serviceRepo.FindAsync(s => serviceIds.Contains(s.Id));
                serviceMap = services.ToDictionary(s => s.Id, s => s.Name);
            

            var items = new List<EmployeeWithServicesDto>();
            foreach (var emp in allEmps.OrderBy(e => e.Name))
            {
                var employeeId = emp.Id;
                var empName = emp.Name;
                var empItems = byEmployee.TryGetValue(employeeId, out var list) ? list : new List<BookingItem>();
                var count = empItems.Count;
                var percent = total > 0 ? Math.Round((decimal)count * 100m / total, 2) : 0m;

                var byService = empItems
                    .GroupBy(i => i.ServiceId)
                    .Select(g => new ServiceCountDto
                    {
                        ServiceId = g.Key,
                        ServiceName = serviceMap.TryGetValue(g.Key, out var sn) ? sn : "",
                        Count = g.Count()
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList();

                var invoicesAsCashier = byCashier.TryGetValue(employeeId, out var c) ? c : 0;
                var invoicesAsSupervisor = bySupervisor.TryGetValue(employeeId, out var s) ? s : 0;

                items.Add(new EmployeeWithServicesDto
                {
                    EmployeeId = employeeId,
                    EmployeeName = empName,
                    Role = emp.Role,
                    Count = count,
                    Percent = percent,
                    Services = byService,
                    InvoicesAsCashierCount = invoicesAsCashier,
                    InvoicesAsSupervisorCount = invoicesAsSupervisor
                });
            }


                items = items.OrderByDescending(x => x.Count).ThenByDescending(x => x.InvoicesAsCashierCount).ThenBy(x => x.EmployeeName).ToList();

            return new TopEmployeesWithServicesResponse
            {
                BranchId = branchId,
                From = from,
                To = to,
                RoleFilter = role,
                TotalDoneItems = total,
                //TotalInvoicesAsCashier = totalInvoicesAsCashier,
                //TotalInvoicesAsSupervisor = totalInvoicesAsSupervisor,
                Items = items
            };
        }
    }
}
