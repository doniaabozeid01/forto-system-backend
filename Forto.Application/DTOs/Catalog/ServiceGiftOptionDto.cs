namespace Forto.Application.DTOs.Catalog
{
    /// <summary>خيار هدية مرتبط بخدمة (ServiceGiftOption).</summary>
    public class ServiceGiftOptionDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? ProductSku { get; set; }
        public bool IsActive { get; set; }
    }
}
