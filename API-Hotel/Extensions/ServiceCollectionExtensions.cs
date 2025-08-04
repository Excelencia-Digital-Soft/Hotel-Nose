using hotel.Auth;
using hotel.Data;
using hotel.Interfaces;
using hotel.Jobs;
using hotel.Mapping;
using hotel.Models.Identity;
using hotel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
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

        // Register mappers
        services.AddScoped<ICajaDtoMapper, CajaDtoMapper>();

        // Register V1 services
        services.AddScoped<IArticulosService, ArticulosService>();
        services.AddScoped<ICajaService, CajaService>();
        services.AddScoped<ICategoriasService, CategoriasService>();
        services.AddScoped<IConsumosService, ConsumosService>();
        services.AddScoped<IHabitacionesService, HabitacionesService>();
        services.AddScoped<IMovimientosService, MovimientosService>();
        services.AddScoped<IPromocionesService, PromocionesService>();
        services.AddScoped<IRegistrosService, RegistrosService>();
        services.AddScoped<IReservasService, ReservasService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IUserConsumptionService, UserConsumptionService>();
        services.AddScoped<IVisitasService, VisitasService>();

        // Register notification services
        services.AddSingleton<INotificationService, SignalRNotificationService>();
        services.AddScoped<IReservationNotificationService, ReservationNotificationService>();

        // Register specialized inventory services (following Single Responsibility Principle)
        services.AddScoped<IInventoryCoreService, InventoryCoreService>();
        services.AddScoped<IInventoryValidationService, InventoryValidationService>();
        services.AddScoped<IInventoryReportingService, InventoryReportingService>();
        services.AddScoped<IInventoryMovementService, InventoryMovementService>();
        services.AddScoped<IInventoryAlertService, InventoryAlertService>();
        services.AddScoped<IInventoryTransferService, InventoryTransferService>();

        // Keep backward compatibility for legacy controllers that still use IInventoryService
        services.AddScoped<IInventoryService, InventoryUnifiedServiceRefactored>();

        return services;
    }

    /// <summary>
    /// Configure API versioning
    /// </summary>
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Register database context
    /// </summary>
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
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
        // Configure graceful shutdown timeout for background services
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(15);
        });

        // Reservation expiration monitoring job (every 5 minutes)
        services.AddCronJob<ReservationExpirationJob>(options =>
        {
            options.CronExpression = "*/5 * * * *"; // Every 5 minutes
            options.TimeZone = TimeZoneInfo.Local;
        });

        // Add SignalR
        services.AddSignalR();

        return services;
    }
}

