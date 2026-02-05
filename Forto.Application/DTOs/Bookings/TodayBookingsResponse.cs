using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Enum;

namespace Forto.Application.DTOs.Bookings
{
    public class TodayBookingsResponse
    {
        public DateOnly Date { get; set; }
        public int BranchId { get; set; }

        public List<BookingListItemDto> Pending { get; set; } = new();
        public List<BookingListItemDto> Active { get; set; } = new();
        public List<BookingListItemDto> Completed { get; set; } = new();
        public List<BookingListItemDto> Cancelled { get; set; } = new();
    }

    public class BookingListItemDto
    {
        public int BookingId { get; set; }
        public DateTime ScheduledStart { get; set; }
        public BookingStatus Status { get; set; }

        public int ClientId { get; set; }
        public string ClientName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        public int CarId { get; set; }
        public string PlateNumber { get; set; } = "";
        public string CarModel { get; set; } = "";

        public decimal TotalPrice { get; set; }
        public int ServicesCount { get; set; }
        public List<BookingServiceLineDto> Services { get; set; } = new();

        /// <summary>معرف الفاتورة لو الفاتورة موجودة للـ booking.</summary>
        public int? InvoiceId { get; set; }
        /// <summary>هل الفاتورة مدفوعة (Paid). لو مفيش فاتورة = false.</summary>
        public bool IsInvoicePaid { get; set; }
    }



    public class BookingServiceLineDto
    {
        public int BookingItemId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";

        public decimal UnitPrice { get; set; }
        public int DurationMinutes { get; set; }
        public BookingItemStatus Status { get; set; }

        public int? AssignedEmployeeId { get; set; }
    }

}
