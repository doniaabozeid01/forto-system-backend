using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Catalog.Categories;
using Forto.Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Catalogs.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _uow;

        public CategoryService(IUnitOfWork uow) => _uow = uow;

        public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
        {
            var repo = _uow.Repository<Category>();

            // لو ParentId موجودة، اتأكد إنها موجودة
            if (request.ParentId.HasValue)
            {
                var parent = await repo.GetByIdAsync(request.ParentId.Value);
                if (parent == null)
                    throw new BusinessException("Parent category not found", 400,
                        new Dictionary<string, string[]> { ["parentId"] = new[] { "Invalid parentId." } });
            }

            // optional: منع تكرار الاسم تحت نفس الأب (بدون الاعتماد على unique index فقط)
            var name = request.Name.Trim();
            var exists = await repo.AnyAsync(x => x.ParentId == request.ParentId && x.Name == name);
            if (exists)
                throw new BusinessException("Category already exists", 409,
                    new Dictionary<string, string[]> { ["name"] = new[] { "Duplicate category name under same parent." } });

            var entity = new Category
            {
                Name = name,
                ParentId = request.ParentId,
                IsActive = true
            };

            await repo.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<CategoryResponse?> GetByIdAsync(int id)
        {
            var entity = await _uow.Repository<Category>().GetByIdAsync(id);
            return entity == null ? null : Map(entity);
        }

        public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(int? parentId = null)
        {
            var repo = _uow.Repository<Category>();

            var list = parentId.HasValue
                ? await repo.FindAsync(x => x.ParentId == parentId.Value)
                : await repo.GetAllAsync();

            return list.Select(Map).ToList();
        }

        public async Task<CategoryResponse?> UpdateAsync(int id, UpdateCategoryRequest request)
        {
            var repo = _uow.Repository<Category>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return null;

            // ParentId validation + منع إن category تبقى parent لنفسها
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

            // منع تكرار الاسم تحت نفس الأب
            var exists = await repo.AnyAsync(x => x.Id != id && x.ParentId == request.ParentId && x.Name == name);
            if (exists)
                throw new BusinessException("Category already exists", 409,
                    new Dictionary<string, string[]> { ["name"] = new[] { "Duplicate category name under same parent." } });

            entity.Name = name;
            entity.ParentId = request.ParentId;
            entity.IsActive = request.IsActive;

            repo.Update(entity);
            await _uow.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Category>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return false;

            // مهم: لو عندك subcategories، امنعي حذف category ليها children
            var hasChildren = await repo.AnyAsync(x => x.ParentId == id);
            if (hasChildren)
                throw new BusinessException("Cannot delete category that has subcategories", 409);

            repo.Delete(entity); // soft delete
            await _uow.SaveChangesAsync();
            return true;
        }

        private static CategoryResponse Map(Category x) => new()
        {
            Id = x.Id,
            Name = x.Name,
            ParentId = x.ParentId,
            IsActive = x.IsActive
        };
    }
}