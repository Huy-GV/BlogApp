using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Extensions.Configuration;
using RazorBlog.Core.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using RazorBlog.Web;
using Bogus;
using RazorBlog.Core.Models;
using Microsoft.AspNetCore.Identity;
using FluentAssertions;

namespace RazorBlog.IntegrationTest.Factories;

public class RazorBlogApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string DatabaseName = "RazorBlogIntegrationTestDatabase";
    private const string Username = "sa";
    private const ushort MsSqlPort = 1433;
    private readonly IContainer _mssqlContainer;
    private readonly string _databasePassword;

    public RazorBlogApplicationFactory()
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

        // use the admin user password as the test database password
        _databasePassword = configuration["SeedUser:Password"]!;

        _mssqlContainer = new ContainerBuilder()
            .WithName($"RazorBlogTest-{Guid.NewGuid()}")
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(MsSqlPort, assignRandomHostPort: true)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_SA_PASSWORD", _databasePassword)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MsSqlPort))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mssqlContainer.StartAsync();
        await SeedUsers();
    }

    private async Task SeedUsers()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RazorBlogDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var faker = new Faker();
        for (int i = 0; i < 30; i++)
        {
            var user = new ApplicationUser
            {
                UserName = $"{faker.Name.LastName().Replace("'", string.Empty)}{faker.Random.UShort()}",
                RegistrationDate = faker.Date.Past()
            };

            if (userManager.Users.Any(x => x.UserName == user.UserName))
            {
                continue;
            }

            var createUserResult = await userManager.CreateAsync(user, "TestPassword123@@");
            createUserResult.Succeeded.Should().BeTrue(string.Join("\n", createUserResult.Errors.Select(x => x.Description)));
            if (i % 2 == 0)
            {
                var createRoleResult = await userManager.AddToRoleAsync(user, "moderator");
                createRoleResult.Succeeded.Should().BeTrue(string.Join("\n", createRoleResult.Errors.Select(x => x.Description)));
            }
        }
    }

    public new async Task DisposeAsync()
    {
        await _mssqlContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var host = _mssqlContainer.Hostname;
        var port = _mssqlContainer.GetMappedPublicPort(MsSqlPort);
        builder.ConfigureServices(services =>
        {
            services.Remove(services.First(descriptor => descriptor.ServiceType == typeof(DbContextOptions<RazorBlogDbContext>)));

            services.AddDbContext<RazorBlogDbContext>(options =>
                options.UseSqlServer($"Server={host},{port};Database={DatabaseName};User Id={Username};Password={_databasePassword};TrustServerCertificate=True"));

            services.AddRazorPages();
        });

    }
}
