using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Clients
{
    public class Car : BaseEntity
    {
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public string PlateNumber { get; set; } = "";
        public CarBodyType BodyType { get; set; }

        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public int? Year { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}
