using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Recipes
{
    public class PendingMaterialRequestDto
    {
        public int RequestId { get; set; }
        public int BookingItemId { get; set; }
        public int BookingId { get; set; }
        public DateTime ScheduledStart { get; set; }

        public int RequestedByEmployeeId { get; set; }
        public string RequestedByEmployeeName { get; set; } = "";

        public DateTime RequestedAt { get; set; }

        public List<PendingMaterialRequestLineDto> Lines { get; set; } = new();
    }

    public class PendingMaterialRequestLineDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = "";
        public decimal DefaultQty { get; set; }
        public decimal CurrentActualQty { get; set; }
        public decimal ProposedActualQty { get; set; }
    }

}
