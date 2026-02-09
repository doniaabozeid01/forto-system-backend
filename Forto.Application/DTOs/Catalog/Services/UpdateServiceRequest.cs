using System.ComponentModel.DataAnnotations;

namespace Forto.Application.DTOs.Catalog.Services
{
    public class UpdateServiceRequest
    {
        [Required]
        public int CategoryId { get; set; }

        [Required, MinLength(2)]
        public string Name { get; set; } = "";

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
