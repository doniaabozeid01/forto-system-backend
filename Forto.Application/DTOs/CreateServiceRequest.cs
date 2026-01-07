using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs
{
    public class CreateServiceRequest
    {
        [Required, MinLength(2)]
        public string Name { get; set; } = "";

        [Range(1, 1000000)]
        public decimal Price { get; set; }

        [Range(1, 600)]
        public int DurationMinutes { get; set; }
    }
}
