using hotel.DTOs.Common;
using hotel.DTOs.HabitacionCategorias;
using hotel.Extensions;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers.V1
{
    /// <summary>
    /// Controller for managing room categories (CategoriasHabitaciones)
    /// </summary>
    [ApiController]
    [Route("api/v1/habitacion-categorias")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class HabitacionCategoriasController : ControllerBase
    {
        private readonly IHabitacionCategoriasService _habitacionCategoriasService;
        private readonly ILogger<HabitacionCategoriasController> _logger;

        public HabitacionCategoriasController(
            IHabitacionCategoriasService habitacionCategoriasService,
            ILogger<HabitacionCategoriasController> logger)
        {
            _habitacionCategoriasService = habitacionCategoriasService ?? throw new ArgumentNullException(nameof(habitacionCategoriasService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all room categories for the current institution
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of room categories</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<HabitacionCategoriaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<HabitacionCategoriaDto>>>> GetHabitacionCategorias(
            CancellationToken cancellationToken = default)
        {
            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                _logger.LogWarning("Institution ID not found in user claims for user {UserId}", 
                    this.GetCurrentUserId());
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation("Getting room categories for institution {InstitucionId} requested by user {UserId}", 
                institucionId.Value, this.GetCurrentUserId());

            var result = await _habitacionCategoriasService.GetAllByInstitutionAsync(institucionId.Value, cancellationToken);
            
            return result.IsSuccess 
                ? Ok(result) 
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Gets a specific room category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Room category details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<HabitacionCategoriaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<HabitacionCategoriaDto>>> GetHabitacionCategoria(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse.Failure("ID de categoría inválido"));
            }

            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation("Getting room category {Id} for institution {InstitucionId} requested by user {UserId}", 
                id, institucionId.Value, this.GetCurrentUserId());

            var result = await _habitacionCategoriasService.GetByIdAsync(id, institucionId.Value, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return result.Errors.Any(e => e.Contains("no encontrada"))
                ? NotFound(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Creates a new room category
        /// </summary>
        /// <param name="createDto">Room category creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created room category</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<HabitacionCategoriaDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<HabitacionCategoriaDto>>> CreateHabitacionCategoria(
            [FromBody] HabitacionCategoriaCreateDto createDto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse.Failure(errors, "Datos de entrada inválidos"));
            }

            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation("Creating room category '{Name}' for institution {InstitucionId} by user {UserId}", 
                createDto.NombreCategoria, institucionId.Value, this.GetCurrentUserId());

            var result = await _habitacionCategoriasService.CreateAsync(createDto, institucionId.Value, cancellationToken);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetHabitacionCategoria),
                    new { id = result.Data!.CategoriaId },
                    result);
            }

            return result.Errors.Any(e => e.Contains("nombre"))
                ? Conflict(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Updates an existing room category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="updateDto">Room category update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated room category</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<HabitacionCategoriaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<HabitacionCategoriaDto>>> UpdateHabitacionCategoria(
            int id,
            [FromBody] HabitacionCategoriaUpdateDto updateDto,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse.Failure("ID de categoría inválido"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse.Failure(errors, "Datos de entrada inválidos"));
            }

            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation("Updating room category {Id} for institution {InstitucionId} by user {UserId}", 
                id, institucionId.Value, this.GetCurrentUserId());

            var result = await _habitacionCategoriasService.UpdateAsync(id, updateDto, institucionId.Value, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.Errors.Any(e => e.Contains("no encontrada")))
            {
                return NotFound(result);
            }

            return result.Errors.Any(e => e.Contains("nombre"))
                ? Conflict(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Deletes a room category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation result</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteHabitacionCategoria(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse.Failure("ID de categoría inválido"));
            }

            var institucionId = this.GetCurrentInstitucionId();
            if (!institucionId.HasValue)
            {
                return BadRequest(ApiResponse.Failure("ID de institución requerido"));
            }

            _logger.LogInformation("Deleting room category {Id} for institution {InstitucionId} by user {UserId}", 
                id, institucionId.Value, this.GetCurrentUserId());

            var result = await _habitacionCategoriasService.DeleteAsync(id, institucionId.Value, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.Errors.Any(e => e.Contains("no encontrada")))
            {
                return NotFound(result);
            }

            return result.Errors.Any(e => e.Contains("utilizada")))
                ? Conflict(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Health check endpoint for the HabitacionCategorias V1 service
        /// </summary>
        /// <returns>Service health status</returns>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new
            {
                service = "HabitacionCategoriasService V1",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}