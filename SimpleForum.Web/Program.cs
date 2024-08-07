using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using SimpleForum.Core.Data.Seeder;
using SimpleForum.Core.Extensions;
using SimpleForum.Core.Features;
using SimpleForum.Web.Middleware;
namespace SimpleForum.Web;

public class Program
{
    private const string DockerEnvName = "Docker";
    private const string DotnetRunningInContainerEnvVariable = "DOTNET_RUNNING_IN_CONTAINER";
    public static async Task Main(string[] args)
    {
        var builder = CreateHostBuilder(args);
        var app = builder.Build();

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await dataSeeder.SeedData();
        }

        if (builder.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndpointExtension();
        }
        else
        {
            app.UseExceptionHandler("/Error/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ErrorPageRoutingMiddleware>();

        app.MapRazorPages();
        app.MapBlazorHub();

        app.MapGet("/health", () => "Application Running");

        await app.RunAsync();
    }

    private static WebApplicationBuilder CreateHostBuilder(string[] args)
    {
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<Program>();

        var environmentName = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(DotnetRunningInContainerEnvVariable))
            ? DockerEnvName
            : Environments.Development;

        logger.LogInformation("Using environment '{env}'", environmentName);
        var webApplicationOptions = new WebApplicationOptions
        {
            EnvironmentName = environmentName,
            Args = args,
        };

        var builder = WebApplication.CreateBuilder(webApplicationOptions);
        if (environmentName == DockerEnvName)
        {
            // secrets are configured via environment variables in Docker
            logger.LogInformation("Adding environment variables");
            builder.Configuration.AddEnvironmentVariables();
        }

        builder.Services.UseCoreDataStore(builder.Configuration, environmentName);

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services
            .AddMvc()
            .AddRazorPagesOptions(options =>
            {
                options.Conventions.AddPageRoute("/Threads/Index", "");
            });

        builder.Services.AddAuthorization();
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.LoginPath = "/Authentication/Login";
            options.LogoutPath = "/Authentication/Logout";
        });

        builder.Services.AddFeatureManagement(builder.Configuration.GetSection(FeatureNames.SectionName));
        var featureFlags = builder.Configuration.GetSection(FeatureNames.SectionName);
        var useAwsS3 = bool.TryParse(featureFlags[FeatureNames.UseAwsS3], out var useAwsS3Option) && useAwsS3Option;
        if (useAwsS3)
        {
            logger.LogInformation("Using S3 as image store");
            builder.Services.UseCoreServicesWithS3(builder.Configuration);
        }
        else
        {
            logger.LogInformation("Using local image store");
            builder.Services.UseCoreServices(builder.Configuration);
        }

        var useHangFire = bool.TryParse(featureFlags[FeatureNames.UseHangFire], out var useHangFireOption) && useHangFireOption;
        if (useHangFire)
        {
            logger.LogInformation("Using HangFire as background job server");
            builder.Services.UseHangFireServer(builder.Configuration, environmentName);
        }
        else
        {
            logger.LogInformation("Background job disabled");
            builder.Services.UseFakeHangFireServer();
        }

        return builder;
    }
}
