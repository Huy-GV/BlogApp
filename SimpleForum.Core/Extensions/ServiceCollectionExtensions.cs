using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleForum.Core.Data;
using SimpleForum.Core.Data.Seeder;
using SimpleForum.Core.Models;
using SimpleForum.Core.Options;
using System.IO;
using System;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using SimpleForum.Core.QueryServices;
using SimpleForum.Core.CommandServices;

namespace SimpleForum.Core.Extensions;
public static class ServiceCollectionsExtensions
{
    private static void RegisterSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddScoped<IUserModerationService, UserModerationService>();
        services.AddScoped<IPostDeletionScheduler, PostDeletionScheduler>();
        services.AddScoped<IPostModerationService, PostModerationService>();
        services.AddScoped<IThreadContentManager, ThreadContentManager>();
        services.AddScoped<ICommentContentManager, CommentContentManager>();
        services.AddScoped<IUserPermissionValidator, UserPermissionValidator>();
        services.AddScoped<IThreadReader, ThreadReader>();
        services.AddScoped<ICommentReader, CommentReader>();
        services.AddScoped<IUserProfileReader, UserProfileReader>();
        services.AddScoped<IUserProfileEditor, UserProfileEditor>();
        services.AddScoped<IBanTicketReader, BanTicketReader>();
        services.AddScoped<IAggregateImageUriResolver, AggregateImageUriResolver>();
        services.AddScoped<IDefaultProfileImageProvider, DefaultProfileImageProvider>();
        services
            .AddOptions<DefaultProfileImageOptions>()
            .Bind(configuration.GetRequiredSection(DefaultProfileImageOptions.SectionName))
            .ValidateOnStart()
            .ValidateDataAnnotations();
    }

    public static void UseCoreServices(
        this IServiceCollection services,
       IConfiguration configuration)
    {
        services.RegisterSharedServices(configuration);
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
        services.RegisterSharedServices(configuration);

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
            .Bind(configuration.GetRequiredSection(AwsOptions.SectionName))
            .ValidateOnStart()
            .ValidateDataAnnotations();

        services.AddScoped<IImageStore, S3ImageStore>();

        // register LocalImageUriResolver as a fallback
        services.AddScoped<IImageUriResolver, S3ImageUriResolver>();
        services.AddScoped<IImageUriResolver, LocalImageUriResolver>();
    }

    public static void UseHangFireServer(this IServiceCollection services, IConfiguration configuration)
    {
        var logger = LoggerFactory
            .Create(x => x.AddConsole())
            .CreateLogger(nameof(ServiceCollectionsExtensions));

        var connectionString = ResolveConnectionString(configuration, "DefaultConnection", "DefaultLocation", logger);
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(connectionString);
        });

        services.AddHangfireServer(options =>
        {
            options.SchedulePollingInterval = TimeSpan.FromMinutes(1);
        });
    }

    public static void UseFakeHangFireServer(this IServiceCollection services)
    {
        services.AddTransient<IBackgroundJobClient, FakeBackgroundJobClient>();
    }

    public static void UseCoreDataStore(this IServiceCollection services, IConfiguration configuration)
    {
        var logger = LoggerFactory
            .Create(x => x.AddConsole())
            .CreateLogger(nameof(ServiceCollectionsExtensions));

        var dbConnectionString = ResolveConnectionString(configuration, "DefaultConnection", "DefaultLocation", logger);

        services.AddDbContext<SimpleForumDbContext>(
            options => options.UseSqlServer(dbConnectionString)
        );

        // for use in Blazor components as injected DB context is not scoped
        services.AddDbContextFactory<SimpleForumDbContext>(
            options => options.UseSqlServer(dbConnectionString),

            // use Scoped lifetime as the injected DbContextOptions used by AddDbContext also has a Scoped lifetime
            lifetime: ServiceLifetime.Scoped
        );

        services.AddDataProtection().PersistKeysToDbContext<SimpleForumDbContext>();
        services.AddDatabaseDeveloperPageExceptionFilter();

        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<SimpleForumDbContext>()
            .AddDefaultTokenProviders();

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

    private static string ResolveConnectionString(
        IConfiguration configuration,
        string connectionString,
        string location,
        ILogger logger)
    {
        var dbConnectionString = configuration.GetConnectionString(connectionString);
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new InvalidOperationException("Connection string must not be null");
        }

        var dbLocation = configuration.GetConnectionString(location);
        if (string.IsNullOrEmpty(dbLocation))
        {
            return dbConnectionString;
        }
        
        logger.LogInformation("Creating database directory '{directory}'", dbLocation);
        var dbDirectory = Path.GetDirectoryName(dbLocation)
                          ?? throw new InvalidOperationException("Invalid DB directory name");
        Directory.CreateDirectory(dbDirectory);
        dbConnectionString = $"{dbConnectionString}AttachDbFileName={dbLocation};";

        return dbConnectionString;
    }
}
