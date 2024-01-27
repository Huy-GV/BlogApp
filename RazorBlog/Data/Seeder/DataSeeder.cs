using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RazorBlog.Data.Constants;
using RazorBlog.Models;

namespace RazorBlog.Data.Seeder;

internal class DataSeeder : IDataSeeder
{
    private readonly RazorBlogDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        RazorBlogDbContext dbContext,
        IConfiguration configuration,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<DataSeeder> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedData()
    {
        _logger.LogInformation("Migrating database");
        await _dbContext.Database.MigrateAsync();

        await EnsureRole(Roles.AdminRole);
        await EnsureRole(Roles.ModeratorRole);
        await EnsureAdminUser();
    }

    private async Task EnsureAdminUser()
    {
        _logger.LogInformation("Ensuring admin user exists");
        const string userName = "admin";
        var password = _configuration["SeedUser:Password"]!;
        var user = await _userManager.FindByNameAsync(userName);

        if (user != null)
        {
            if (!await _userManager.IsInRoleAsync(user, Roles.AdminRole))
            {
                await _userManager.AddToRoleAsync(user, Roles.AdminRole);
            }

            return;
        }

        user = new ApplicationUser
        {
            UserName = userName,
            EmailConfirmed = true,
            ProfileImageUri = @"images/readonly/default.jpg",
            Description =
                "Lorem ipsum dolor sed temda met sedim ips dolor sed temda met sedim ips dolor sed temda met sedim ips"
        };

        _logger.LogInformation("Creating admin user");
        await _userManager.CreateAsync(user, password);
        await _userManager.AddToRoleAsync(user, Roles.AdminRole);
    }

    private async Task EnsureRole(string roleName)
    {
        _logger.LogInformation("Ensuring role {roleName} exists", roleName);
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            _logger.LogInformation("Creating new role: {roleName}", roleName);
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
