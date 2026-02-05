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

        /// <summary>عميل مميز: 5 فواتير مدفوعة (خدمات) أو أكثر في آخر 6 أشهر.</summary>
        public bool IsPremiumCustomer { get; set; }
        /// <summary>عدد الفواتير المدفوعة من حجوزات (خدمات) في آخر 6 أشهر.</summary>
        public int PaidServiceInvoicesCountLast6Months { get; set; }
    }


    public class ClientCarResponse
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = "";
        public CarBodyType BodyType { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public int? Year { get; set; }
        // أو أحيانًا int
        public bool IsDefault { get; set; }
    }

}
