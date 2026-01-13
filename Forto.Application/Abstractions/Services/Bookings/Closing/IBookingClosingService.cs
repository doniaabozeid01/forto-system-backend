using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Bookings.Closing
{
    public interface IBookingClosingService
    {
        Task TryAutoCloseBookingAsync(int bookingId, bool save = true);
    }
}
