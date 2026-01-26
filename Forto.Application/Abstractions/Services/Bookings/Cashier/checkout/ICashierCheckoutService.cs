using Forto.Application.DTOs.Billings;
using Forto.Application.DTOs.Bookings.cashier.checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Bookings.Cashier.checkout
{
    public interface ICashierCheckoutService
    {
        Task<InvoiceResponse> CheckoutNowAsync(CashierCheckoutRequest request);
    }

}
