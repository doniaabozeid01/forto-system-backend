using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Inventory.MaterialsCheck;

namespace Forto.Application.Abstractions.Services.Inventory.MaterialsCheck
{
    public interface IMaterialsCheckService
    {
        Task<MaterialsCheckResponse> CheckAsync(int branchId, MaterialsCheckRequest request);
    }
}
