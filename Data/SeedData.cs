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

            using (var context = new RazorBlogDbContext(serviceProvider.GetRequiredService<DbContextOptions<RazorBlogDbContext>>()))
            {
                var adminID = await EnsureAdmin(serviceProvider, "admin");
                await AssignRole(adminID, Roles.AdminRole, serviceProvider);
                await CreateModeratorRole(serviceProvider);
                await AssignUserInfo(serviceProvider);
            }
        }
        private static async Task<string> EnsureAdmin(IServiceProvider serviceProvider, string username)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = username,
                    EmailConfirmed = true,
                    RegistrationDate = DateTime.UtcNow,
                    Country = "Australia"
                };
                await userManager.CreateAsync(user, "Admin123@@");
            }
            if (user == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }

            return user.Id;
        }
        private static async Task<IdentityResult> AssignRole(
            string userID, 
            string role, 
            IServiceProvider serviceProvider)
        {
            IdentityResult identityResult = null;
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            if (!await roleManager.RoleExistsAsync(role))
            {
                identityResult = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var user = await userManager.FindByIdAsync(userID);
            if (user == null)
            {
                throw new Exception("The password was probably not strong enough!");
            }

            if (!(await userManager.GetRolesAsync(user)).Contains(Roles.AdminRole))
            {
                identityResult = await userManager.AddToRoleAsync(user, role);
            }
            

            return identityResult;
        }
        private static async Task CreateModeratorRole(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync(Roles.ModeratorRole))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.ModeratorRole));
            }
        }

        // assign custom data to users who registered before this feature
        private async static Task AssignUserInfo(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetService<RazorBlogDbContext>();
            var users = userManager.Users.ToList();
            foreach (ApplicationUser user in users)
            {
                if (user.RegistrationDate == null)
                {
                    user.RegistrationDate = DateTime.Now;
                    context.Attach(user).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
            }
        }

    }
}
