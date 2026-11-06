using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Winery.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider 
                .GetRequiredService<DbContext>();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Seed Roles
            await SeedRolesAsync(roleManager);
        }

        private static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Customer", "Manager" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }
    }
}