using hotel.DTOs.Common;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace hotel.Controllers.V1;

/// <summary>
/// Example controller demonstrating how to use the notification service
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Send a test notification to all users in the current institution
    /// </summary>
    [HttpPost("test")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> SendTestNotification(
        [FromBody] NotificationDto notification)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        try
        {
            await _notificationService.SendNotificationToInstitutionAsync(
                institucionId.Value, 
                notification.Type, 
                notification.Message, 
                notification.Data);

            return Ok(ApiResponse.Success("Notification sent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification");
            return StatusCode(500, ApiResponse.Failure("Failed to send notification"));
        }
    }

    /// <summary>
    /// Send a notification to all connected users
    /// </summary>
    [HttpPost("broadcast")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> BroadcastNotification(
        [FromBody] NotificationDto notification)
    {
        try
        {
            await _notificationService.SendNotificationToAllAsync(
                notification.Type, 
                notification.Message, 
                notification.Data);

            return Ok(ApiResponse.Success("Broadcast notification sent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification");
            return StatusCode(500, ApiResponse.Failure("Failed to broadcast notification"));
        }
    }

    /// <summary>
    /// Send a notification to a specific user
    /// </summary>
    [HttpPost("user/{userId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> SendNotificationToUser(
        string userId,
        [FromBody] NotificationDto notification)
    {
        try
        {
            await _notificationService.SendNotificationToUserAsync(
                userId, 
                notification.Type, 
                notification.Message, 
                notification.Data);

            return Ok(ApiResponse.Success($"Notification sent to user {userId}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            return StatusCode(500, ApiResponse.Failure("Failed to send notification to user"));
        }
    }

    #region Private Methods
    private int? GetCurrentInstitucionId()
    {
        var institucionIdClaim = User.FindFirstValue("InstitucionId");
        if (!string.IsNullOrEmpty(institucionIdClaim) && int.TryParse(institucionIdClaim, out int institucionId))
        {
            return institucionId;
        }
        
        _logger.LogWarning("Institution ID not found in user claims for user {UserId}", 
            User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        return null;
    }
    #endregion
}