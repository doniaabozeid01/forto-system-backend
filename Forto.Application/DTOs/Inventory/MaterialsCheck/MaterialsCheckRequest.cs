using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Inventory.MaterialsCheck
{
    public class MaterialsCheckRequest
    {
        [Required]
        public CarBodyType BodyType { get; set; }

        [Required, MinLength(1)]
        public List<int> ServiceIds { get; set; } = new();
    }
}
