using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Services
{
    public class CreateServiceRequest
    {
        [Required]
        public int CategoryId { get; set; }

        [Required, MinLength(2)]
        public string Name { get; set; } = "";

        public string? Description { get; set; }
    }
}
