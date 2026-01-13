using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Usage
{
    public class UpdateMaterialActualDto
    {
        [Required]
        public int MaterialId { get; set; }

        [Range(0, 100000000)]
        public decimal ActualQty { get; set; }
    }
}
