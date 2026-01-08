using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Domain.Entities.Catalog
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = "";
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();

        public bool IsActive { get; set; } = true;

        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
