using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Inventory.Materials
{
    public class CreateMaterialRequest
    {
        [Required, MinLength(2)]
        public string Name { get; set; } = "";

        [Required]
        public MaterialUnit Unit { get; set; }

        [Range(0, 1000000)]
        public decimal CostPerUnit { get; set; }

        [Range(0, 1000000)]
        public decimal ChargePerUnit { get; set; }
    }

}
