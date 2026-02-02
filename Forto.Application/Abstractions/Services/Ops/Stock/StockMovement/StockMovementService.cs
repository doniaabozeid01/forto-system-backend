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



        //public async Task StockInAsync(int branchId, StockInRequest request)
        //{
        //    await RequireCashierAsync(request.CashierId);

        //    var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
        //    if (branch == null || !branch.IsActive) throw new BusinessException("Branch not found", 404);

        //    var material = await _uow.Repository<Material>().GetByIdAsync(request.MaterialId);
        //    if (material == null || !material.IsActive) throw new BusinessException("Material not found", 404);

        //    var stockRepo = _uow.Repository<BranchMaterialStock>();
        //    var movementRepo = _uow.Repository<MaterialMovement>();

        //    var stock = (await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && s.MaterialId == request.MaterialId))
        //        .FirstOrDefault();

        //    if (stock == null)
        //    {
        //        stock = new BranchMaterialStock
        //        {
        //            BranchId = branchId,
        //            MaterialId = request.MaterialId,
        //            OnHandQty = 0,
        //            ReservedQty = 0,
        //            ReorderLevel = 0
        //        };
        //        await stockRepo.AddAsync(stock);
        //    }

        //    stock.OnHandQty += request.Qty;
        //    stockRepo.Update(stock);

        //    var occurred = request.OccurredAt ?? DateTime.UtcNow;

        //    await movementRepo.AddAsync(new MaterialMovement
        //    {
        //        BranchId = branchId,
        //        MaterialId = request.MaterialId,
        //        MovementType = MaterialMovementType.In,
        //        Qty = request.Qty,
        //        UnitCostSnapshot = request.UnitCost,
        //        TotalCost = request.Qty * request.UnitCost,
        //        OccurredAt = occurred,
        //        RecordedByEmployeeId = request.CashierId,
        //        Notes = request.Notes
        //    });

        //    await _uow.SaveChangesAsync();
        //}



        public async Task StockInAsync(int branchId, StockInRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            var materialRepo = _uow.Repository<Material>();
            var material = await materialRepo.GetByIdAsync(request.MaterialId);
            if (material == null || !material.IsActive)
                throw new BusinessException("Material not found", 404);

            // ✅ choose unitCost:
            // - if request.UnitCost sent => use it AND update material.UnitCost
            // - else fallback to material.UnitCost
            var unitCost = request.UnitCost ?? material.CostPerUnit;
            // لو عندك اسم الحقل في material "UnitCost" بدل CostPerUnit غيّري السطر ده

            // (اختياري) لو عايزة تمنعي unitCost=0 بالغلط:
            // if (unitCost < 0) throw new BusinessException("Invalid unitCost", 400);

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

            // update stock
            stock.OnHandQty += request.Qty;
            stockRepo.Update(stock);

            var occurred = request.OccurredAt ?? DateTime.UtcNow;

            // record movement IN
            await movementRepo.AddAsync(new MaterialMovement
            {
                BranchId = branchId,
                MaterialId = request.MaterialId,
                MovementType = MaterialMovementType.In,
                Qty = request.Qty,
                UnitCostSnapshot = unitCost,
                TotalCost = request.Qty * unitCost,
                OccurredAt = occurred,
                //RecordedByEmployeeId = request.CashierId,
                Notes = request.Notes
            });

            // ✅ if unitCost was explicitly sent, update material current cost
            if (request.UnitCost.HasValue)
            {
                // هنا مهم: حدثّي التكلفة الحالية علشان الشغل اللي بعد كده ياخد snapshot جديد
                material.CostPerUnit = request.UnitCost.Value;
                materialRepo.Update(material);
            }

            await _uow.SaveChangesAsync();
        }



        public async Task StockAdjustAsync(int branchId, StockAdjustRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            var materialRepo = _uow.Repository<Material>();
            var material = await materialRepo.GetByIdAsync(request.MaterialId);
            if (material == null || !material.IsActive)
                throw new BusinessException("Material not found", 404);

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

            var occurred = request.OccurredAt ?? DateTime.UtcNow;

            // =========================
            // Monthly average cost from IN movements (same month)
            // =========================
            var monthStart = new DateTime(occurred.Year, occurred.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var nextMonthStart = monthStart.AddMonths(1);

            var inMoves = await movementRepo.FindAsync(m =>
                m.BranchId == branchId &&
                m.MaterialId == request.MaterialId &&
                m.MovementType == MaterialMovementType.In &&
                m.OccurredAt >= monthStart &&
                m.OccurredAt < nextMonthStart);

            var totalInQty = inMoves.Sum(m => m.Qty);
            var totalInCost = inMoves.Sum(m => m.TotalCost);

            decimal unitCostForAdjust;
            if (totalInQty > 0)
                unitCostForAdjust = totalInCost / totalInQty;     // ✅ monthly avg
            else
                unitCostForAdjust = material.CostPerUnit;          // fallback

            // =========================
            // Apply adjust on stock
            // =========================
            var diff = request.PhysicalOnHandQty - stock.OnHandQty; // ممكن + أو -
            stock.OnHandQty = request.PhysicalOnHandQty;
            stockRepo.Update(stock);

            await movementRepo.AddAsync(new MaterialMovement
            {
                BranchId = branchId,
                MaterialId = request.MaterialId,
                MovementType = MaterialMovementType.Adjust,
                Qty = diff,
                UnitCostSnapshot = unitCostForAdjust,
                TotalCost = diff * unitCostForAdjust, // سالب لو diff سالب
                OccurredAt = occurred,
                //RecordedByEmployeeId = request.CashierId,
                Notes = request.Notes
            });

            await _uow.SaveChangesAsync();
        }





    }

}
