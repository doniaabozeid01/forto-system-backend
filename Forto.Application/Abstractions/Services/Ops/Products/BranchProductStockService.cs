using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Ops.Products;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;

namespace Forto.Application.Abstractions.Services.Ops.Products
{

    public class BranchProductStockService : IBranchProductStockService
    {
        private readonly IUnitOfWork _uow;

        public BranchProductStockService(IUnitOfWork uow) => _uow = uow;

        public async Task<BranchProductStockResponse> UpsertAsync(int branchId, UpsertBranchProductStockRequest request)
        {
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            var product = await _uow.Repository<Product>().GetByIdAsync(request.ProductId);
            if (product == null || !product.IsActive)
                throw new BusinessException("Product not found", 404);

            var stockRepo = _uow.Repository<BranchProductStock>();

            var stock = (await stockRepo.FindTrackingAsync(s =>
                s.BranchId == branchId && s.ProductId == request.ProductId)).FirstOrDefault();

            if (stock == null)
            {
                stock = new BranchProductStock
                {
                    BranchId = branchId,
                    ProductId = request.ProductId,
                    OnHandQty = request.OnHandQty,
                    ReservedQty = 0,
                    ReorderLevel = request.ReorderLevel
                };
                await stockRepo.AddAsync(stock);
            }
            else
            {
                // protect reserved
                if (request.OnHandQty < stock.ReservedQty)
                    throw new BusinessException("OnHandQty cannot be less than ReservedQty", 409);

                stock.OnHandQty = request.OnHandQty;
                stock.ReorderLevel = request.ReorderLevel;
                stockRepo.Update(stock);
            }

            await _uow.SaveChangesAsync();

            return new BranchProductStockResponse
            {
                BranchId = branchId,
                ProductId = product.Id,
                ProductName = product.Name,
                Sku = product.Sku,
                OnHandQty = stock.OnHandQty,
                ReservedQty = stock.ReservedQty,
                AvailableQty = stock.OnHandQty - stock.ReservedQty,
                ReorderLevel = stock.ReorderLevel
            };
        }

        //    public async Task<IReadOnlyList<BranchProductStockResponse>> GetBranchStockAsync(int branchId)
        //    {
        //        var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
        //        if (branch == null)
        //            throw new BusinessException("Branch not found", 404);

        //        var stockRepo = _uow.Repository<BranchProductStock>();
        //        var productRepo = _uow.Repository<Product>();

        //        var stocks = await stockRepo.FindAsync(s => s.BranchId == branchId);
        //        if (stocks.Count == 0) return new List<BranchProductStockResponse>();


        //        var productIds = stocks.Select(s => s.ProductId).Distinct().ToList();
        //        if (productIds == null)
        //            throw new BusinessException("productIds not found", 404);




        //        var products = await productRepo.FindAsync(p => productIds.Contains(p.Id));
        //        if (products == null)
        //            throw new BusinessException("products not found", 404);



        //        var productMap = products.ToDictionary(p => p.Id, p => p);

        //        return stocks.Select(s =>
        //        {
        //            productMap.TryGetValue(s.ProductId, out var p);
        //            return new BranchProductStockResponse
        //            {
        //                BranchId = s.BranchId,
        //                ProductId = s.ProductId,
        //                ProductName = p?.Name ?? "",
        //                Sku = p?.Sku,
        //                OnHandQty = s.OnHandQty,
        //                ReservedQty = s.ReservedQty,
        //                AvailableQty = s.OnHandQty - s.ReservedQty,
        //                ReorderLevel = s.ReorderLevel
        //            };
        //        }).ToList();
        //    }
        //}




        public async Task<IReadOnlyList<BranchProductStockResponse>> GetBranchStockAsync(int branchId)
        {
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null)
                throw new BusinessException("Branch not found", 404);

            var stockRepo = _uow.Repository<BranchProductStock>();
            var productRepo = _uow.Repository<Product>();

            var stocks = await stockRepo.FindAsync(s => s.BranchId == branchId);

            // ✅ لو فاضي رجع list فاضية
            if (stocks.Count == 0)
                return new List<BranchProductStockResponse>();

            var productIds = stocks.Select(s => s.ProductId).Distinct().ToList();
            var products = await productRepo.FindAsync(p => productIds.Contains(p.Id));
            var productMap = products.ToDictionary(p => p.Id, p => p);

            return stocks.Select(s =>
            {
                productMap.TryGetValue(s.ProductId, out var p);

                return new BranchProductStockResponse
                {
                    BranchId = s.BranchId,
                    ProductId = s.ProductId,
                    ProductName = p?.Name ?? "",
                    Sku = p?.Sku,
                    OnHandQty = s.OnHandQty,
                    ReservedQty = s.ReservedQty,
                    AvailableQty = s.OnHandQty - s.ReservedQty,
                    ReorderLevel = s.ReorderLevel
                };
            }).ToList();
        }



    }
    }
