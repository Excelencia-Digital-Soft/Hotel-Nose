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

        // SignalR V1 Hub with authentication required
        app.MapHub<NotificationsHub>("/api/v1/notifications")
            .RequireAuthorization();

        return app;
    }
}