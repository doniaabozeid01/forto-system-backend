using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Identity;
using Forto.Domain.Entities.Identity;
using Forto.Domain.Entities.Employees;
using Microsoft.EntityFrameworkCore;

namespace Forto.Api.Controllers
{
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IUnitOfWork uow,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _uow = uow;
            _config = config;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest req)
        {
            var phone = req.PhoneNumber.Trim();
            var role = req.Role.Trim().ToLowerInvariant();

            // basic role validation
            var allowed = new[] { "washer", "admin", "cashier", "client" };
            if (!allowed.Contains(role))
                return BadRequest(new { message = "Invalid role" });

            // ⚠️ important: protect creating admin/cashier/washer
            // MVP rule: allow only client signup publicly.
            // You can later restrict with Admin-only endpoint.
            if (role != "client")
                return Forbid();

            var existing = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (existing != null)
                return Conflict(new { message = "Phone already registered" });

            var user = new ApplicationUser
            {
                UserName = phone,           // use phone as username
                PhoneNumber = phone,
                PhoneNumberConfirmed = true // you can switch to OTP later
            };

            var create = await _userManager.CreateAsync(user, req.Password);
            if (!create.Succeeded)
                return BadRequest(create.Errors);

            await _userManager.AddToRoleAsync(user, role);

            var token = await CreateJwtAsync(user, role);
            return Ok(new AuthResponse { Token = token, Role = role, PhoneNumber = phone });
        }

        [HttpPost("signin")]
        public async Task<IActionResult> Signin([FromBody] SigninRequest req)
        {
            var phone = req.PhoneNumber.Trim();

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (user == null)
                return Unauthorized(new { message = "رقم الهاتف او كلمه المرور غير صحيحه" });

            var ok = await _userManager.CheckPasswordAsync(user, req.Password);
            if (!ok)
                return Unauthorized(new { message = "رقم الهاتف او كلمه المرور غير صحيحه" });

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "client";

            // اسم الموظف و EmployeeId إن وُجد (للدخول برقم التليفون)
            string? fullName = null;
            int? employeeId = null;
            var empRepo = _uow.Repository<Employee>();
            var employee = (await empRepo.FindAsync(e => e.UserId == user.Id)).FirstOrDefault();
            if (employee != null)
            {
                fullName = employee.Name;
                employeeId = employee.Id;
            }

            var token = await CreateJwtAsync(user, role);
            return Ok(new AuthResponse
            {
                Token = token,
                Role = role,
                PhoneNumber = phone,
                FullName = fullName,
                EmployeeId = employeeId
            });
        }

        private Task<string> CreateJwtAsync(ApplicationUser user, string role)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.PhoneNumber ?? ""),
            new Claim(ClaimTypes.Role, role),
            new Claim("phone", user.PhoneNumber ?? "")
        };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }

}
