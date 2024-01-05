using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorBlog.Data.Constants;
using RazorBlog.Models;

namespace RazorBlog.Data;

public static class SeedData
{
    public static async Task SeedProductionData(this IServiceProvider serviceProvider)
    {
        await using var context =
            new RazorBlogDbContext(serviceProvider.GetRequiredService<DbContextOptions<RazorBlogDbContext>>());

        await context.Database.MigrateAsync();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureRole(Roles.AdminRole, roleManager);
        await EnsureRole(Roles.ModeratorRole, roleManager);
        await EnsureAdminUser(userManager, configuration);
    }

    private static async Task EnsureAdminUser(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var userName = "admin";
        var password = configuration.GetValue<string>("SeedUser:Password")!;
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
            ProfileImageUri = @"/readonly/default.jpg",
            Description =
                "Lorem ipsum dolor sed temda met sedim ips dolor sed temda met sedim ips dolor sed temda met sedim ips"
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
