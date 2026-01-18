using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Inventory.Products;

namespace Forto.Application.Abstractions.Services.Inventory.Products
{

    public interface IProductService
    {
        Task<ProductResponse> CreateAsync(CreateProductRequest request);
        Task<IReadOnlyList<ProductResponse>> GetAllAsync();
        Task<ProductResponse?> GetByIdAsync(int id);
        Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
