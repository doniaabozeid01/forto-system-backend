using Forto.Application.DTOs.Catalog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Catalogs.Recipes.MaterialApprove
{
    public interface IMaterialApprovalService
    {
        Task<int> CreateRequestAsync(int bookingItemId, CreateMaterialChangeRequestDto dto);
        Task ApproveAsync(int requestId, ReviewMaterialChangeRequestDto dto);
        Task RejectAsync(int requestId, ReviewMaterialChangeRequestDto dto);

        Task<IReadOnlyList<PendingMaterialRequestDto>> ListPendingAsync(int branchId, DateOnly date);

    }

}
