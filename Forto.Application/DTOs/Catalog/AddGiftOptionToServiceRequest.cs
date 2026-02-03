using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Forto.Application.DTOs.Catalog.Services
{
    /// <summary>طلب إضافة هدايا لخدمة — productIds فقط (حتى لو منتج واحد).</summary>
    public class AddGiftOptionToServiceRequest
    {
        [Required]
        public List<int> ProductIds { get; set; } = new();
    }
}
