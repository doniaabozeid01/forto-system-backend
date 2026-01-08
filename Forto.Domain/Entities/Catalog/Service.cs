using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Catalog
{
    public class Service : BaseEntity
    {
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string Name { get; set; } = "";
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<ServiceRate> Rates { get; set; } = new List<ServiceRate>();
    }
}
