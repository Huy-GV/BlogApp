using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RazorBlog.Core.Data;
using RazorBlog.Core.Data.Seeder;
using RazorBlog.Core.Models;
using RazorBlog.Core.Options;
using RazorBlog.Core.Services;
using System.IO;
using System;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace RazorBlog.Core.Extensions;
public static class ServiceCollectionsExtensions
{
    public static void UseCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddScoped<IUserModerationService, UserModerationService>();
        services.AddScoped<IPostDeletionScheduler, PostDeletionScheduler>();
        services.AddScoped<IPostModerationService, PostModerationService>();
        services.AddScoped<IBlogContentManager, BlogContentManager>();
        services.AddScoped<ICommentContentManager, CommentContentManager>();
        services.AddScoped<IUserPermissionValidator, UserPermissionValidator>();
        services.AddScoped<IBlogReader, BlogReader>();
        services.AddScoped<IAggregateImageUriResolver, AggregateImageUriResolver>();
        services.AddScoped<IDefaultProfileImageProvider, LocalImageStore>();


        var logger = LoggerFactory
            .Create(x => x.AddConsole())
            .CreateLogger(nameof(ServiceCollectionsExtensions));

        logger.LogInformation("Registering local image store");
        services.AddScoped<IImageStore, LocalImageStore>();
        services.AddScoped<IImageUriResolver, LocalImageUriResolver>();
    }

    public static void UseCoreServicesWithS3(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddScoped<IUserModerationService, UserModerationService>();
        services.AddScoped<IPostDeletionScheduler, PostDeletionScheduler>();
        services.AddScoped<IPostModerationService, PostModerationService>();
        services.AddScoped<IBlogContentManager, BlogContentManager>();
        services.AddScoped<ICommentContentManager, CommentContentManager>();
        services.AddScoped<IUserPermissionValidator, UserPermissionValidator>();
        services.AddScoped<IBlogReader, BlogReader>();
        services.AddScoped<IAggregateImageUriResolver, AggregateImageUriResolver>();
        services.AddScoped<IDefaultProfileImageProvider, LocalImageStore>();

        var logger = LoggerFactory
            .Create(x => x.AddConsole())
            .CreateLogger(nameof(ServiceCollectionsExtensions));

        logger.LogInformation("Registering AWS S3 image store");
        var awsOptions = new AWSOptions
        {
            Profile = configuration["Aws:Profile"],
            Region = Amazon.RegionEndpoint.GetBySystemName(configuration["Aws:Region"]),
        };

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonS3>();

        services
            .AddOptions<AwsOptions>()
            .Bind(configuration.GetRequiredSection(AwsOptions.Name))
            .ValidateOnStart()
            .ValidateDataAnnotations();

        services.AddScoped<IImageStore, S3ImageStore>();

        // register LocalImageUriResolver as a fallback
        services.AddScoped<IImageUriResolver, S3ImageUriResolver>();
        services.AddScoped<IImageUriResolver, LocalImageUriResolver>();
    }

    public static void UseCoreDataStore(this IServiceCollection services, IConfiguration configuration)
    {
        var logger = LoggerFactory
            .Create(x => x.AddConsole())
            .CreateLogger(nameof(ServiceCollectionsExtensions));

        var dbConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new InvalidOperationException("Connection string must not be null");
        }

        var dbLocation = configuration.GetConnectionString("DefaultLocation");
        if (!string.IsNullOrEmpty(dbLocation))
        {
            logger.LogInformation("Creating database directory '{directory}'", dbLocation);

            var dbDirectory = Path.GetDirectoryName(dbLocation)
                ?? throw new InvalidOperationException("Invalid DB directory name");
            Directory.CreateDirectory(dbDirectory);
            dbConnectionString = $"{dbConnectionString}AttachDbFileName={dbLocation};";
        }

        Action<SqlServerDbContextOptionsBuilder> buildSqlServerOptions = sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(3),
                errorNumbersToAdd: null
            );
        };

        services.AddDbContext<RazorBlogDbContext>(
            options => options.UseSqlServer(dbConnectionString, buildSqlServerOptions)
        );

        // for use in Blazor components as injected DB context is not scoped
        services.AddDbContextFactory<RazorBlogDbContext>(
            options => options.UseSqlServer(dbConnectionString, buildSqlServerOptions),

            // use Scoped lifetime as the injected DbContextOptions used by AddDbContext also has a Scoped lifetime
            lifetime: ServiceLifetime.Scoped
        );

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
    }
}
