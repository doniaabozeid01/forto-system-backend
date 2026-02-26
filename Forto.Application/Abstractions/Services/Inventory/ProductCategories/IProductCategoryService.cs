using Forto.Application.DTOs.Inventory.ProductCategories;

namespace Forto.Application.Abstractions.Services.Inventory.ProductCategories
{
    public interface IProductCategoryService
    {
        Task<ProductCategoryResponse> CreateAsync(CreateProductCategoryRequest request);
        Task<ProductCategoryResponse?> GetByIdAsync(int id);
        Task<IReadOnlyList<ProductCategoryResponse>> GetAllAsync(int? parentId = null);
        Task<ProductCategoryResponse?> UpdateAsync(int id, UpdateProductCategoryRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
