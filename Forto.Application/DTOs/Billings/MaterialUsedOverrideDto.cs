using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Billings
{
    public class MaterialUsedOverrideDto
    {
        public int MaterialId { get; set; }
        public decimal ActualQty { get; set; }
    }
}
