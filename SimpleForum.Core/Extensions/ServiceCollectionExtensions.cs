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
using Microsoft.Extensions.Hosting;

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

    public static void UseHangFireServer(this IServiceCollection services, IConfiguration configuration, string environment)
    {
        var logger = LoggerFactory
            .Create(x => x.AddConsole())
            .CreateLogger(nameof(ServiceCollectionsExtensions));

        var dbConnectionString = environment != Environments.Development
            ? BuildConnectionString(configuration, "UserId", "Password", "Endpoint", "DatabaseName")
            : configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string must not be null");

        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(dbConnectionString);
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

    public static void UseCoreDataStore(this IServiceCollection services, IConfiguration configuration, string environment)
    {
        var logger = LoggerFactory
            .Create(x => x.AddConsole())
            .CreateLogger(nameof(ServiceCollectionsExtensions));

        var dbConnectionString = environment != Environments.Development
            ? BuildConnectionString(configuration, "UserId", "Password", "Endpoint", "DatabaseName")
            : configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string must not be null");

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

    private static string BuildConnectionString(
        IConfiguration configuration,
        string userIdField,
        string passwordField,
        string endpointField,
        string databaseNameField)
    {
        var userId = configuration.GetConnectionString(userIdField)
            ?? throw new InvalidOperationException("Database User ID must not be null");
        var password = configuration.GetConnectionString(passwordField)
            ?? throw new InvalidOperationException("Database Password string must not be null");
        var endpoint = configuration.GetConnectionString(endpointField)
            ?? throw new InvalidOperationException("Database Endpoint string must not be null");
        var databaseName = configuration.GetConnectionString(databaseNameField)
            ?? throw new InvalidOperationException("Database Name string must not be null");

        return $"Server={endpoint},1433;Database={databaseName};User ID={userId};Password={password};MultipleActiveResultSets=false;TrustServerCertificate=True";
    }
}
