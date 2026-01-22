using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Dashboard;
using Forto.Domain.Entities.Billings;
using Forto.Domain.Entities.Ops;

namespace Forto.Application.Abstractions.Services.Dashboard
{

    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;

        public DashboardService(IUnitOfWork uow) => _uow = uow;

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

            // -------- Net Profit --------
            // NetProfit = PaidRevenue
            //          - ConsumeCost - WasteCost - ProductSellCost - GiftCost
            //          + AdjustNet(materials+products)
            var totalCosts = materialsConsumeCost + materialsWasteCost + productsSoldCost + giftsCost;
            var netProfit = paidRevenue - totalCosts + (materialsAdjustNet + productsAdjustNet);

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
                NetProfit = netProfit
            };
        }
            
    }

}
