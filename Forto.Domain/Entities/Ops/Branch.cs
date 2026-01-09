using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Ops
{
    public class Branch : BaseEntity
    {
        public string Name { get; set; } = "";
        public int CapacityPerHour { get; set; } = 2;
        public bool IsActive { get; set; } = true;
    }
}
