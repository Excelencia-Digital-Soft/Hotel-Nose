using System.Security.Claims;
using hotel.DTOs.Common;
using hotel.DTOs.Reservas;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers.V1;

/// <summary>
/// V1 Controller for managing reservas (reservations)
/// Follows REST principles and clean architecture patterns
/// </summary>
[ApiController]
[Route("api/v1/reservas")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ReservasController : ControllerBase
{
    private readonly IReservasService _reservasService;
    private readonly ILogger<ReservasController> _logger;

    public ReservasController(IReservasService reservasService, ILogger<ReservasController> logger)
    {
        _reservasService =
            reservasService ?? throw new ArgumentNullException(nameof(reservasService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get reservation by ID
    /// </summary>
    /// <param name="reservaId">Reservation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reservation details</returns>
    [HttpGet("{reservaId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ReservaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ReservaDto>>> GetReservation(
        int reservaId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting reservation {ReservaId}", reservaId);

        var result = await _reservasService.GetReservationByIdAsync(reservaId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get active reservations for current institution
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active reservations</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReservaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReservaDto>>>> GetActiveReservations(
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        _logger.LogInformation(
            "Getting active reservations for institution {InstitucionId}",
            institucionId.Value
        );

        var result = await _reservasService.GetActiveReservationsAsync(
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Finalize a reservation and free the room
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("finalize")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> FinalizeReservation(
        [FromQuery] int habitacionId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Finalizing reservation for habitacion {HabitacionId}",
            habitacionId
        );

        var result = await _reservasService.FinalizeReservationAsync(
            habitacionId,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Pause an active occupation
    /// </summary>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{visitaId:int}/pause")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> PauseOccupation(
        int visitaId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Pausing occupation for visita {VisitaId}", visitaId);

        var result = await _reservasService.PauseOccupationAsync(visitaId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Resume a paused occupation
    /// </summary>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{visitaId:int}/resume")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> ResumeOccupation(
        int visitaId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Resuming occupation for visita {VisitaId}", visitaId);

        var result = await _reservasService.ResumeOccupationAsync(visitaId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update reservation promotion
    /// </summary>
    /// <param name="reservaId">Reservation ID</param>
    /// <param name="updateDto">Promotion update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated reservation</returns>
    [HttpPut("{reservaId:int}/promotion")]
    [ProducesResponseType(typeof(ApiResponse<ReservaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ReservaDto>>> UpdateReservationPromotion(
        int reservaId,
        [FromBody] ReservaPromocionUpdateDto updateDto,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse.Failure(errors, "Invalid request data"));
        }

        _logger.LogInformation(
            "Updating promotion for reservation {ReservaId} to {PromocionId}",
            reservaId,
            updateDto.PromocionId
        );

        var result = await _reservasService.UpdateReservationPromotionAsync(
            reservaId,
            updateDto.PromocionId,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Extend reservation time
    /// </summary>
    /// <param name="reservaId">Reservation ID</param>
    /// <param name="extensionDto">Extension data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated reservation</returns>
    [HttpPut("{reservaId:int}/extend")]
    [ProducesResponseType(typeof(ApiResponse<ReservaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ReservaDto>>> ExtendReservation(
        int reservaId,
        [FromBody] ReservaExtensionDto extensionDto,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse.Failure(errors, "Invalid request data"));
        }

        _logger.LogInformation(
            "Extending reservation {ReservaId} by {Hours}h {Minutes}m",
            reservaId,
            extensionDto.AdditionalHours,
            extensionDto.AdditionalMinutes
        );

        var result = await _reservasService.ExtendReservationAsync(
            reservaId,
            extensionDto.AdditionalHours,
            extensionDto.AdditionalMinutes,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel/void a reservation
    /// </summary>
    /// <param name="reservaId">Reservation ID</param>
    /// <param name="cancelDto">Cancellation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{reservaId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> CancelReservation(
        int reservaId,
        [FromBody] ReservaCancelDto cancelDto,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse.Failure(errors, "Invalid request data"));
        }

        _logger.LogInformation(
            "Cancelling reservation {ReservaId} with reason: {Reason}",
            reservaId,
            cancelDto.Reason
        );

        var result = await _reservasService.CancelReservationAsync(
            reservaId,
            cancelDto.Reason,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Comprehensive cancellation of an occupation including movements, consumptions, and inventory restoration
    /// </summary>
    /// <param name="reservaId">Reservation ID to cancel</param>
    /// <param name="cancelDto">Cancellation details including reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{reservaId:int}/comprehensive-cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> ComprehensiveCancelOccupation(
        int reservaId,
        [FromBody] ReservaCancelDto cancelDto,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID is required"));
        }

        _logger.LogInformation(
            "Comprehensive cancellation of reservation {ReservaId} in institution {InstitucionId} by user {UserId} with reason: {Reason}",
            reservaId,
            institucionId.Value,
            userId,
            cancelDto.Reason
        );

        var result = await _reservasService.ComprehensiveCancelOccupationAsync(
            reservaId,
            cancelDto.Reason,
            institucionId.Value,
            userId,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new reservation
    /// </summary>
    /// <param name="createDto">Reservation creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created reservation details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReservaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ReservaDto>>> CreateReservation(
        [FromBody] ReservaCreateDto createDto,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID is required"));
        }

        _logger.LogInformation(
            "Creating reservation for room {HabitacionId} in institution {InstitucionId} by user {UserId}",
            createDto.HabitacionId,
            institucionId.Value,
            userId
        );

        var result = await _reservasService.CreateReservationAsync(
            createDto,
            institucionId.Value,
            userId,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetReservation),
            new { reservaId = result.Data!.ReservaId },
            result
        );
    }

    /// <summary>
    /// Health check endpoint for the reservas service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(
            new
            {
                service = "ReservasService V1",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
            }
        );
    }

    #region Private Methods

    /// <summary>
    /// Get the current user's institution ID from JWT claims
    /// </summary>
    /// <returns>Institution ID or null if not found</returns>
    private int? GetCurrentInstitucionId()
    {
        var institucionIdClaim = User.FindFirstValue("InstitucionId");
        if (
            !string.IsNullOrEmpty(institucionIdClaim)
            && int.TryParse(institucionIdClaim, out int institucionId)
        )
        {
            return institucionId;
        }

        _logger.LogWarning(
            "Institution ID not found in user claims for user {UserId}",
            User.FindFirstValue(ClaimTypes.NameIdentifier)
        );

        return null;
    }

    #endregion
}

