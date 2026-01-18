using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Ops.Products;

namespace Forto.Application.Abstractions.Services.Ops.Products
{
    public interface IBranchProductStockService
    {
        Task<BranchProductStockResponse> UpsertAsync(int branchId, UpsertBranchProductStockRequest request);
        Task<IReadOnlyList<BranchProductStockResponse>> GetBranchStockAsync(int branchId);
    }

}
