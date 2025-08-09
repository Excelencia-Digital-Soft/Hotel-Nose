using System.Security.Claims;
using System.Text;
using hotel.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace hotel.Extensions;

/// <summary>
/// Extension methods for authentication and authorization configuration
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Configure JWT authentication
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                ),
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };

            // Configure events to prevent redirects, handle SignalR tokens, and return proper HTTP status codes
            options.Events = new JwtBearerEvents
            {
                // Handle SignalR token from query string
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    
                    // If the request is for SignalR hub and we have a token in query string
                    if (!string.IsNullOrEmpty(accessToken) && 
                        path.StartsWithSegments("/api/v1/notifications"))
                    {
                        // Read the token out of the query string
                        context.Token = accessToken;
                    }
                    
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        isSuccess = false,
                        message = "Authentication required. Please provide a valid JWT token.",
                        errors = new[] { "Unauthorized access. Token missing or invalid." }
                    });

                    return context.Response.WriteAsync(result);
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";

                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        isSuccess = false,
                        message = "Access forbidden. Insufficient permissions.",
                        errors = new[] { "You don't have permission to access this resource." }
                    });

                    return context.Response.WriteAsync(result);
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                    logger?.LogWarning("JWT Authentication failed: {Exception}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                    logger?.LogInformation("JWT Token validated successfully for user: {UserName}", 
                        context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            // Policy that allows either Administrator OR Director role
            options.AddPolicy("AdminOrDirector", policy =>
                policy.RequireRole("Administrator", "Director")
                      .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
        });

        return services;
    }

    /// <summary>
    /// Configure ASP.NET Core Identity
    /// </summary>
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings - Relaxed for legacy users
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 3;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<hotel.Data.HotelDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}