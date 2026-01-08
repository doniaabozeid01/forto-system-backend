using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Services
{
    public class ServiceResponse
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }

        public List<ServiceRateResponse> Rates { get; set; } = new();
    }
}
