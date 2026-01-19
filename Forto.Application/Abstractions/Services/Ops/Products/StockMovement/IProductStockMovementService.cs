using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Ops.Products;

namespace Forto.Application.Abstractions.Services.Ops.Products.StockMovement
{
    public interface IProductStockMovementService
    {
        Task StockInAsync(int branchId, StockInProductRequest request);
        Task AdjustAsync(int branchId, AdjustProductStockRequest request);
    }
}
