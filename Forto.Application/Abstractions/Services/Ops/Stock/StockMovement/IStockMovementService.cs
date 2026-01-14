using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Ops.Stock;

namespace Forto.Application.Abstractions.Services.Ops.Stock.StockMovement
{
    public interface IStockMovementService
    {
        Task StockInAsync(int branchId, StockInRequest request);
        Task StockAdjustAsync(int branchId, StockAdjustRequest request);
    }

}
