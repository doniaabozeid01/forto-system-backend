    using Forto.Domain.Entities.Identity;
    using Microsoft.AspNetCore.Identity;

namespace Forto.Api.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            string[] roles = { "washer", "admin", "cashier", "client", "worker" };

            foreach (var r in roles)
                if (!await roleManager.RoleExistsAsync(r))
                    await roleManager.CreateAsync(new ApplicationRole { Name = r });
        }
    }

}
