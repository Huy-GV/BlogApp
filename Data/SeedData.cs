using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlogApp.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {

            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var adminID = await EnsureAdmin(serviceProvider, "admin");
                await AssignRole(adminID, "Admin", serviceProvider);
            }
        }
        private static async Task<string> EnsureAdmin(IServiceProvider serviceProvider, string username)
        {
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = username,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, "Admin123@@");
            }
            if (user == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }

            return user.Id;
        }
        private static async Task<IdentityResult> AssignRole(string userID, string role, IServiceProvider serviceProvider)
        {
            IdentityResult identityResult;
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            if (roleManager == null)
            {
                throw new Exception("roleManager null");
            }

            if (!await roleManager.RoleExistsAsync(role))
            {
                identityResult = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var user = await userManager.FindByIdAsync(userID);
            if (user == null)
            {
                throw new Exception("The testUserPw password was probably not strong enough!");
            }

            identityResult = await userManager.AddToRoleAsync(user, role);

            return identityResult;
        }
    }
}
