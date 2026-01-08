using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Services
{

    public class ServiceRateRequest
    {
        [Required]
        public CarBodyType BodyType { get; set; }

        [Range(0, 1000000)]
        public decimal Price { get; set; }

        [Range(1, 10000)]
        public int DurationMinutes { get; set; }
    }
}
