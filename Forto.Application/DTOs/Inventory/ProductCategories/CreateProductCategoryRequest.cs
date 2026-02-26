using System.ComponentModel.DataAnnotations;

namespace Forto.Application.DTOs.Inventory.ProductCategories
{
    public class CreateProductCategoryRequest
    {
        [Required, MinLength(2)]
        public string Name { get; set; } = "";
        public int? ParentId { get; set; }
    }
}
