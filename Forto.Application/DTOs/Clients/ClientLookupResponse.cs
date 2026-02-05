using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Clients
{
    public class ClientLookupResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string? Email { get; set; }
        public bool IsActive { get; set; }

        public int? DefaultCarId { get; set; }
        public List<Forto.Application.DTOs.Cars.CarResponse> Cars { get; set; } = new();

        /// <summary>عميل مميز: 5 فواتير مدفوعة (خدمات) أو أكثر في آخر 6 أشهر.</summary>
        public bool IsPremiumCustomer { get; set; }
    }
}
