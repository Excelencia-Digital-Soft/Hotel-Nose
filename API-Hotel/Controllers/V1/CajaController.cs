using hotel.DTOs.Caja;
using hotel.DTOs.Common;
using hotel.Extensions;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers.V1;

/// <summary>
/// V1 Cash register management controller
/// </summary>
[ApiController]
[Route("api/v1/caja")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class CajaController : ControllerBase
{
    private readonly ICajaService _cajaService;
    private readonly ILogger<CajaController> _logger;

    public CajaController(ICajaService cajaService, ILogger<CajaController> logger)
    {
        _cajaService = cajaService ?? throw new ArgumentNullException(nameof(cajaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new cash register entry
    /// </summary>
    /// <param name="crearCajaDto">Cash register creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created cash register information</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CajaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CajaDto>>> CrearCaja(
        [FromBody] CrearCajaDto crearCajaDto,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse.Failure("Datos de entrada inválidos", errors.ToString()));
        }

        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("ID de institución requerido"));
        }

        var result = await _cajaService.CrearCajaAsync(
            crearCajaDto,
            institucionId.Value,
            this.GetCurrentUserId(),
            cancellationToken
        );

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Cash register created successfully for institution {InstitucionId} by user {UserId}",
                institucionId.Value,
                this.GetCurrentUserId()
            );

            return CreatedAtAction(nameof(CrearCaja), new { id = result.Data?.CierreId }, result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Gets all cash register closures for the current institution
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of cash register closures with details</returns>
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<CajaDetalladaDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CajaDetalladaDto>>>> GetCierres(
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("ID de institución requerido"));
        }

        var result = await _cajaService.GetCierresAsync(institucionId.Value, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Retrieved cash register closures for institution {InstitucionId} by user {UserId}",
                institucionId.Value,
                this.GetCurrentUserId()
            );

            return Ok(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Gets cash register closures with detailed payment information
    /// </summary>
    /// <param name="filtro">Filter parameters for the query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated cash register closures with payment details</returns>
    [HttpGet("con-pagos")]
    [ProducesResponseType(
        typeof(ApiResponse<CierresConPagosPaginadosDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CierresConPagosPaginadosDto>>> GetCierresConPagos(
        [FromQuery] CierresConPagosFiltroDto filtro,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(
                ApiResponse.Failure("Parámetros de filtro inválidos", errors.ToString())
            );
        }

        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("ID de institución requerido"));
        }

        var result = await _cajaService.GetCierresConPagosAsync(
            institucionId.Value,
            filtro,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Retrieved cash closures with payments for institution {InstitucionId} by user {UserId} (Page {Page})",
                institucionId.Value,
                this.GetCurrentUserId(),
                filtro.PageNumber
            );

            return Ok(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Gets a specific cash register closure by ID
    /// </summary>
    /// <param name="id">The ID of the cash register closure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cash register closure details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CajaDetalladaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CajaDetalladaDto>>> GetCierre(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponse.Failure("ID de cierre inválido"));
        }

        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("ID de institución requerido"));
        }

        var result = await _cajaService.GetCierreByIdAsync(
            id,
            institucionId.Value,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            if (result.Data == null)
            {
                return NotFound(ApiResponse.Failure("Cierre de caja no encontrado"));
            }

            _logger.LogInformation(
                "Retrieved cash register closure {CierreId} for institution {InstitucionId} by user {UserId}",
                id,
                institucionId.Value,
                this.GetCurrentUserId()
            );

            return Ok(result);
        }

        // Check if it's a not found error
        if (result.Message == "Cierre no encontrado")
        {
            return NotFound(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Gets complete details of a cash register closure including payments, cancellations, and expenses
    /// </summary>
    /// <param name="id">The ID of the cash register closure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete cash register closure details</returns>
    [HttpGet("{id}/detalle")]
    [ProducesResponseType(typeof(ApiResponse<CierreDetalleCompletoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CierreDetalleCompletoDto>>> GetDetalleCierre(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponse.Failure("ID de cierre inválido"));
        }

        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("ID de institución requerido"));
        }

        var result = await _cajaService.GetCierreDetalleAsync(
            id,
            institucionId.Value,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            if (result.Data == null)
            {
                return NotFound(ApiResponse.Failure("Cierre de caja no encontrado"));
            }

            _logger.LogInformation(
                "Retrieved complete details for cash register closure {CierreId} for institution {InstitucionId} by user {UserId}",
                id,
                institucionId.Value,
                this.GetCurrentUserId()
            );

            return Ok(result);
        }

        // Check if it's a not found error
        if (result.Message == "Cierre no encontrado")
        {
            return NotFound(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Gets all cash register closures and current pending transactions for the institution
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cash register closures and pending transactions</returns>
    [HttpGet("actual")]
    [ProducesResponseType(typeof(ApiResponse<CierresyActualDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CierresyActualDto>>> GetCierresyActual(
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("ID de institución requerido"));
        }

        var currentUserId = this.GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            _logger.LogWarning("User ID not found in claims for GetCierresyActual request");
            return BadRequest(ApiResponse.Failure("ID de usuario requerido"));
        }

        var result = await _cajaService.GetCierresyActualAsync(
            institucionId.Value,
            currentUserId,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Retrieved closures and current transactions for institution {InstitucionId} by user {UserId}",
                institucionId.Value,
                currentUserId
            );

            return Ok(result);
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Health check endpoint for the cash register service
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(
            new
            {
                service = "CajaService V1",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
            }
        );
    }
}
