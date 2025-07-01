using hotel.Auth;
using hotel.Data;
using hotel.Interfaces;
using hotel.Jobs;
using hotel.Mapping;
using hotel.Models.Identity;
using hotel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace hotel.Extensions;

/// <summary>
/// Extension methods for service registration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register application services with dependency injection
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // Register JWT service
        services.AddScoped<JwtService>();

        // Register custom BCrypt password hasher
        services.AddScoped<IPasswordHasher<ApplicationUser>, BCryptPasswordHasher>();

        // Register authentication service
        services.AddScoped<IAuthService, AuthService>();

        // Register V1 services
        services.AddScoped<IHabitacionesService, HabitacionesService>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        return services;
    }

    /// <summary>
    /// Register database context
    /// </summary>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HotelDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        );

        return services;
    }

    /// <summary>
    /// Register background services and SignalR
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        // Configure Host Options for better handling of background service exceptions
        services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            options.ShutdownTimeout = TimeSpan.FromSeconds(15);
        });

        // Add Cron Job
        services.AddCronJob<MySchedulerJob>(options =>
        {
            options.CronExpression = "20 8 * * *";
            options.TimeZone = TimeZoneInfo.Local;
        });

        // Add SignalR and background services
        services.AddSignalR();
        services.AddSingleton<ReservationMonitorService>();
        services.AddHostedService<ReservationMonitorService>();

        return services;
    }
}