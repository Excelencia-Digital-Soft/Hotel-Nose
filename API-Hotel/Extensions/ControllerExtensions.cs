using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Extensions;

/// <summary>
/// Extension methods for Controllers to help with common operations
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Obtiene el UserId del usuario autenticado desde los claims (AspNetCore Identity)
    /// </summary>
    /// <param name="controller">Controller instance</param>
    /// <returns>UserId como string (GUID) o null si no está autenticado</returns>
    public static string? GetCurrentUserId(this ControllerBase controller)
    {
        return controller.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    /// <summary>
    /// Obtiene el UserName del usuario autenticado desde los claims
    /// </summary>
    /// <param name="controller">Controller instance</param>
    /// <returns>UserName o null si no está autenticado</returns>
    public static string? GetCurrentUserName(this ControllerBase controller)
    {
        return controller.User?.FindFirstValue(ClaimTypes.Name);
    }

    /// <summary>
    /// Obtiene el InstitucionId del usuario autenticado desde los claims
    /// </summary>
    /// <param name="controller">Controller instance</param>
    /// <returns>InstitucionId o null si no está en los claims</returns>
    public static int? GetCurrentInstitucionId(this ControllerBase controller)
    {
        var institucionIdClaim = controller.User?.FindFirstValue("InstitucionId");
        if (!string.IsNullOrEmpty(institucionIdClaim) && int.TryParse(institucionIdClaim, out int institucionId))
        {
            return institucionId;
        }
        return null;
    }

    /// <summary>
    /// Obtiene la dirección IP del cliente desde la request
    /// </summary>
    /// <param name="controller">Controller instance</param>
    /// <returns>IP Address como string</returns>
    public static string? GetClientIpAddress(this ControllerBase controller)
    {
        var ipAddress = controller.HttpContext.Connection.RemoteIpAddress?.ToString();
        
        // Check for forwarded IP in case of proxy/load balancer
        var forwardedFor = controller.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            ipAddress = forwardedFor.Split(',')[0].Trim();
        }
        
        var realIp = controller.HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            ipAddress = realIp;
        }

        return ipAddress;
    }

    /// <summary>
    /// Verifica si el usuario actual está autenticado y retorna información básica
    /// </summary>
    /// <param name="controller">Controller instance</param>
    /// <returns>Información del usuario autenticado o null</returns>
    public static CurrentUserInfo? GetCurrentUserInfo(this ControllerBase controller)
    {
        var userId = controller.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return new CurrentUserInfo
        {
            UserId = userId,
            UserName = controller.GetCurrentUserName(),
            InstitucionId = controller.GetCurrentInstitucionId(),
            IpAddress = controller.GetClientIpAddress()
        };
    }
}

/// <summary>
/// Información del usuario autenticado
/// </summary>
public class CurrentUserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public int? InstitucionId { get; set; }
    public string? IpAddress { get; set; }
}