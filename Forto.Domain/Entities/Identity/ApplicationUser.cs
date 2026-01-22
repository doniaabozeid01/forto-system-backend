using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Forto.Domain.Entities.Identity
{

    public class ApplicationUser : IdentityUser
    {
        // Optional: link to your CRM Client / HR Employee later
        //public int? EmployeeId { get; set; }
        //public int? ClientId { get; set; }
    }

}
