using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Services
{
    public class ServiceRateResponse
    {
        public int Id { get; set; }
        public CarBodyType BodyType { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
    }
}
