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

namespace Forto.Application.Abstractions.Services.Inventory.Products
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;

        public ProductService(IUnitOfWork uow) => _uow = uow;

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var repo = _uow.Repository<Product>();

            var name = request.Name.Trim();
            var exists = await repo.AnyAsync(p => p.Name == name);
            if (exists)
                throw new BusinessException("Product name already exists", 409);

            var p = new Product
            {
                Name = name,
                Sku = request.Sku?.Trim(),
                SalePrice = request.SalePrice,
                CostPerUnit = request.CostPerUnit,
                IsActive = true
            };

            await repo.AddAsync(p);
            await _uow.SaveChangesAsync();

            return Map(p);
        }

        public async Task<IReadOnlyList<ProductResponse>> GetAllAsync()
        {
            var list = await _uow.Repository<Product>().GetAllAsync();
            return list.Select(Map).ToList();
        }
        public async Task<IReadOnlyList<ProductWithStockResponse>> GetAllWithStockAsync(int branchId)
        {
            // validate branch
            var branch = await _uow.Repository<Branch>().GetByIdAsync(branchId);
            if (branch == null || !branch.IsActive)
                throw new BusinessException("Branch not found", 404);

            var productRepo = _uow.Repository<Product>();
            var stockRepo = _uow.Repository<BranchProductStock>();

            // 1) products
            var products = await productRepo.GetAllAsync();
            if (products.Count == 0)
                return new List<ProductWithStockResponse>();

            var productIds = products.Select(p => p.Id).ToList();

            // 2) stock for that branch
            var stocks = await stockRepo.FindAsync(s => s.BranchId == branchId && productIds.Contains(s.ProductId));
            var stockMap = stocks.ToDictionary(s => s.ProductId, s => s);

            // 3) merge
            return products.Select(p =>
            {
                stockMap.TryGetValue(p.Id, out var st);

                var onHand = st?.OnHandQty ?? 0m;
                var reserved = st?.ReservedQty ?? 0m;
                var available = onHand - reserved;
                if (available < 0) available = 0;

                return new ProductWithStockResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Sku = p.Sku,
                    SalePrice = p.SalePrice,
                    CostPerUnit = p.CostPerUnit,
                    IsActive = p.IsActive,

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
            return p == null ? null : Map(p);
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

            p.Name = name;
            p.Sku = request.Sku?.Trim();
            p.SalePrice = request.SalePrice;
            p.CostPerUnit = request.CostPerUnit;
            p.IsActive = request.IsActive;

            repo.Update(p);
            await _uow.SaveChangesAsync();

            return Map(p);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Product>();
            var p = await repo.GetByIdAsync(id);
            if (p == null) return false;

            repo.HardDelete(p);
            await _uow.SaveChangesAsync();
            return true;
        }

        private static ProductResponse Map(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            SalePrice = p.SalePrice,
            CostPerUnit = p.CostPerUnit,
            IsActive = p.IsActive
        };
    }

}
