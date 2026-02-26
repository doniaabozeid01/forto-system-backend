namespace Forto.Domain.Entities.Inventory
{
    /// <summary>فئة المنتجات (مستقلة عن فئات كتالوج الخدمات).</summary>
    public class ProductCategory : BaseEntity
    {
        public string Name { get; set; } = "";
        public int? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
