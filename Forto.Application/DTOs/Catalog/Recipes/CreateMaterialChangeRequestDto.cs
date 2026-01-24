using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Recipes
{
    public class CreateMaterialChangeRequestDto
    {
        public int EmployeeId { get; set; }
        public List<MaterialProposedDto> Materials { get; set; } = new();
    }

    public class MaterialProposedDto
    {
        public int MaterialId { get; set; }
        public decimal ProposedActualQty { get; set; }
    }

}
