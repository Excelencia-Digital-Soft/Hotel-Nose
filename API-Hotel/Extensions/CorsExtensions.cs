namespace hotel.Extensions;

/// <summary>
/// Extension methods for CORS configuration
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Configure CORS policy
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "NuevaPolitica",
                builder =>
                {
                    builder
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        // Optimize for SignalR
                        .WithExposedHeaders("Connection", "Upgrade")
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
                }
            );

            // Alternative policy for production (more restrictive)
            options.AddPolicy(
                "ProductionCorsPolicy",
                builder =>
                {
                    builder
                        .WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:3001",
                            "https://excelencia.myiphost.com:86",
                            "https://excelencia.myiphost.com"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            );
        });

        return services;
    }
}
