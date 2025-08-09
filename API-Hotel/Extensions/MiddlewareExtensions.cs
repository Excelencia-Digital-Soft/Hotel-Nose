using hotel.Hubs.V1;

namespace hotel.Extensions;

/// <summary>
/// Extension methods for middleware pipeline configuration
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Configure the HTTP request pipeline
    /// </summary>
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        // Development-specific middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerConfiguration();
        }

        // Static files (images, CSS, JavaScript, etc.)
        app.UseStaticFiles();

        // HTTPS redirection (commented out as per original)
        // app.UseHttpsRedirection();

        // CORS
        app.UseCors("NuevaPolitica");

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Controllers
        app.MapControllers();

        // SignalR V1 Hub with authentication and optimized transport configuration
        app.MapHub<NotificationsHub>("/api/v1/notifications", options =>
        {
            // Configure transports for better compatibility
            options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
            
            // Prevent long polling which can block HTTP connections
            // options.Transports does not include LongPolling
            
            // Configure application maximum buffer size
            options.ApplicationMaxBufferSize = 32 * 1024; // 32KB
            options.TransportMaxBufferSize = 32 * 1024; // 32KB
        })
        .RequireAuthorization();

        return app;
    }
}