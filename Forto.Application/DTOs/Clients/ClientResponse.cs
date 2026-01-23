using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Cars;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Clients
{
    public class ClientResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string? Email { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public List<ClientCarResponse> Cars { get; set; } = new();


    }


    public class ClientCarResponse
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = "";
        public CarBodyType BodyType { get; set; }
        // أو أحيانًا int
        public bool IsDefault { get; set; }
    }

}
