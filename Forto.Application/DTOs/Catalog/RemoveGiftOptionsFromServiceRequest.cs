using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Forto.Application.DTOs.Catalog.Services
{
    /// <summary>طلب إزالة هدايا من خدمة — قائمة productIds.</summary>
    public class RemoveGiftOptionsFromServiceRequest
    {
        [Required]
        public List<int> ProductIds { get; set; } = new();
    }
}
