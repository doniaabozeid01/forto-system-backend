using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings.Gifts
{
    public class InvoiceGiftOptionsResponse
    {
        public int InvoiceId { get; set; }
        public int BookingId { get; set; }

        public bool AlreadySelected { get; set; }
        public int? SelectedProductId { get; set; }

        public List<GiftOptionDto> Options { get; set; } = new();
    }

}
