using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Catalog.Recipes;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Enum;

namespace Forto.Application.Abstractions.Services.Catalogs.Recipes
{
    public class ServiceRecipeService : IServiceRecipeService
    {
        private readonly IUnitOfWork _uow;

        public ServiceRecipeService(IUnitOfWork uow) => _uow = uow;

        public async Task<ServiceRecipeResponse> GetAsync(int serviceId, CarBodyType bodyType)
        {
            var service = await _uow.Repository<Domain.Entities.Catalog.Service>().GetByIdAsync(serviceId);
            if (service == null)
                throw new BusinessException("Service not found", 404);

            var recipeRepo = _uow.Repository<ServiceMaterialRecipe>();
            var materialRepo = _uow.Repository<Material>();

            var rows = await recipeRepo.FindAsync(r => r.ServiceId == serviceId && r.BodyType == bodyType && r.IsActive);
            if (rows.Count == 0)
            {
                return new ServiceRecipeResponse
                {
                    ServiceId = serviceId,
                    BodyType = bodyType,
                    Materials = new()
                };
            }

            var materialIds = rows.Select(r => r.MaterialId).Distinct().ToList();
            var mats = await materialRepo.FindAsync(m => materialIds.Contains(m.Id));
            var map = mats.ToDictionary(m => m.Id, m => m);

            return new ServiceRecipeResponse
            {
                ServiceId = serviceId,
                BodyType = bodyType,
                Materials = rows.Select(r =>
                {
                    map.TryGetValue(r.MaterialId, out var m);
                    return new ServiceRecipeMaterialResponse
                    {
                        MaterialId = r.MaterialId,
                        MaterialName = m?.Name ?? "",
                        Unit = m?.Unit.ToString() ?? "",
                        DefaultQty = r.DefaultQty
                    };
                }).ToList()
            };
        }

        public async Task<ServiceRecipeResponse> UpsertAsync(int serviceId, CarBodyType bodyType, UpsertServiceRecipeRequest request)
        {
            var serviceRepo = _uow.Repository<Domain.Entities.Catalog.Service>();
            var recipeRepo = _uow.Repository<ServiceMaterialRecipe>();
            var materialRepo = _uow.Repository<Material>();

            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null)
                throw new BusinessException("Service not found", 404);

            // منع تكرار materialId في نفس الطلب
            var dup = request.Materials.GroupBy(x => x.MaterialId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (dup.Any())
                throw new BusinessException("Duplicate materials in recipe request", 400,
                    new Dictionary<string, string[]>
                    {
                        ["materials"] = dup.Select(x => $"MaterialId {x} duplicated").ToArray()
                    });

            // validate material ids exist + active
            var materialIds = request.Materials.Select(x => x.MaterialId).Distinct().ToList();
            var mats = materialIds.Count == 0 ? new List<Material>() : (await materialRepo.FindAsync(m => materialIds.Contains(m.Id) && m.IsActive)).ToList();
            var foundIds = mats.Select(m => m.Id).ToHashSet();
            var missing = materialIds.Where(id => !foundIds.Contains(id)).ToList();
            if (missing.Any())
                throw new BusinessException("Some materials do not exist", 400,
                    new Dictionary<string, string[]>
                    {
                        ["materialId"] = missing.Select(x => $"MaterialId {x} not found").ToArray()
                    });

            // existing rows
            var existing = await recipeRepo.FindAsync(r => r.ServiceId == serviceId && r.BodyType == bodyType);
            var existingMap = existing.ToDictionary(x => x.MaterialId, x => x);

            // strategy: sync
            // - materials not in request => IsActive=false
            // - in request => upsert (IsActive=true + update qty)
            var requestMap = request.Materials.ToDictionary(x => x.MaterialId, x => x);

            foreach (var row in existing)
            {
                if (!requestMap.ContainsKey(row.MaterialId))
                {
                    if (row.IsActive)
                    {
                        row.IsActive = false;
                        recipeRepo.Update(row);
                    }
                }
            }

            foreach (var req in request.Materials)
            {
                if (req.DefaultQty < 0)
                    throw new BusinessException("DefaultQty cannot be negative", 400);

                if (existingMap.TryGetValue(req.MaterialId, out var row))
                {
                    row.DefaultQty = req.DefaultQty;
                    row.IsActive = true;
                    recipeRepo.Update(row);
                }
                else
                {
                    await recipeRepo.AddAsync(new ServiceMaterialRecipe
                    {
                        ServiceId = serviceId,
                        BodyType = bodyType,
                        MaterialId = req.MaterialId,
                        DefaultQty = req.DefaultQty,
                        IsActive = true
                    });
                }
            }

            await _uow.SaveChangesAsync();

            // return
            return await GetAsync(serviceId, bodyType);
        }
    }
}
