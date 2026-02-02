using Forto.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Bookings.cashier.checkout
{
    public class CashierCheckoutRequest
    {
        public int BranchId { get; set; }
        public int CashierId { get; set; }
        /// <summary>معرف المشرف (اختياري).</summary>
        public int? SupervisorId { get; set; }

        public DateTime ScheduledStart { get; set; } // usually now rounded to hour

        public QuickClientDto Client { get; set; } = new();
        public QuickCarDto Car { get; set; } = new();

        public List<int> ServiceIds { get; set; } = new();

        public List<ServiceAssignmentDto>? ServiceAssignments { get; set; } // optional but recommended

        public List<PosInvoiceItemDto>? Products { get; set; } // optional products on the same invoice

        public string? Notes { get; set; }
    }

    public class QuickClientDto
    {
        public string PhoneNumber { get; set; } = "";
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class QuickCarDto
    {
        public string PlateNumber { get; set; } = "";
        public CarBodyType BodyType { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public int? Year { get; set; }
        public bool IsDefault { get; set; } = true;
    }

    public class ServiceAssignmentDto
    {
        public int ServiceId { get; set; }
        public int EmployeeId { get; set; }
    }

    public class PosInvoiceItemDto
    {
        public int ProductId { get; set; }
        public decimal Qty { get; set; }
    }

}
