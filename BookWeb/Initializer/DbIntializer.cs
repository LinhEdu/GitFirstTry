using BookWeb.Data;
using BookWeb.Models;
using BookWeb.Contast;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Initializer;

public static class DbInitializer
{
    public static void Initialize(IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            var _userManager =
                serviceScope.ServiceProvider.GetService<UserManager<IdentityUser>>();
            var _roleManager =
                serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
            try
            {
                if (context.Database.GetPendingMigrations().Count() > 0)
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
            }

            if (_roleManager.FindByNameAsync(SD.Admin_Role).Result == null)
            {
                var adminRole = new IdentityRole(SD.Admin_Role);
                _roleManager.CreateAsync(adminRole).GetAwaiter().GetResult();
                var customerRole = new IdentityRole(SD.Customer_Role);
                _roleManager.CreateAsync(customerRole).GetAwaiter().GetResult();
                var storeOwner = new IdentityRole(SD.StoreOwner_Role);
                _roleManager.CreateAsync(storeOwner).GetAwaiter().GetResult();
            }
            else
            {
                return;
            }

            // user admin
            User adminUser = new User()
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "111111111111",
                FullName = "Admin"
            };

            _userManager.CreateAsync(adminUser, "Admin123@").GetAwaiter().GetResult();

            var userAdmin = context.Users.Where(u => u.Email == "admin@gmail.com").FirstOrDefault();

            _userManager.AddToRoleAsync(userAdmin, SD.Admin_Role).GetAwaiter().GetResult();
        }
    }
}