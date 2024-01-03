using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazorBlog.Data;
using RazorBlog.Middleware;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = CreateHostBuilder(args);
        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            await scope.ServiceProvider.SeedProductionData();
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

    public static WebApplicationBuilder CreateHostBuilder(string[] args)
    {
        var environment = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"))
            ? "Docker"
            : Environments.Development;
        var webApplicationOptions = new WebApplicationOptions
        {
            EnvironmentName = environment,
            Args = args
        };

        var builder = WebApplication.CreateBuilder(webApplicationOptions);

        var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var dbLocation = builder.Configuration.GetConnectionString("DefaultLocation") ?? string.Empty;
        if (!string.IsNullOrEmpty(dbLocation))
        {
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

        builder.Services.AddScoped<IImageStorage, ImageLocalFileStorage>();
        builder.Services.AddScoped<IUserModerationService, UserModerationService>();
        builder.Services.AddScoped<IPostDeletionScheduler, PostDeletionScheduler>();
        builder.Services.AddScoped<IPostModerationService, PostModerationService>();
        builder.Services.AddScoped<IBlogContentManager, BlogContentManager>();
        builder.Services.AddScoped<ICommentContentManager, CommentContentManager>();
        builder.Services.AddScoped<IUserPermissionValidator, UserPermissionValidator>();

        return builder;
    }
}
