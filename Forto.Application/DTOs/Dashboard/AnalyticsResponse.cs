using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Dashboard
{
    public class AnalyticsResponse
    {
        public int BranchId { get; set; }
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int TotalDoneItems { get; set; }

        public List<AnalyticsSliceDto> Items { get; set; } = new();
    }

}
