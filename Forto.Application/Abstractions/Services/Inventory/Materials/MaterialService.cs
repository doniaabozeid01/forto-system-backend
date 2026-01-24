using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Inventory.Materials;

namespace Forto.Application.Abstractions.Services.Inventory.Materials
{
    public class MaterialService : IMaterialService
    {
        private readonly IUnitOfWork _uow;
        public MaterialService(IUnitOfWork uow) => _uow = uow;

        public async Task<MaterialResponse> CreateAsync(CreateMaterialRequest request)
        {
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

            return Map(m);
        }

        public async Task<IReadOnlyList<MaterialResponse>> GetAllAsync()
        {
            var list = await _uow.Repository<Domain.Entities.Inventory.Material>().GetAllAsync();
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

            repo.HardDelete(m); // soft delete
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
