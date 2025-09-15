using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using hotel.DTOs.Common;
using hotel.DTOs.Consumos;
using hotel.Interfaces;

namespace hotel.Controllers.V1;

/// <summary>
/// V1 Controller for managing consumos (consumptions)
/// Follows REST principles and clean architecture patterns
/// </summary>
[ApiController]
[Route("api/v1/consumos")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ConsumosController : ControllerBase
{
    private readonly IConsumosService _consumosService;
    private readonly ILogger<ConsumosController> _logger;

    public ConsumosController(
        IConsumosService consumosService,
        ILogger<ConsumosController> logger)
    {
        _consumosService = consumosService ?? throw new ArgumentNullException(nameof(consumosService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all consumos for a specific visit
    /// </summary>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of consumos</returns>
    [HttpGet("visita/{visitaId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConsumoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ConsumoDto>>>> GetConsumosByVisita(
        int visitaId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting consumos for visit {VisitaId}", visitaId);

        var result = await _consumosService.GetConsumosByVisitaAsync(visitaId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get consumos summary for a visit
    /// </summary>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Consumos summary</returns>
    [HttpGet("visita/{visitaId:int}/summary")]
    [ProducesResponseType(typeof(ApiResponse<ConsumoSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ConsumoSummaryDto>>> GetConsumosSummary(
        int visitaId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting consumos summary for visit {VisitaId}", visitaId);

        var result = await _consumosService.GetConsumosSummaryAsync(visitaId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add general consumos to a visit
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="items">Items to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("general")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> AddGeneralConsumos(
        [FromQuery] int habitacionId,
        [FromQuery] int visitaId,
        [FromBody] IEnumerable<ConsumoCreateDto> items,
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

        _logger.LogInformation("Adding general consumos for habitacion {HabitacionId}, visita {VisitaId}", 
            habitacionId, visitaId);

        var result = await _consumosService.AddGeneralConsumosAsync(
            habitacionId, visitaId, items, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetConsumosByVisita), new { visitaId }, result);
    }

    /// <summary>
    /// Add room-specific consumos to a visit
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="items">Items to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("room")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> AddRoomConsumos(
        [FromQuery] int habitacionId,
        [FromQuery] int visitaId,
        [FromBody] IEnumerable<ConsumoCreateDto> items,
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

        _logger.LogInformation("Adding room consumos for habitacion {HabitacionId}, visita {VisitaId}", 
            habitacionId, visitaId);

        var result = await _consumosService.AddRoomConsumosAsync(
            habitacionId, visitaId, items, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetConsumosByVisita), new { visitaId }, result);
    }

    /// <summary>
    /// Update consumo quantity
    /// </summary>
    /// <param name="consumoId">Consumo ID</param>
    /// <param name="updateDto">Update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated consumo</returns>
    [HttpPut("{consumoId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ConsumoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ConsumoDto>>> UpdateConsumo(
        int consumoId,
        [FromBody] ConsumoUpdateDto updateDto,
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

        _logger.LogInformation("Updating consumo {ConsumoId} with quantity {Quantity}", 
            consumoId, updateDto.Cantidad);

        var result = await _consumosService.UpdateConsumoQuantityAsync(
            consumoId, updateDto.Cantidad, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel/void a consumo
    /// </summary>
    /// <param name="consumoId">Consumo ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{consumoId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> CancelConsumo(
        int consumoId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling consumo {ConsumoId}", consumoId);

        var result = await _consumosService.CancelConsumoAsync(consumoId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint for the consumos service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            service = "ConsumosService V1",
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