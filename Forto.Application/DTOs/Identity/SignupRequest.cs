using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Identity
{
    public class SignupRequest
    {
        public string PhoneNumber { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = ""; // washer/admin/cashier/client
    }

    public class SigninRequest
    {
        public string PhoneNumber { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class AuthResponse
    {
        public string Token { get; set; } = "";
        public string Role { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
    }

}
