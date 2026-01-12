using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Ops.Stock;

namespace Forto.Application.Abstractions.Services.Ops.Stock
{
    public interface IBranchStockService
    {
        Task<BranchStockItemResponse> UpsertAsync(int branchId, UpsertBranchStockRequest request);
        Task<IReadOnlyList<BranchStockItemResponse>> GetBranchStockAsync(int branchId);
    }
}
