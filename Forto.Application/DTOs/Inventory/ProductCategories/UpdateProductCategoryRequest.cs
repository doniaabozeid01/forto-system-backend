using System.ComponentModel.DataAnnotations;

namespace Forto.Application.DTOs.Inventory.ProductCategories
{
    public class UpdateProductCategoryRequest
    {
        [Required, MinLength(2)]
        public string Name { get; set; } = "";
        public int? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
