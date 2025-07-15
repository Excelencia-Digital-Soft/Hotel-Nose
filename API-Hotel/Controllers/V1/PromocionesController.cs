using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using hotel.DTOs.Common;
using hotel.DTOs.Promociones;
using hotel.Interfaces;

namespace hotel.Controllers.V1;

/// <summary>
/// V1 Controller for managing promociones (promotions)
/// Follows REST principles and clean architecture patterns
/// </summary>
[ApiController]
[Route("api/v1/promociones")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class PromocionesController : ControllerBase
{
    private readonly IPromocionesService _promocionesService;
    private readonly ILogger<PromocionesController> _logger;

    public PromocionesController(
        IPromocionesService promocionesService,
        ILogger<PromocionesController> logger)
    {
        _promocionesService = promocionesService ?? throw new ArgumentNullException(nameof(promocionesService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all promotions for a specific category
    /// </summary>
    /// <param name="categoriaId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of promotions</returns>
    [HttpGet("categoria/{categoriaId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PromocionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PromocionDto>>>> GetPromotionsByCategory(
        int categoriaId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting promotions for category {CategoriaId}", categoriaId);

        var result = await _promocionesService.GetPromotionsByCategoryAsync(categoriaId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all active promotions for current institution
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active promotions</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PromocionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PromocionDto>>>> GetActivePromotions(
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        _logger.LogInformation("Getting active promotions for institution {InstitucionId}", institucionId.Value);

        var result = await _promocionesService.GetActivePromotionsAsync(institucionId.Value, cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get promotion by ID
    /// </summary>
    /// <param name="promocionId">Promotion ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Promotion details</returns>
    [HttpGet("{promocionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PromocionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PromocionDto>>> GetPromotion(
        int promocionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting promotion {PromocionId}", promocionId);

        var result = await _promocionesService.GetPromotionByIdAsync(promocionId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new promotion
    /// </summary>
    /// <param name="createDto">Promotion creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created promotion</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PromocionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PromocionDto>>> CreatePromotion(
        [FromBody] PromocionCreateDto createDto,
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

        _logger.LogInformation("Creating promotion {Name} for institution {InstitucionId}", 
            createDto.Nombre, institucionId.Value);

        var result = await _promocionesService.CreatePromotionAsync(
            createDto, institucionId.Value, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetPromotion),
            new { promocionId = result.Data!.PromocionId },
            result);
    }

    /// <summary>
    /// Update an existing promotion
    /// </summary>
    /// <param name="promocionId">Promotion ID</param>
    /// <param name="updateDto">Promotion update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated promotion</returns>
    [HttpPut("{promocionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PromocionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PromocionDto>>> UpdatePromotion(
        int promocionId,
        [FromBody] PromocionUpdateDto updateDto,
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

        _logger.LogInformation("Updating promotion {PromocionId} for institution {InstitucionId}", 
            promocionId, institucionId.Value);

        var result = await _promocionesService.UpdatePromotionAsync(
            promocionId, updateDto, institucionId.Value, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a promotion (soft delete)
    /// </summary>
    /// <param name="promocionId">Promotion ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{promocionId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> DeletePromotion(
        int promocionId,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        _logger.LogInformation("Deleting promotion {PromocionId} for institution {InstitucionId}", 
            promocionId, institucionId.Value);

        var result = await _promocionesService.DeletePromotionAsync(
            promocionId, institucionId.Value, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Errors?.Any(e => e.Contains("not found")) == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Validate if a promotion is applicable to a reservation
    /// </summary>
    /// <param name="promocionId">Promotion ID</param>
    /// <param name="reservaId">Reservation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    [HttpPost("{promocionId:int}/validate")]
    [ProducesResponseType(typeof(ApiResponse<PromocionValidationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PromocionValidationDto>>> ValidatePromotion(
        int promocionId,
        [FromQuery] int reservaId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating promotion {PromocionId} for reservation {ReservaId}", 
            promocionId, reservaId);

        var result = await _promocionesService.ValidatePromotionAsync(
            promocionId, reservaId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint for the promociones service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            service = "PromocionesService V1",
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