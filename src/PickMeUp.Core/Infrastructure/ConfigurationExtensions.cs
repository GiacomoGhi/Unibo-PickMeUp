using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PickMeUp.Core.Database;
using PickMeUp.Core.Services.Auth;
using PickMeUp.Core.Services.Email;
using PickMeUp.Core.Services.User;
using PickMeUp.Core.Services.UserPickUpRequest;
using PickMeUp.Core.Services.UserTravel;
using RazorLight;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace PickMeUp.Core.Infrastructure;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds pick me up core services to the service collection.
    /// </summary>
    public static IServiceCollection AddPickMeUpCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserTravelService, UserTravelService>();
        services.AddScoped<IUserPickUpRequestService, UserPickUpRequestService>();
        services.AddScoped<IEmailService, EmailService>();

        // Configure RazorLight engine for email templates
        var templatesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Services", "Email", "Templates");
        services.AddSingleton<IRazorLightEngine>(sp =>
        {
            return new RazorLightEngineBuilder()
                .UseFileSystemProject(templatesPath)
                .UseMemoryCachingProvider()
                .Build();
        });

        return services;
    }

    /// <summary>
    /// Registers the PickMeUpDbContext with PostgreSQL configuration.
    /// </summary>
    public static IServiceCollection AddPickMeUpDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PickMeUp");

        services.AddDbContext<PickMeUpDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("PickMeUp.Core");
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        return services;
    }

    /// <summary>
    /// Setup pick me up core database.
    /// </summary>
    public static async Task<IServiceProvider> SetupDatabase(this IServiceProvider serviceProvider, bool isDevelopmentEnv)
    {
        // Get context
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PickMeUpDbContext>();

        // Ensure the database is created
        await dbContext.Database.EnsureCreatedAsync();

        // Seed data in dev environment
        if (isDevelopmentEnv)
        {
            await DataSeeder.SeedDataAsync(dbContext);
        }

        return serviceProvider;
    }
}
