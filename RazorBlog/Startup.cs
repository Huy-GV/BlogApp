using System;
using System.IO;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazorBlog.Data;
using RazorBlog.Models;
using RazorBlog.Services;

namespace RazorBlog;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        var dbConnectionString = Configuration.GetConnectionString("DefaultConnection");
        var dbLocation = Configuration.GetConnectionString("DefaultLocation") ?? string.Empty;
        if (!string.IsNullOrEmpty(dbLocation))
        {
            var dbDirectory = Path.GetDirectoryName(dbLocation)
                ?? throw new InvalidOperationException("Invalid DB directory name");
            Directory.CreateDirectory(dbDirectory);
            dbConnectionString = $"{dbConnectionString}AttachDbFileName={dbLocation};";
        }

        services
            .AddDbContext<RazorBlogDbContext>(
            options => options.UseSqlServer(dbConnectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();
        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<RazorBlogDbContext>()
            .AddDefaultTokenProviders();

        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(dbConnectionString);
        });

        services.AddHangfireServer(options =>
        {
            options.SchedulePollingInterval = TimeSpan.FromMinutes(1);
        });

        services.AddRazorPages();
        services.AddServerSideBlazor(); 
        services
            .AddMvc()
            .AddRazorPagesOptions(
            options => { options.Conventions.AddPageRoute("/Blogs/Index", ""); });

        services.Configure<IdentityOptions>(options =>
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

        services.AddAuthorization();
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.LoginPath = "/Authentication/Login";
            options.LogoutPath = "/Authentication/Logout";
        });

        services.AddScoped<IImageStorage, ImageLocalFileStorage>();
        services.AddScoped<IUserModerationService, UserModerationService>();
        services.AddScoped<IPostDeletionScheduler, PostDeletionScheduler>();
        services.AddScoped<IPostModerationService, PostModerationService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => 
        { 
            endpoints.MapRazorPages();
            endpoints.MapBlazorHub();
        });
    }
}