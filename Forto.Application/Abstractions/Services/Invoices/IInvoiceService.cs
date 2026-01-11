using Forto.Application.DTOs.Billings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Invoices
{
    public interface IInvoiceService
    {
        Task<InvoiceResponse?> GetByBookingIdAsync(int bookingId);
        Task<InvoiceResponse> EnsureInvoiceForBookingAsync(int bookingId); // create if not exists
        Task<InvoiceResponse> PayCashAsync(int invoiceId, int cashierId);

        Task RecalculateForBookingAsync(int bookingId, bool save = true); // لو اتلغت خدمات قبل الدفع
    }
}
