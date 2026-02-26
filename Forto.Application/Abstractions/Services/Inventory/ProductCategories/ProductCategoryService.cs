using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Inventory.ProductCategories;
using Forto.Domain.Entities.Inventory;

namespace Forto.Application.Abstractions.Services.Inventory.ProductCategories
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IUnitOfWork _uow;

        public ProductCategoryService(IUnitOfWork uow) => _uow = uow;

        public async Task<ProductCategoryResponse> CreateAsync(CreateProductCategoryRequest request)
        {
            var repo = _uow.Repository<ProductCategory>();
            if (request.ParentId.HasValue)
            {
                var parent = await repo.GetByIdAsync(request.ParentId.Value);
                if (parent == null)
                    throw new BusinessException("Parent category not found", 400,
                        new Dictionary<string, string[]> { ["parentId"] = new[] { "Invalid parentId." } });
            }

            var name = request.Name.Trim();
            var exists = await repo.AnyAsync(x => x.ParentId == request.ParentId && x.Name == name);
            if (exists)
                throw new BusinessException("Product category already exists", 409,
                    new Dictionary<string, string[]> { ["name"] = new[] { "Duplicate name under same parent." } });

            var entity = new ProductCategory
            {
                Name = name,
                ParentId = request.ParentId,
                IsActive = true
            };
            await repo.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<ProductCategoryResponse?> GetByIdAsync(int id)
        {
            var entity = await _uow.Repository<ProductCategory>().GetByIdAsync(id);
            return entity == null ? null : Map(entity);
        }

        public async Task<IReadOnlyList<ProductCategoryResponse>> GetAllAsync(int? parentId = null)
        {
            var repo = _uow.Repository<ProductCategory>();
            var list = parentId.HasValue
                ? await repo.FindAsync(x => x.ParentId == parentId.Value && !x.IsDeleted)
                : await repo.FindAsync(x => !x.IsDeleted);
            return list.Select(Map).ToList();
        }

        public async Task<ProductCategoryResponse?> UpdateAsync(int id, UpdateProductCategoryRequest request)
        {
            var repo = _uow.Repository<ProductCategory>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return null;
            if (request.ParentId == id)
                throw new BusinessException("Category cannot be its own parent", 400,
                    new Dictionary<string, string[]> { ["parentId"] = new[] { "Invalid parentId." } });
            if (request.ParentId.HasValue)
            {
                var parent = await repo.GetByIdAsync(request.ParentId.Value);
                if (parent == null)
                    throw new BusinessException("Parent category not found", 400,
                        new Dictionary<string, string[]> { ["parentId"] = new[] { "Invalid parentId." } });
            }
            var name = request.Name.Trim();
            var exists = await repo.AnyAsync(x => x.Id != id && x.ParentId == request.ParentId && x.Name == name);
            if (exists)
                throw new BusinessException("Product category already exists", 409,
                    new Dictionary<string, string[]> { ["name"] = new[] { "Duplicate name under same parent." } });
            entity.Name = name;
            entity.ParentId = request.ParentId;
            entity.IsActive = request.IsActive;
            repo.Update(entity);
            await _uow.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<ProductCategory>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return false;
            var hasChildren = await repo.AnyAsync(x => x.ParentId == id);
            if (hasChildren)
                throw new BusinessException("Cannot delete category that has subcategories", 409);
            repo.Delete(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        private static ProductCategoryResponse Map(ProductCategory x) => new()
        {
            Id = x.Id,
            Name = x.Name,
            ParentId = x.ParentId,
            IsActive = x.IsActive
        };
    }
}
