using hotel.DTOs.Common;
using hotel.DTOs.Empenos;
using hotel.Extensions;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers.V1
{
    /// <summary>
    /// Controller for managing Empeños (Pawn/Collateral) operations
    /// </summary>
    [ApiController]
    [Route("api/v1/empenos")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class EmpenosController : ControllerBase
    {
        private readonly IEmpenosService _empenosService;
        private readonly ILogger<EmpenosController> _logger;

        public EmpenosController(IEmpenosService empenosService, ILogger<EmpenosController> logger)
        {
            _empenosService =
                empenosService ?? throw new ArgumentNullException(nameof(empenosService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all unpaid empeños for the current institution
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of unpaid empeños</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmpenoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmpenoDto>>>> GetEmpenos(
            CancellationToken cancellationToken = default
        )
        {
            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                _logger.LogWarning(
                    "Institution ID not found in user claims for user {UserId}",
                    this.GetCurrentUserId()
                );
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation(
                "Getting unpaid empeños for institution {InstitucionId} requested by user {UserId}",
                institucionId.Value,
                this.GetCurrentUserId()
            );

            var result = await _empenosService.GetAllUnpaidByInstitutionAsync(
                institucionId.Value,
                cancellationToken
            );

            return result.IsSuccess
                ? Ok(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Gets all empeños (paid and unpaid) for the current institution
        /// </summary>
        /// <param name="includeAnulados">Include cancelled empeños</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all empeños</returns>
        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmpenoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmpenoDto>>>> GetAllEmpenos(
            [FromQuery] bool includeAnulados = false,
            CancellationToken cancellationToken = default
        )
        {
            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                _logger.LogWarning(
                    "Institution ID not found in user claims for user {UserId}",
                    this.GetCurrentUserId()
                );
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation(
                "Getting all empeños for institution {InstitucionId} (includeAnulados: {IncludeAnulados}) requested by user {UserId}",
                institucionId.Value,
                includeAnulados,
                this.GetCurrentUserId()
            );

            var result = await _empenosService.GetAllByInstitutionAsync(
                institucionId.Value,
                includeAnulados,
                cancellationToken
            );

            return result.IsSuccess
                ? Ok(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Gets a specific empeño by ID
        /// </summary>
        /// <param name="id">Empeño ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Empeño details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<EmpenoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<EmpenoDto>>> GetEmpenoById(
            int id,
            CancellationToken cancellationToken = default
        )
        {
            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                _logger.LogWarning(
                    "Institution ID not found in user claims for user {UserId}",
                    this.GetCurrentUserId()
                );
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation(
                "Getting empeño {EmpenoId} for institution {InstitucionId} requested by user {UserId}",
                id,
                institucionId.Value,
                this.GetCurrentUserId()
            );

            var result = await _empenosService.GetByIdAsync(
                id,
                institucionId.Value,
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return result.Errors.Any(e => e.Contains("No se encontró"))
                    ? NotFound(result)
                    : StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets empeños by visit ID
        /// </summary>
        /// <param name="visitaId">Visit ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of empeños for the visit</returns>
        [HttpGet("by-visita/{visitaId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmpenoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmpenoDto>>>> GetEmpenosByVisita(
            int visitaId,
            CancellationToken cancellationToken = default
        )
        {
            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                _logger.LogWarning(
                    "Institution ID not found in user claims for user {UserId}",
                    this.GetCurrentUserId()
                );
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation(
                "Getting empeños for visit {VisitaId} in institution {InstitucionId} requested by user {UserId}",
                visitaId,
                institucionId.Value,
                this.GetCurrentUserId()
            );

            var result = await _empenosService.GetByVisitaIdAsync(
                visitaId,
                institucionId.Value,
                cancellationToken
            );

            return result.IsSuccess
                ? Ok(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Creates a new empeño
        /// </summary>
        /// <param name="createDto">Empeño creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created empeño</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmpenoDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<EmpenoDto>>> CreateEmpeno(
            [FromBody] EmpenoCreateDto createDto,
            CancellationToken cancellationToken = default
        )
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();
                return BadRequest(
                    ApiResponse<EmpenoDto>.Failure(errors, "Datos de entrada inválidos")
                );
            }

            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                _logger.LogWarning(
                    "Institution ID not found in user claims for user {UserId}",
                    this.GetCurrentUserId()
                );
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            var currentUserId = this.GetCurrentUserId();
            
            _logger.LogInformation(
                "Creating empeño for visit {VisitaId} in institution {InstitucionId} requested by user {UserId}",
                createDto.VisitaId,
                institucionId.Value,
                currentUserId
            );

            var result = await _empenosService.CreateAsync(
                createDto,
                institucionId.Value,
                currentUserId, // Pass the authenticated user's ID
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return CreatedAtAction(
                nameof(GetEmpenoById),
                new { id = result.Data!.EmpenoId },
                result
            );
        }

        /// <summary>
        /// Updates an existing empeño
        /// </summary>
        /// <param name="id">Empeño ID</param>
        /// <param name="updateDto">Update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated empeño</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<EmpenoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<EmpenoDto>>> UpdateEmpeno(
            int id,
            [FromBody] EmpenoUpdateDto updateDto,
            CancellationToken cancellationToken = default
        )
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();
                return BadRequest(
                    ApiResponse<EmpenoDto>.Failure(errors, "Datos de entrada inválidos")
                );
            }

            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                _logger.LogWarning(
                    "Institution ID not found in user claims for user {UserId}",
                    this.GetCurrentUserId()
                );
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation(
                "Updating empeño {EmpenoId} in institution {InstitucionId} requested by user {UserId}",
                id,
                institucionId.Value,
                this.GetCurrentUserId()
            );

            var result = await _empenosService.UpdateAsync(
                id,
                updateDto,
                institucionId.Value,
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return result.Errors.Any(e => e.Contains("No se encontró"))
                    ? NotFound(result)
                    : StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Processes payment for an empeño (improved logic - no duplicate payments)
        /// </summary>
        /// <param name="id">Empeño ID</param>
        /// <param name="pagoDto">Payment details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment processing result</returns>
        [HttpPost("{id:int}/payment")]
        [ProducesResponseType(typeof(ApiResponse<EmpenoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<EmpenoDto>>> PayEmpeno(
            int id,
            [FromBody] EmpernoPagoDto pagoDto,
            CancellationToken cancellationToken = default
        )
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();
                return BadRequest(
                    ApiResponse<EmpenoDto>.Failure(errors, "Datos de pago inválidos")
                );
            }

            if (!pagoDto.IsValidPayment)
            {
                return BadRequest(
                    ApiResponse<EmpenoDto>.Failure("El monto total del pago debe ser mayor a 0")
                );
            }

            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                _logger.LogWarning(
                    "Institution ID not found in user claims for user {UserId}",
                    this.GetCurrentUserId()
                );
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation(
                "Processing payment for empeño {EmpenoId} (Total: {MontoTotal}) in institution {InstitucionId} requested by user {UserId}",
                id,
                pagoDto.MontoTotal,
                institucionId.Value,
                this.GetCurrentUserId()
            );

            var result = await _empenosService.PayEmpenoAsync(
                id,
                pagoDto,
                institucionId.Value,
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return result.Errors.Any(e => e.Contains("No se encontró"))
                    ? NotFound(result)
                    : StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Soft deletes (cancels) an empeño
        /// </summary>
        /// <param name="id">Empeño ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation result</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteEmpeno(
            int id,
            CancellationToken cancellationToken = default
        )
        {
            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                _logger.LogWarning(
                    "Institution ID not found in user claims for user {UserId}",
                    this.GetCurrentUserId()
                );
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation(
                "Deleting empeño {EmpenoId} in institution {InstitucionId} requested by user {UserId}",
                id,
                institucionId.Value,
                this.GetCurrentUserId()
            );

            var result = await _empenosService.DeleteAsync(
                id,
                institucionId.Value,
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return result.Errors.Any(e => e.Contains("No se encontró"))
                    ? NotFound(result)
                    : StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Validates if a visit exists for empeño creation
        /// </summary>
        /// <param name="visitaId">Visit ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        [HttpGet("validate-visita/{visitaId:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateVisita(
            int visitaId,
            CancellationToken cancellationToken = default
        )
        {
            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                _logger.LogWarning(
                    "Institution ID not found in user claims for user {UserId}",
                    this.GetCurrentUserId()
                );
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation(
                "Validating visit {VisitaId} for institution {InstitucionId} requested by user {UserId}",
                visitaId,
                institucionId.Value,
                this.GetCurrentUserId()
            );

            var result = await _empenosService.ValidateVisitaExistsAsync(
                visitaId,
                institucionId.Value,
                cancellationToken
            );

            return result.IsSuccess
                ? Ok(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Health check endpoint for the empeños service
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
                    service = "EmpenosService V1",
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    features = new[]
                    {
                        "CRUD operations",
                        "Payment processing (fixed logic)",
                        "Institution-based multi-tenancy",
                        "Bearer token authentication",
                        "RESTful API design",
                    },
                }
            );
        }
    }
}

