namespace Forto.Application.DTOs.Inventory.ProductCategories
{
    public class ProductCategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int? ParentId { get; set; }
        public bool IsActive { get; set; }
    }
}
