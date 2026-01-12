using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Ops.Stock;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;

namespace Forto.Application.Abstractions.Services.Ops.Stock
{
    public class BranchStockService : IBranchStockService
    {
        private readonly IUnitOfWork _uow;
        public BranchStockService(IUnitOfWork uow) => _uow = uow;

        public async Task<BranchStockItemResponse> UpsertAsync(int branchId, UpsertBranchStockRequest request)
        {
            // validate branch exists
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null)
                throw new BusinessException("Branch not found", 404);

            var material = await _uow.Repository<Material>().GetByIdAsync(request.MaterialId);
            if (material == null)
                throw new BusinessException("Material not found", 404);

            var stockRepo = _uow.Repository<BranchMaterialStock>();

            var existing = (await stockRepo.FindAsync(x => x.BranchId == branchId && x.MaterialId == request.MaterialId))
                .FirstOrDefault();

            if (existing == null)
            {
                existing = new BranchMaterialStock
                {
                    BranchId = branchId,
                    MaterialId = request.MaterialId,
                    OnHandQty = request.OnHandQty,
                    ReservedQty = 0,
                    ReorderLevel = request.ReorderLevel
                };
                await stockRepo.AddAsync(existing);
            }
            else
            {
                // IMPORTANT: onhand cannot go below reserved
                if (request.OnHandQty < existing.ReservedQty)
                    throw new BusinessException("OnHandQty cannot be less than ReservedQty", 409,
                        new Dictionary<string, string[]>
                        {
                            ["onHandQty"] = new[] { $"ReservedQty={existing.ReservedQty}." }
                        });

                existing.OnHandQty = request.OnHandQty;
                existing.ReorderLevel = request.ReorderLevel;
                stockRepo.Update(existing);
            }

            await _uow.SaveChangesAsync();

            return new BranchStockItemResponse
            {
                BranchId = branchId,
                MaterialId = material.Id,
                MaterialName = material.Name,
                Unit = material.Unit.ToString(),
                OnHandQty = existing.OnHandQty,
                ReservedQty = existing.ReservedQty,
                AvailableQty = existing.OnHandQty - existing.ReservedQty,
                ReorderLevel = existing.ReorderLevel
            };
        }

        public async Task<IReadOnlyList<BranchStockItemResponse>> GetBranchStockAsync(int branchId)
        {
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null)
                throw new BusinessException("Branch not found", 404);

            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var materialRepo = _uow.Repository<Material>();

            var stocks = await stockRepo.FindAsync(s => s.BranchId == branchId);
            if (stocks.Count == 0) return new List<BranchStockItemResponse>();

            var materialIds = stocks.Select(s => s.MaterialId).Distinct().ToList();
            var materials = await materialRepo.FindAsync(m => materialIds.Contains(m.Id));
            var map = materials.ToDictionary(m => m.Id, m => m);

            return stocks.Select(s =>
            {
                map.TryGetValue(s.MaterialId, out var mat);

                return new BranchStockItemResponse
                {
                    BranchId = s.BranchId,
                    MaterialId = s.MaterialId,
                    MaterialName = mat?.Name ?? "",
                    Unit = mat?.Unit.ToString() ?? "",
                    OnHandQty = s.OnHandQty,
                    ReservedQty = s.ReservedQty,
                    AvailableQty = s.OnHandQty - s.ReservedQty,
                    ReorderLevel = s.ReorderLevel
                };
            }).ToList();
        }
    }
}
