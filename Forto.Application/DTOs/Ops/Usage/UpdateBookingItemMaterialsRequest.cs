using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Ops.Usage
{
    public class UpdateBookingItemMaterialsRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required, MinLength(1)]
        public List<UpdateMaterialActualDto> Materials { get; set; } = new();
    }
}
