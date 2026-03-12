using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Inventory.Products;
using Forto.Domain.Entities.Inventory;
using Forto.Domain.Entities.Ops;
using Forto.Domain.Enum;
using ProductCategoryEntity = Forto.Domain.Entities.Inventory.ProductCategory;

namespace Forto.Application.Abstractions.Services.Inventory.Products
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;

        public ProductService(IUnitOfWork uow) => _uow = uow;

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var repo = _uow.Repository<Product>();

            if (request.InitialStockQty.HasValue && !request.BranchId.HasValue)
                throw new BusinessException("BranchId is required when InitialStockQty is provided", 400);

            if (request.CategoryId.HasValue)
            {
                var cat = await _uow.Repository<ProductCategoryEntity>().GetByIdAsync(request.CategoryId.Value);
                if (cat == null || !cat.IsActive)
                    throw new BusinessException("Product category not found or inactive", 400);
            }

            var name = request.Name.Trim();
            var exists = await repo.AnyAsync(p => p.Name == name);
            if (exists)
                throw new BusinessException("Product name already exists", 409);

            var sku = request.Sku.Trim();
            var skuExists = await repo.AnyAsync(p => p.Sku == sku);
            if (skuExists)
                throw new BusinessException("Product sku already exists", 409);

            var p = new Product
            {
                Name = name,
                Sku = request.Sku?.Trim(),
                SalePrice = request.SalePrice,
                CostPerUnit = request.CostPerUnit,
                CategoryId = request.CategoryId,
                IsActive = true
            };

            await repo.AddAsync(p);
            await _uow.SaveChangesAsync();

            // لو أُدخل مخزون ابتدائي → تسجيل حركة Stock In في الفرع
            if (request.InitialStockQty.HasValue && request.InitialStockQty.Value > 0)
            {
                // TotalCost = InitialStockQty * CostPerUnit يُخزّن في decimal(18,3) — أقصى قيمة 999_999_999_999_999.999
                const decimal maxTotalCost = 999_999_999_999_999.999m;
                var totalCost = request.InitialStockQty.Value * p.CostPerUnit;
                if (totalCost > maxTotalCost)
                    throw new BusinessException(
                        "InitialStockQty × CostPerUnit exceeds the maximum storable value (999,999,999,999,999.999). Please reduce quantity or cost.",
                        400);

                var branchId = request.BranchId!.Value;
                var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
                if (branch == null || !branch.IsActive)
                    throw new BusinessException("Branch not found or inactive", 404);

                var stockRepo = _uow.Repository<BranchProductStock>();
                var moveRepo = _uow.Repository<ProductMovement>();

                var stock = (await stockRepo.FindTrackingAsync(s => s.BranchId == branchId && s.ProductId == p.Id))
                    .FirstOrDefault();

                var isNewStock = stock == null;
                if (stock == null)
                {
                    stock = new BranchProductStock
                    {
                        BranchId = branchId,
                        ProductId = p.Id,
                        OnHandQty = 0,
                        TotalCostOfStock = 0,
                        ReservedQty = 0,
                        ReorderLevel = request.ReorderLevel ?? 0
                    };
                    await stockRepo.AddAsync(stock);
                }
                else
                {
                    stock.ReorderLevel = request.ReorderLevel ?? stock.ReorderLevel;
                    stockRepo.Update(stock);
                }

                var qty = request.InitialStockQty.Value;
                var unitCost = p.CostPerUnit;
                stock.TotalCostOfStock += qty * unitCost;
                stock.OnHandQty += qty;
                if (!isNewStock)
                    stockRepo.Update(stock);

                await moveRepo.AddAsync(new ProductMovement
                {
                    BranchId = branchId,
                    ProductId = p.Id,
                    MovementType = ProductMovementType.In,
                    Qty = qty,
                    UnitCostSnapshot = unitCost,
                    TotalCost = qty * unitCost,
                    OccurredAt = DateTime.UtcNow,
                    RecordedByEmployeeId = null,
                    Notes = "Initial stock on product creation"
                });

                await _uow.SaveChangesAsync();
            }

            var categoryName = await GetCategoryNameAsync(p.CategoryId);
            return Map(p, categoryName);
        }



        public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(int? categoryId = null)
        {
            var repo = _uow.Repository<Product>();
            var list = categoryId.HasValue
                ? await repo.FindAsync(p => !p.IsDeleted && p.CategoryId == categoryId.Value)
                : await repo.FindAsync(p => !p.IsDeleted);
            var categoryNameMap = await GetCategoryNameMapAsync(list.Where(x => x.CategoryId.HasValue).Select(x => x.CategoryId!.Value).Distinct().ToList());
            return list.Select(p => Map(p, p.CategoryId.HasValue && categoryNameMap.TryGetValue(p.CategoryId.Value, out var n) ? n : null)).ToList();
        }



        public async Task<IReadOnlyList<ProductWithStockResponse>> GetAllWithStockAsync(int branchId, int? categoryId = null)
        {
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            var productRepo = _uow.Repository<Product>();
            var stockRepo = _uow.Repository<BranchProductStock>();

            var products = categoryId.HasValue
                ? await productRepo.FindAsync(p => !p.IsDeleted && p.CategoryId == categoryId.Value)
                : await productRepo.FindAsync(p => !p.IsDeleted);
            if (products.Count == 0)
                return new List<ProductWithStockResponse>();

            var productIds = products.Select(p => p.Id).ToList();
            var stocks = await stockRepo.FindAsync(s => s.BranchId == branchId && productIds.Contains(s.ProductId));
            var stockMap = stocks.ToDictionary(s => s.ProductId, s => s);
            var categoryNameMap = await GetCategoryNameMapAsync(products.Where(p => p.CategoryId.HasValue).Select(p => p.CategoryId!.Value).Distinct().ToList());

            return products.Select(p =>
            {
                stockMap.TryGetValue(p.Id, out var st);
                var onHand = st?.OnHandQty ?? 0m;
                var reserved = st?.ReservedQty ?? 0m;
                var available = onHand - reserved;
                if (available < 0) available = 0;
                var categoryName = p.CategoryId.HasValue && categoryNameMap.TryGetValue(p.CategoryId.Value, out var cn) ? cn : null;
                return new ProductWithStockResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Sku = p.Sku,
                    SalePrice = p.SalePrice,
                    CostPerUnit = p.CostPerUnit,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryName = categoryName,
                    OnHandQty = onHand,
                    ReservedQty = reserved,
                    AvailableQty = available,
                    ReorderLevel = st?.ReorderLevel ?? 0m
                };
            }).ToList();
        }



        public async Task<ProductResponse?> GetByIdAsync(int id)
        {
            var p = await _uow.Repository<Product>().GetByIdAsync(id);
            if (p == null) return null;
            var categoryName = await GetCategoryNameAsync(p.CategoryId);
            return Map(p, categoryName);
        }


        public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request)
        {
            var repo = _uow.Repository<Product>();
            var p = await repo.GetByIdAsync(id);
            if (p == null) return null;

            var name = request.Name.Trim();

            var exists = await repo.AnyAsync(x => x.Id != id && x.Name == name);
            if (exists)
                throw new BusinessException("Product name already exists", 409);

            if (request.CategoryId.HasValue)
            {
                var cat = await _uow.Repository<ProductCategoryEntity>().GetByIdAsync(request.CategoryId.Value);
                if (cat == null || !cat.IsActive)
                    throw new BusinessException("Product category not found or inactive", 400);
            }

            p.Name = name;
            p.Sku = request.Sku?.Trim();
            p.SalePrice = request.SalePrice;
            p.CostPerUnit = request.CostPerUnit;
            p.CategoryId = request.CategoryId;
            p.IsActive = request.IsActive;

            repo.Update(p);
            await _uow.SaveChangesAsync();

            var categoryName = await GetCategoryNameAsync(p.CategoryId);
            return Map(p, categoryName);
        }



        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Product>();
            var p = await repo.GetByIdAsync(id);
            if (p == null) return false;

            repo.Delete(p);
            await _uow.SaveChangesAsync();
            return true;
        }



        private async Task<string?> GetCategoryNameAsync(int? categoryId)
        {
            if (!categoryId.HasValue) return null;
            var c = await _uow.Repository<ProductCategoryEntity>().GetByIdAsync(categoryId.Value);
            return c?.Name;
        }



        private async Task<Dictionary<int, string>> GetCategoryNameMapAsync(List<int> categoryIds)
        {
            if (categoryIds == null || categoryIds.Count == 0) return new Dictionary<int, string>();
            var list = await _uow.Repository<ProductCategoryEntity>().FindAsync(c => categoryIds.Contains(c.Id));
            return list.ToDictionary(c => c.Id, c => c.Name);
        }



        private static ProductResponse Map(Product p, string? categoryName = null) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            SalePrice = p.SalePrice,
            CostPerUnit = p.CostPerUnit,
            IsActive = p.IsActive,
            CategoryId = p.CategoryId,
            CategoryName = categoryName
        };

    }

}
