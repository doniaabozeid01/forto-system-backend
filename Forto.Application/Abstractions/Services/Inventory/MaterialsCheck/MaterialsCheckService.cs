using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Inventory.MaterialsCheck;
using Forto.Domain.Entities.Catalog;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;

namespace Forto.Application.Abstractions.Services.Inventory.MaterialsCheck
{
    public class MaterialsCheckService : IMaterialsCheckService
    {
        private readonly IUnitOfWork _uow;

        public MaterialsCheckService(IUnitOfWork uow) => _uow = uow;

        public async Task<MaterialsCheckResponse> CheckAsync(int branchId, MaterialsCheckRequest request)
        {
            // validate branch
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            var bodyType = request.BodyType;

            var serviceIds = request.ServiceIds?.Distinct().ToList() ?? new List<int>();
            if (serviceIds.Count == 0)
                throw new BusinessException("ServiceIds is required", 400);

            // ensure services exist + get names
            var serviceRepo = _uow.Repository<Service>();
            var services = await serviceRepo.FindAsync(s => serviceIds.Contains(s.Id) && s.IsActive);

            var foundServiceIds = services.Select(s => s.Id).ToHashSet();
            var missingServiceIds = serviceIds.Where(id => !foundServiceIds.Contains(id)).ToList();

            if (missingServiceIds.Any())
            {
                var msg = missingServiceIds
                    .Select(id => $"ServiceId {id} not found")
                    .ToArray();

                throw new BusinessException(
                    "Some services do not exist",
                    400,
                    new Dictionary<string, string[]>
                    {
                        ["services"] = msg
                    });
            }

            // 1) load recipes for all services for this bodyType
            var recipeRepo = _uow.Repository<ServiceMaterialRecipe>();
            var recipes = await recipeRepo.FindAsync(r =>
                r.IsActive &&
                r.BodyType == bodyType &&
                serviceIds.Contains(r.ServiceId));

            // If a service has no recipe rows -> block (missing recipe)
            var recipeServiceIds = recipes.Select(r => r.ServiceId).Distinct().ToHashSet();
            var servicesWithoutRecipeIds = serviceIds.Where(id => !recipeServiceIds.Contains(id)).ToList();

            if (servicesWithoutRecipeIds.Any())
            {
                var missingServices = services
                    .Where(s => servicesWithoutRecipeIds.Contains(s.Id))
                    .Select(s => $"No recipe for '{s.Name}' (ServiceId={s.Id}) with bodyType {bodyType}")
                    .ToArray();

                throw new BusinessException(
                    "Missing recipe for one or more services for this car type",
                    409,
                    new Dictionary<string, string[]>
                    {
                        ["missingRecipes"] = missingServices
                    });
            }

            // 2) aggregate required qty per material
            // (handles shared materials across multiple services)
            var requiredByMaterial = recipes
                .GroupBy(r => r.MaterialId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.DefaultQty));

            var materialIds = requiredByMaterial.Keys.ToList();

            // 3) get stock for branch for those materials
            var stockRepo = _uow.Repository<BranchMaterialStock>();
            var stocks = await stockRepo.FindAsync(s => s.BranchId == branchId && materialIds.Contains(s.MaterialId));
            var stockMap = stocks.ToDictionary(s => s.MaterialId, s => s);

            // 4) load materials names/units
            var materialRepo = _uow.Repository<Material>();
            var mats = await materialRepo.FindAsync(m => materialIds.Contains(m.Id) && m.IsActive);
            var matMap = mats.ToDictionary(m => m.Id, m => m);

            // build required/missing lists (with names)
            var requiredList = new List<MaterialRequirementDto>();
            var missingList = new List<MaterialRequirementDto>();

            foreach (var mid in materialIds.OrderBy(x => x))
            {
                var reqQty = requiredByMaterial[mid];

                stockMap.TryGetValue(mid, out var stock);
                matMap.TryGetValue(mid, out var mat);

                var onHand = stock?.OnHandQty ?? 0m;
                var reserved = stock?.ReservedQty ?? 0m;
                var available = onHand - reserved;
                if (available < 0) available = 0;

                var shortage = reqQty > available ? (reqQty - available) : 0m;

                var materialName = mat?.Name ?? $"MaterialId={mid}";
                var unit = mat?.Unit.ToString() ?? "";

                var dto = new MaterialRequirementDto
                {
                    MaterialId = mid,
                    MaterialName = materialName,
                    Unit = unit,
                    RequiredQty = reqQty,
                    AvailableQty = available,
                    ShortageQty = shortage
                };

                requiredList.Add(dto);

                if (shortage > 0)
                    missingList.Add(dto);
            }

            // services summary for UI (names)
            var serviceSummary = services
                .OrderBy(s => s.Name)
                .Select(s => new ServiceBriefDto
                {
                    ServiceId = s.Id,
                    ServiceName = s.Name
                })
                .ToList();

            return new MaterialsCheckResponse
            {
                BranchId = branchId,
                ServiceCount = serviceIds.Count,
                MaterialCount = materialIds.Count,
                IsAvailable = missingList.Count == 0,
                Services = serviceSummary,
                Required = requiredList,
                Missing = missingList
            };
        }






    }
}
