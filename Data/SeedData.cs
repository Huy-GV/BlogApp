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
                await AssignAdminRole(adminID, Roles.AdminRole, serviceProvider);
                await CreateModeratorRole(serviceProvider);
            }
        }
        private static async Task<string> EnsureAdmin(IServiceProvider serviceProvider, string username)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByNameAsync(username);

            if (user != null)
                await userManager.DeleteAsync(user);

            user = new ApplicationUser
            {
                UserName = username,
                EmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                Country = "Australia",
                ProfilePicture = "default.jpg"
            };
            await userManager.CreateAsync(user, "Admin123@@");

            return user.Id;
        }
        private static async Task AssignAdminRole(
            string userID, 
            string role, 
            IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

            var user = await userManager.FindByIdAsync(userID);
            if (user == null)
                throw new Exception("User was not found");

            if (!(await userManager.GetRolesAsync(user)).Contains(Roles.AdminRole))
                await userManager.AddToRoleAsync(user, role);
            
        }
        private static async Task CreateModeratorRole(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync(Roles.ModeratorRole))
                await roleManager.CreateAsync(new IdentityRole(Roles.ModeratorRole));
        }
    }
}
