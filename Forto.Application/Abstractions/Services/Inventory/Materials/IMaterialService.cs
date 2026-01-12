using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Inventory.Materials;

namespace Forto.Application.Abstractions.Services.Inventory.Materials
{
    public interface IMaterialService
    {
        Task<MaterialResponse> CreateAsync(CreateMaterialRequest request);
        Task<IReadOnlyList<MaterialResponse>> GetAllAsync();
        Task<MaterialResponse?> GetByIdAsync(int id);
        Task<MaterialResponse?> UpdateAsync(int id, UpdateMaterialRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
