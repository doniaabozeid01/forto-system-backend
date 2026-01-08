using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Catalog.Categories
{
    public class CreateCategoryRequest
    {
        [Required, MinLength(2)]
        public string Name { get; set; } = "";

        public int? ParentId { get; set; }
    }
}
