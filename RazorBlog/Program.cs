using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RazorBlog.Data;
using RazorBlog.Data.Seeder;
using RazorBlog.Middleware;
using RazorBlog.Models;
using RazorBlog.Options;
using RazorBlog.Services;

namespace RazorBlog;

public class Program
{
    private const string DockerEnvName = "Docker";
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
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ErrorPageRoutingMiddleware>();

        app.MapRazorPages();
        app.MapBlazorHub();

        await app.RunAsync();
    }

    private static WebApplicationBuilder CreateHostBuilder(string[] args)
    {
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<Program>();

        var environmentName = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"))
            ? DockerEnvName
            : Environments.Development;

        var webApplicationOptions = new WebApplicationOptions
        {
            EnvironmentName = environmentName,
            Args = args,
        };

        var builder = WebApplication.CreateBuilder(webApplicationOptions);
        if (environmentName == DockerEnvName)
        {
            // secrets are configured via environment variables in Docker
            builder.Configuration.AddEnvironmentVariables();
        }

        var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var dbLocation = builder.Configuration.GetConnectionString("DefaultLocation") ?? string.Empty;
        if (!string.IsNullOrEmpty(dbLocation))
        {
            logger.LogInformation("Creating database directory '{directory}'", dbLocation);

            var dbDirectory = Path.GetDirectoryName(dbLocation)
                ?? throw new InvalidOperationException("Invalid DB directory name");
            Directory.CreateDirectory(dbDirectory);
            dbConnectionString = $"{dbConnectionString}AttachDbFileName={dbLocation};";
        }

        builder.Services.AddDbContext<RazorBlogDbContext>(options => options.UseSqlServer(dbConnectionString));

        // for use in Blazor components as injected DB context is not scoped
        builder.Services.AddDbContextFactory<RazorBlogDbContext>(options =>
            options.UseSqlServer(dbConnectionString),

            // use Scoped lifetime as the injected DbContextOptions used by AddDbContext also has a Scoped lifetime
            lifetime: ServiceLifetime.Scoped
        );

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<RazorBlogDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(dbConnectionString);
        });

        builder.Services.AddHangfireServer(options =>
        {
            options.SchedulePollingInterval = TimeSpan.FromMinutes(1);
        });

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services
            .AddMvc()
            .AddRazorPagesOptions(options =>
            {
                options.Conventions.AddPageRoute("/Blogs/Index", "");
            });

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
            options.User.RequireUniqueEmail = false;
            options.SignIn.RequireConfirmedEmail = false;
        });

        builder.Services.AddAuthorization();
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.LoginPath = "/Authentication/Login";
            options.LogoutPath = "/Authentication/Logout";
        });

        builder.Services.AddScoped<IDataSeeder, DataSeeder>();
        
        builder.Services.AddScoped<IUserModerationService, UserModerationService>();
        builder.Services.AddScoped<IPostDeletionScheduler, PostDeletionScheduler>();
        builder.Services.AddScoped<IPostModerationService, PostModerationService>();
        builder.Services.AddScoped<IBlogContentManager, BlogContentManager>();
        builder.Services.AddScoped<ICommentContentManager, CommentContentManager>();
        builder.Services.AddScoped<IUserPermissionValidator, UserPermissionValidator>();
        builder.Services.AddScoped<IBlogReader, BlogReader>();
        builder.Services.AddScoped<IAggregateImageUriResolver, AggregateImageUriResolver>();
        builder.Services.AddScoped<IDefaultProfileImageProvider, LocalImageStore>();

        var useAwsS3 = bool.TryParse(builder.Configuration["UseAwsS3"], out var result) && result;
        if (useAwsS3)
        {
            logger.LogInformation("Registering AWS S3 image store");
            var awsOptions = new AWSOptions
            {
                Credentials = new BasicAWSCredentials(
                    builder.Configuration["Aws:AccessKey"],
                    builder.Configuration["Aws:SecretKey"]),
                Region = Amazon.RegionEndpoint.APSoutheast2,
            };

            builder.Services.AddDefaultAWSOptions(awsOptions);
            builder.Services.AddAWSService<IAmazonS3>();

            builder.Services
                .AddOptions<AwsS3Options>()
                .Bind(builder.Configuration.GetRequiredSection($"Aws:{AwsS3Options.Name}"))
                .ValidateOnStart()
                .ValidateDataAnnotations();

            builder.Services.AddScoped<IImageStore, S3ImageStore>();
            
            // register LocalImageUriResolver as a fallback
            builder.Services.AddScoped<IImageUriResolver, S3ImageUriResolver>();
            builder.Services.AddScoped<IImageUriResolver, LocalImageUriResolver>();
        }
        else
        {
            logger.LogInformation("Registering local image store");
            builder.Services.AddScoped<IImageStore, LocalImageStore>();
            builder.Services.AddScoped<IImageUriResolver, LocalImageUriResolver>();
        }

        return builder;
    }
}
