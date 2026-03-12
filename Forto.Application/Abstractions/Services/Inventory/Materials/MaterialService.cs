using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Inventory.Materials;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;

namespace Forto.Application.Abstractions.Services.Inventory.Materials
{
    public class MaterialService : IMaterialService
    {
        private readonly IUnitOfWork _uow;
        public MaterialService(IUnitOfWork uow) => _uow = uow;

        public async Task<MaterialResponse> CreateAsync(CreateMaterialRequest request)
        {
            if (request.InitialStockQty.HasValue && !request.BranchId.HasValue)
                throw new BusinessException("BranchId is required when InitialStockQty is provided", 400);

            var repo = _uow.Repository<Domain.Entities.Inventory.Material>();

            var name = request.Name.Trim();
            var exists = await repo.AnyAsync(m => m.Name == name);
            if (exists)
                throw new BusinessException("Material name already exists", 409,
                    new Dictionary<string, string[]> { ["name"] = new[] { "Duplicate material name." } });

            var m = new Domain.Entities.Inventory.Material
            {
                Name = name,
                Unit = request.Unit,
                CostPerUnit = request.CostPerUnit,
                ChargePerUnit = request.ChargePerUnit,
                IsActive = true
            };

            await repo.AddAsync(m);
            await _uow.SaveChangesAsync();

            // لو أُدخل مخزون ابتدائي → تسجيل حركة Stock In في الفرع
            if (request.InitialStockQty.HasValue && request.InitialStockQty.Value > 0)
            {
                var branchId = request.BranchId!.Value;
                var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
                if (branch == null || !branch.IsActive)
                    throw new BusinessException("Branch not found or inactive", 404);

                var stockRepo = _uow.Repository<BranchMaterialStock>();
                var moveRepo = _uow.Repository<MaterialMovement>();

                var stock = (await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && s.MaterialId == m.Id))
                    .FirstOrDefault();

                var isNewStock = stock == null;
                if (stock == null)
                {
                    stock = new BranchMaterialStock
                    {
                        BranchId = branchId,
                        MaterialId = m.Id,
                        OnHandQty = 0,
                        TotalCostOfStock = 0,
                        ReservedQty = 0,
                        ReorderLevel = request.ReorderLevel ?? 0
                    };
                    await stockRepo.AddAsync(stock);
                }
                else
                {
                    stock.ReorderLevel = request.ReorderLevel ?? stock.ReorderLevel;
                    stockRepo.Update(stock);
                }

                var qty = request.InitialStockQty.Value;
                var unitCost = m.CostPerUnit;
                stock.TotalCostOfStock += qty * unitCost;
                stock.OnHandQty += qty;
                if (!isNewStock)
                    stockRepo.Update(stock);

                await moveRepo.AddAsync(new MaterialMovement
                {
                    BranchId = branchId,
                    MaterialId = m.Id,
                    MovementType = MaterialMovementType.In,
                    Qty = qty,
                    UnitCostSnapshot = unitCost,
                    TotalCost = qty * unitCost,
                    OccurredAt = DateTime.UtcNow,
                    Notes = "Initial stock on material creation"
                });

                await _uow.SaveChangesAsync();
            }

            return Map(m);
        }

        public async Task<IReadOnlyList<MaterialResponse>> GetAllAsync()
        {
            var list = await _uow.Repository<Domain.Entities.Inventory.Material>().FindAsync(m => !m.IsDeleted);
            return list.Select(Map).ToList();
        }

        public async Task<MaterialResponse?> GetByIdAsync(int id)
        {
            var m = await _uow.Repository<Domain.Entities.Inventory.Material>().GetByIdAsync(id);
            return m == null ? null : Map(m);
        }

        public async Task<MaterialResponse?> UpdateAsync(int id, UpdateMaterialRequest request)
        {
            var repo = _uow.Repository<Domain.Entities.Inventory.Material>();
            var m = await repo.GetByIdAsync(id);
            if (m == null) return null;

            var name = request.Name.Trim();
            var exists = await repo.AnyAsync(x => x.Id != id && x.Name == name);
            if (exists)
                throw new BusinessException("Material name already exists", 409);

            m.Name = name;
            m.Unit = request.Unit;
            m.CostPerUnit = request.CostPerUnit;
            m.ChargePerUnit = request.ChargePerUnit;
            m.IsActive = request.IsActive;

            repo.Update(m);
            await _uow.SaveChangesAsync();

            return Map(m);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Domain.Entities.Inventory.Material>();
            var m = await repo.GetByIdAsync(id);
            if (m == null) return false;

            repo.Delete(m); // soft delete
            await _uow.SaveChangesAsync();
            return true;
        }

        private static MaterialResponse Map(Domain.Entities.Inventory.Material m) => new()
        {
            Id = m.Id,
            Name = m.Name,
            Unit = m.Unit,
            CostPerUnit = m.CostPerUnit,
            ChargePerUnit = m.ChargePerUnit,
            IsActive = m.IsActive
        };
    }

}
