using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Bookings
{
    public class CarInput
    {
        [Required]
        public string PlateNumber { get; set; } = "";

        [Required]
        public CarBodyType BodyType { get; set; }

        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public int? Year { get; set; }

        public bool IsDefault { get; set; } = true;
    }
}
