using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BlogApp.Data.Constants;

namespace BlogApp.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new RazorBlogDbContext(serviceProvider.GetRequiredService<DbContextOptions<RazorBlogDbContext>>());
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await EnsureRole(Roles.AdminRole, roleManager);
            await EnsureRole(Roles.ModeratorRole, roleManager);
            await EnsureAdminUser(userManager);
        }

        private static async Task EnsureAdminUser(UserManager<ApplicationUser> userManager)
        {
            var (userName, password) = ("admin", "Admin123@@");
            var user = await userManager.FindByNameAsync(userName);

            if (user != null)
            {
                if (!await userManager.IsInRoleAsync(user, Roles.AdminRole))
                {
                    await userManager.AddToRoleAsync(user, Roles.AdminRole);
                }

                return;
            }

            user = new ApplicationUser
            {
                UserName = userName,
                EmailConfirmed = true,
                ProfileImageUri = @"/ProfileImage/default.jpg",
                Description = "Lorem ipsum dolor sed temda met sedim ips dolor sed temda met sedim ips dolor sed temda met sedim ips"
            };

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, Roles.AdminRole);
        }

        private static async Task EnsureRole(string roleName, RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}