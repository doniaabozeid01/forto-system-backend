using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Ops.Products;
using Forto.Domain.Entities.Employees;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;

namespace Forto.Application.Abstractions.Services.Ops.Products.StockMovement
{
    public class ProductStockMovementService : IProductStockMovementService
    {
        private readonly IUnitOfWork _uow;
           
        public ProductStockMovementService(IUnitOfWork uow) => _uow = uow;

        private static bool IsCashierRole(EmployeeRole role)
            => role == EmployeeRole.Cashier || role == EmployeeRole.Supervisor || role == EmployeeRole.Admin;

        private async Task RequireCashierAsync(int cashierId)
        {
            var emp = await _uow.Repository<Employee>().GetByIdAsync(cashierId);
            if (emp == null || !emp.IsActive) throw new BusinessException("Cashier not found", 404);
            if (!IsCashierRole(emp.Role)) throw new BusinessException("Not allowed", 403);
        }

        public async Task StockInAsync(int branchId, StockInProductRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive) throw new BusinessException("Branch not found", 404);

            var productRepo = _uow.Repository<Product>();
            var product = await productRepo.GetByIdAsync(request.ProductId);
            if (product == null || !product.IsActive) throw new BusinessException("Product not found", 404);

            var unitCost = request.UnitCost ?? product.CostPerUnit; // ✅ fallback
            var occurred = request.OccurredAt ?? DateTime.UtcNow;

            var stockRepo = _uow.Repository<BranchProductStock>();
            var moveRepo = _uow.Repository<ProductMovement>();

            var stock = (await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && s.ProductId == request.ProductId))
                .FirstOrDefault();

            var isNewStock = stock == null;
            if (stock == null)
            {
                stock = new BranchProductStock
                {
                    BranchId = branchId,
                    ProductId = request.ProductId,
                    OnHandQty = 0,
                    TotalCostOfStock = 0,
                    ReservedQty = 0,
                    ReorderLevel = 0
                };
                await stockRepo.AddAsync(stock);
            }

            // الحساب خطوة بخطوة (متوسط مرجح متداول):
            // (1) الرصيد الجديد = الرصيد القديم + كمية الإدخال
            // (2) تكلفة الرصيد الجديدة = تكلفة الرصيد القديمة + (كمية الإدخال × سعر شراء الإدخال)
            // (3) متوسط التكلفة الجديد = تكلفة الرصيد الجديدة ÷ الرصيد الجديد  ← يُحسب عند الحاجة (بيع/جرد) من TotalCostOfStock / OnHandQty
            var newBatchCost = request.Qty * unitCost;  // كمية الإدخال × سعر شراء الإدخال
            stock.TotalCostOfStock += newBatchCost;     // (2)
            stock.OnHandQty += request.Qty;             // (1)
            if (!isNewStock)
                stockRepo.Update(stock);

            await moveRepo.AddAsync(new ProductMovement
            {
                BranchId = branchId,
                ProductId = request.ProductId,
                MovementType = ProductMovementType.In,
                Qty = request.Qty,
                UnitCostSnapshot = unitCost,
                TotalCost = request.Qty * unitCost,
                OccurredAt = occurred,
                RecordedByEmployeeId = request.CashierId,
                Notes = request.Notes
            });

            // التكلفة على المنتج = المتوسط المرجح الجديد
            if (stock.OnHandQty > 0)
            {
                product.CostPerUnit = stock.TotalCostOfStock / stock.OnHandQty;
                productRepo.Update(product);
            }

            // سعر البيع: لو مُدخل في الطلب نحدّث، لو مش مبعوت نتركه كما هو
            if (request.SalePrice.HasValue)
            {
                product.SalePrice = request.SalePrice.Value;
                productRepo.Update(product);
            }

            await _uow.SaveChangesAsync();
        }







        //public async Task AdjustAsync(int branchId, AdjustProductStockRequest request)
        //{
        //    await RequireCashierAsync(request.CashierId);

        //    var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
        //    if (branch == null || !branch.IsActive) throw new BusinessException("Branch not found", 404);

        //    var productRepo = _uow.Repository<Product>();
        //    var product = await productRepo.GetByIdAsync(request.ProductId);
        //    if (product == null || !product.IsActive) throw new BusinessException("Product not found", 404);

        //    var stockRepo = _uow.Repository<BranchProductStock>();
        //    var moveRepo = _uow.Repository<ProductMovement>();

        //    var stock = (await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && s.ProductId == request.ProductId))
        //        .FirstOrDefault();

        //    if (stock == null)
        //    {
        //        stock = new BranchProductStock
        //        {
        //            BranchId = branchId,
        //            ProductId = request.ProductId,
        //            OnHandQty = 0,
        //            ReservedQty = 0,
        //            ReorderLevel = 0
        //        };
        //        await stockRepo.AddAsync(stock);
        //    }

        //    // ماينفعش physical أقل من reserved
        //    if (request.PhysicalOnHandQty < stock.ReservedQty)
        //        throw new BusinessException("PhysicalOnHandQty cannot be less than ReservedQty", 409);

        //    var diff = request.PhysicalOnHandQty - stock.OnHandQty; // ممكن + أو -
        //    stock.OnHandQty = request.PhysicalOnHandQty;
        //    stockRepo.Update(stock);

        //    var occurred = request.OccurredAt ?? DateTime.UtcNow;

        //    await moveRepo.AddAsync(new ProductMovement
        //    {
        //        BranchId = branchId,
        //        ProductId = request.ProductId,
        //        MovementType = ProductMovementType.Adjust,
        //        Qty = diff,
        //        UnitCostSnapshot = product.CostPerUnit,   // snapshot بسيط
        //        TotalCost = diff * product.CostPerUnit,
        //        OccurredAt = occurred,
        //        RecordedByEmployeeId = request.CashierId,
        //        Notes = request.Notes
        //    });

        //    await _uow.SaveChangesAsync();
        //}












        public async Task AdjustAsync(int branchId, AdjustProductStockRequest request)
        {
            await RequireCashierAsync(request.CashierId);

            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            var productRepo = _uow.Repository<Product>();
            var product = await productRepo.GetByIdAsync(request.ProductId);
            if (product == null || !product.IsActive)
                throw new BusinessException("Product not found", 404);

            var stockRepo = _uow.Repository<BranchProductStock>();
            var moveRepo = _uow.Repository<ProductMovement>();

            var stock = (await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && s.ProductId == request.ProductId))
                .FirstOrDefault();

            if (stock == null)
            {
                stock = new BranchProductStock
                {
                    BranchId = branchId,
                    ProductId = request.ProductId,
                    OnHandQty = 0,
                    TotalCostOfStock = 0,
                    ReservedQty = 0,
                    ReorderLevel = 0
                };
                await stockRepo.AddAsync(stock);
            }

            // ماينفعش physical أقل من reserved
            if (request.PhysicalOnHandQty < stock.ReservedQty)
                throw new BusinessException("PhysicalOnHandQty cannot be less than ReservedQty", 409);

            var occurred = request.OccurredAt ?? DateTime.UtcNow;
            var diff = request.PhysicalOnHandQty - stock.OnHandQty; // ممكن + أو -

            // تكلفة الوحدة للحركة: عند النقص نستخدم المتوسط المرجح الحالي؛ عند الزيادة متوسط إدخالات الشهر أو تكلفة المنتج
            decimal unitCostForAdjust;
            if (diff < 0 && stock.OnHandQty > 0)
            {
                unitCostForAdjust = stock.TotalCostOfStock / stock.OnHandQty;
            }
            else
            {
                var monthStart = new DateTime(occurred.Year, occurred.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var nextMonthStart = monthStart.AddMonths(1);
                var inMoves = await moveRepo.FindAsync(m =>
                    m.BranchId == branchId &&
                    m.ProductId == request.ProductId &&
                    m.MovementType == ProductMovementType.In &&
                    m.OccurredAt >= monthStart &&
                    m.OccurredAt < nextMonthStart);
                var totalInQty = inMoves.Sum(m => m.Qty);
                var totalInCost = inMoves.Sum(m => m.TotalCost);
                unitCostForAdjust = totalInQty > 0 ? totalInCost / totalInQty : product.CostPerUnit;
            }

            // تحديث إجمالي تكلفة الرصيد (المتوسط المرجح المتداول)
            stock.TotalCostOfStock += diff * unitCostForAdjust;
            if (stock.TotalCostOfStock < 0) stock.TotalCostOfStock = 0;
            stock.OnHandQty = request.PhysicalOnHandQty;
            stockRepo.Update(stock);

            // Record ADJUST movement
            await moveRepo.AddAsync(new ProductMovement
            {
                BranchId = branchId,
                ProductId = request.ProductId,
                MovementType = ProductMovementType.Adjust,
                Qty = diff, // ممكن سالب
                UnitCostSnapshot = unitCostForAdjust,
                TotalCost = diff * unitCostForAdjust, // سالب لو diff سالب
                OccurredAt = occurred,
                RecordedByEmployeeId = request.CashierId,
                Notes = request.Notes
            });

            await _uow.SaveChangesAsync();
        }




    }

}
