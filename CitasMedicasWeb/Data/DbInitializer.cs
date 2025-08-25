using CitasMedicasWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace CitasMedicasWeb.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 1) Roles
            string[] roles = { "Admin", "Medico", "Paciente" };
            foreach (var r in roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            // 2) Usuario Admin
            var adminEmail = "admin@demo.com";
            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    NombreMostrado = "Administrador"
                };
                await userMgr.CreateAsync(admin, "Admin*123");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}