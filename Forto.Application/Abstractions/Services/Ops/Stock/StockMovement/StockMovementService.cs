using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Ops.Stock;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;

namespace Forto.Application.Abstractions.Services.Ops.Stock.StockMovement
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IUnitOfWork _uow;
        public StockMovementService(IUnitOfWork uow) => _uow = uow;

        private static bool IsCashierRole(EmployeeRole role)
            => role == EmployeeRole.Cashier || role == EmployeeRole.Supervisor || role == EmployeeRole.Admin;

        private async Task RequireCashierAsync(int cashierId)
        {
            var emp = await _uow.Repository<Employee>().GetByIdAsync(cashierId);
            if (emp == null || !emp.IsActive) throw new BusinessException("Cashier not found", 404);
            if (!IsCashierRole(emp.Role)) throw new BusinessException("Not allowed", 403);
        }

        public async Task StockInAsync(int branchId, StockInRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive) throw new BusinessException("Branch not found", 404);

            var material = await _uow.Repository<Material>().GetByIdAsync(request.MaterialId);
            if (material == null || !material.IsActive) throw new BusinessException("Material not found", 404);

            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var movementRepo = _uow.Repository<MaterialMovement>();

            var stock = (await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && s.MaterialId == request.MaterialId))
                .FirstOrDefault();

            if (stock == null)
            {
                stock = new BranchMaterialStock
                {
                    BranchId = branchId,
                    MaterialId = request.MaterialId,
                    OnHandQty = 0,
                    ReservedQty = 0,
                    ReorderLevel = 0
                };
                await stockRepo.AddAsync(stock);
            }

            stock.OnHandQty += request.Qty;
            stockRepo.Update(stock);

            var occurred = request.OccurredAt ?? DateTime.UtcNow;

            await movementRepo.AddAsync(new MaterialMovement
            {
                BranchId = branchId,
                MaterialId = request.MaterialId,
                MovementType = MaterialMovementType.In,
                Qty = request.Qty,
                UnitCostSnapshot = request.UnitCost,
                TotalCost = request.Qty * request.UnitCost,
                OccurredAt = occurred,
                RecordedByEmployeeId = request.CashierId,
                Notes = request.Notes
            });

            await _uow.SaveChangesAsync();
        }

        public async Task StockAdjustAsync(int branchId, StockAdjustRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive) throw new BusinessException("Branch not found", 404);

            var materialRepo = _uow.Repository<Material>();
            var material = await materialRepo.GetByIdAsync(request.MaterialId);
            if (material == null || !material.IsActive) throw new BusinessException("Material not found", 404);

            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var movementRepo = _uow.Repository<MaterialMovement>();

            var stock = (await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && s.MaterialId == request.MaterialId))
                .FirstOrDefault();

            if (stock == null)
            {
                stock = new BranchMaterialStock
                {
                    BranchId = branchId,
                    MaterialId = request.MaterialId,
                    OnHandQty = 0,
                    ReservedQty = 0,
                    ReorderLevel = 0
                };
                await stockRepo.AddAsync(stock);
            }

            // مهم: ماينفعش physical أقل من reserved (لأن reserved شغل شغال)
            if (request.PhysicalOnHandQty < stock.ReservedQty)
                throw new BusinessException("PhysicalOnHandQty cannot be less than ReservedQty", 409);

            var diff = request.PhysicalOnHandQty - stock.OnHandQty; // ممكن + أو -
            stock.OnHandQty = request.PhysicalOnHandQty;
            stockRepo.Update(stock);

            var occurred = request.OccurredAt ?? DateTime.UtcNow;

            await movementRepo.AddAsync(new MaterialMovement
            {
                BranchId = branchId,
                MaterialId = request.MaterialId,
                MovementType = MaterialMovementType.Adjust,
                Qty = diff,
                UnitCostSnapshot = material.CostPerUnit, // snapshot بسيط (ممكن later avg cost)
                TotalCost = diff * material.CostPerUnit,
                OccurredAt = occurred,
                RecordedByEmployeeId = request.CashierId,
                Notes = request.Notes
            });

            await _uow.SaveChangesAsync();
        }
    }

}
