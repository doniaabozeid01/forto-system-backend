using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Recipes
{
    public class ReviewMaterialChangeRequestDto
    {
        public int CashierId { get; set; }
        public string? Note { get; set; }
    }

}
