using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Inventory.MaterialsCheck
{
    public class MaterialsCheckResponse
    {
        public int BranchId { get; set; }
        public int ServiceCount { get; set; }
        public int MaterialCount { get; set; }

        public bool IsAvailable { get; set; }
        public List<ServiceBriefDto> Services { get; set; } = new();

        public List<MaterialRequirementDto> Required { get; set; } = new();
        public List<MaterialRequirementDto> Missing { get; set; } = new();
    }
}
