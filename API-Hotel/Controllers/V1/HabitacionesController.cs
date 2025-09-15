using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using hotel.DTOs;
using hotel.DTOs.Common;
using hotel.Interfaces;

namespace hotel.Controllers.V1;

/// <summary>
/// V1 Controller for managing hotel rooms (Habitaciones)
/// Follows REST principles and clean architecture patterns
/// </summary>
[ApiController]
[Route("api/v1/habitaciones")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class HabitacionesController : ControllerBase
{
    private readonly IHabitacionesService _habitacionesService;
    private readonly IRoomNotificationService _roomNotificationService;
    private readonly ILogger<HabitacionesController> _logger;

    public HabitacionesController(
        IHabitacionesService habitacionesService,
        IRoomNotificationService roomNotificationService,
        ILogger<HabitacionesController> logger)
    {
        _habitacionesService = habitacionesService ?? throw new ArgumentNullException(nameof(habitacionesService));
        _roomNotificationService = roomNotificationService ?? throw new ArgumentNullException(nameof(roomNotificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all rooms for the current institution
    /// </summary>
    /// <param name="includeInactive">Include inactive rooms in the result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of rooms</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HabitacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<HabitacionDto>>>> GetHabitaciones(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.GetHabitacionesAsync(
            institucionId.Value, 
            includeInactive, 
            cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get a specific room by ID
    /// </summary>
    /// <param name="id">Room ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Room details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<HabitacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<HabitacionDto>>> GetHabitacion(
        int id,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.GetHabitacionByIdAsync(
            id, 
            institucionId.Value, 
            cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all available rooms for the current institution
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available rooms</returns>
    [HttpGet("available")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HabitacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<HabitacionDto>>>> GetAvailableHabitaciones(
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.GetAvailableHabitacionesAsync(
            institucionId.Value, 
            cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get all occupied rooms for the current institution
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of occupied rooms</returns>
    [HttpGet("occupied")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HabitacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<HabitacionDto>>>> GetOccupiedHabitaciones(
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.GetOccupiedHabitacionesAsync(
            institucionId.Value, 
            cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get room statistics for the current institution
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Room statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<HabitacionStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<HabitacionStatsDto>>> GetHabitacionStats(
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.GetHabitacionStatsAsync(
            institucionId.Value, 
            cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Create a new room
    /// </summary>
    /// <param name="createDto">Room creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created room</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HabitacionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<HabitacionDto>>> CreateHabitacion(
        [FromBody] HabitacionCreateDto createDto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse.Failure(errors, "Invalid request data"));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.CreateHabitacionAsync(
            createDto, 
            institucionId.Value, 
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetHabitacion), 
            new { id = result.Data!.HabitacionId }, 
            result);
    }

    /// <summary>
    /// Update an existing room
    /// </summary>
    /// <param name="id">Room ID</param>
    /// <param name="updateDto">Room update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated room</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<HabitacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<HabitacionDto>>> UpdateHabitacion(
        int id,
        [FromBody] HabitacionUpdateDto updateDto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse.Failure(errors, "Invalid request data"));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.UpdateHabitacionAsync(
            id, 
            updateDto, 
            institucionId.Value, 
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true 
                ? NotFound(result) 
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a room (soft delete)
    /// </summary>
    /// <param name="id">Room ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Delete confirmation</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> DeleteHabitacion(
        int id,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.DeleteHabitacionAsync(
            id, 
            institucionId.Value, 
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true 
                ? NotFound(result) 
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Change room availability status
    /// </summary>
    /// <param name="id">Room ID</param>
    /// <param name="availabilityDto">Availability change data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update confirmation</returns>
    [HttpPatch("{id:int}/availability")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> ChangeAvailability(
        int id,
        [FromBody] HabitacionAvailabilityDto availabilityDto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse.Failure(errors, "Invalid request data"));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.ChangeAvailabilityAsync(
            id, 
            availabilityDto.Disponible, 
            institucionId.Value, 
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true 
                ? NotFound(result) 
                : BadRequest(result);
        }

        // Send real-time notification for room availability change
        var status = availabilityDto.Disponible ? "libre" : "mantenimiento";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        await _roomNotificationService.NotifyRoomStatusChanged(
            id,
            status,
            null,
            institucionId.Value,
            userId
        );

        if (!availabilityDto.Disponible)
        {
            await _roomNotificationService.NotifyRoomMaintenanceChanged(
                id,
                "availability",
                "started",
                "Room marked as unavailable",
                institucionId.Value
            );
        }
        else
        {
            await _roomNotificationService.NotifyRoomMaintenanceChanged(
                id,
                "availability",
                "completed",
                "Room marked as available",
                institucionId.Value
            );
        }

        _logger.LogInformation(
            "Sent availability change notification for room {RoomId} - available: {Available}",
            id,
            availabilityDto.Disponible
        );

        return Ok(result);
    }

    /// <summary>
    /// Get complete room information including visits, reservations, orders, images and characteristics
    /// </summary>
    /// <param name="includeInactive">Include inactive rooms in the result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete room information</returns>
    [HttpGet("complete")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HabitacionCompleteDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<HabitacionCompleteDto>>>> GetHabitacionesComplete(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.GetHabitacionesCompleteAsync(
            institucionId.Value, 
            includeInactive, 
            cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get free rooms with minimal data for optimal performance
    /// Returns ~70% less data compared to complete endpoint
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of free rooms with minimal data</returns>
    [HttpGet("free")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HabitacionLibreDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<HabitacionLibreDto>>>> GetFreeHabitaciones(
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.GetFreeHabitacionesOptimizedAsync(
            institucionId.Value, 
            cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get occupied rooms with optimized data structure
    /// Returns ~40% less data compared to complete endpoint
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of occupied rooms with optimized data</returns>
    [HttpGet("occupied-optimized")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HabitacionOptimizedDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<HabitacionOptimizedDto>>>> GetOccupiedHabitacionesOptimized(
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.GetOccupiedHabitacionesOptimizedAsync(
            institucionId.Value, 
            cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get optimized rooms data with conditional loading
    /// Free rooms: minimal data, Occupied rooms: necessary data only
    /// </summary>
    /// <param name="filter">Optional filter parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Optimized rooms data</returns>
    [HttpGet("optimized")]
    [ProducesResponseType(typeof(ApiResponse<HabitacionBulkStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<HabitacionBulkStatsDto>>> GetHabitacionesOptimized(
        [FromQuery] HabitacionFilterDto? filter = null,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _habitacionesService.GetHabitacionesOptimizedAsync(
            institucionId.Value, 
            filter ?? new HabitacionFilterDto(),
            cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Health check endpoint for the habitaciones service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            service = "HabitacionesService V1", 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    #region Private Methods

    /// <summary>
    /// Get the current user's institution ID from JWT claims
    /// </summary>
    /// <returns>Institution ID or null if not found</returns>
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