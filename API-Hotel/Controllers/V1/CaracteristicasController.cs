using hotel.DTOs.Caracteristicas;
using hotel.DTOs.Common;
using hotel.Extensions;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers.V1
{
    /// <summary>
    /// Controller for managing room characteristics/features
    /// </summary>
    [ApiController]
    [Route("api/v1/caracteristicas")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CaracteristicasController : ControllerBase
    {
        private readonly ICaracteristicasService _caracteristicasService;
        private readonly ILogger<CaracteristicasController> _logger;

        public CaracteristicasController(
            ICaracteristicasService caracteristicasService,
            ILogger<CaracteristicasController> logger)
        {
            _caracteristicasService = caracteristicasService ?? throw new ArgumentNullException(nameof(caracteristicasService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all caracteristicas
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all caracteristicas</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CaracteristicaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CaracteristicaDto>>>> GetCaracteristicas(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all caracteristicas requested by user {UserId}", 
                this.GetCurrentUserId());

            var result = await _caracteristicasService.GetAllAsync(cancellationToken);
            
            return result.IsSuccess 
                ? Ok(result) 
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Gets a specific caracteristica by ID
        /// </summary>
        /// <param name="id">Caracteristica ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Caracteristica details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CaracteristicaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CaracteristicaDto>>> GetCaracteristica(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse.Failure("ID de característica inválido"));
            }

            _logger.LogInformation("Getting caracteristica {Id} requested by user {UserId}", 
                id, this.GetCurrentUserId());

            var result = await _caracteristicasService.GetByIdAsync(id, cancellationToken);
            
            return result.IsSuccess
                ? Ok(result)
                : result.Errors.Contains("no encontrada")
                    ? NotFound(result)
                    : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Creates a new caracteristica
        /// </summary>
        /// <param name="createDto">Caracteristica creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created caracteristica</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CaracteristicaDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CaracteristicaDto>>> CreateCaracteristica(
            [FromForm] CaracteristicaCreateDto createDto,
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

            _logger.LogInformation("Creating caracteristica '{Name}' by user {UserId}", 
                createDto.Nombre, this.GetCurrentUserId());

            var result = await _caracteristicasService.CreateAsync(createDto, cancellationToken);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetCaracteristica),
                    new { id = result.Data!.CaracteristicaId },
                    result);
            }

            return result.Errors.Any(e => e.Contains("nombre"))
                ? Conflict(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Updates an existing caracteristica
        /// </summary>
        /// <param name="id">Caracteristica ID</param>
        /// <param name="updateDto">Caracteristica update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated caracteristica</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CaracteristicaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CaracteristicaDto>>> UpdateCaracteristica(
            int id,
            [FromForm] CaracteristicaUpdateDto updateDto,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse.Failure("ID de característica inválido"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse.Failure(errors, "Datos de entrada inválidos"));
            }

            _logger.LogInformation("Updating caracteristica {Id} by user {UserId}", 
                id, this.GetCurrentUserId());

            var result = await _caracteristicasService.UpdateAsync(id, updateDto, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.Errors.Contains("no encontrada"))
            {
                return NotFound(result);
            }

            return result.Errors.Any(e => e.Contains("nombre"))
                ? Conflict(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Deletes a caracteristica
        /// </summary>
        /// <param name="id">Caracteristica ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation result</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteCaracteristica(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse.Failure("ID de característica inválido"));
            }

            _logger.LogInformation("Deleting caracteristica {Id} by user {UserId}", 
                id, this.GetCurrentUserId());

            var result = await _caracteristicasService.DeleteAsync(id, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.Errors.Contains("no encontrada"))
            {
                return NotFound(result);
            }

            return result.Errors.Any(e => e.Contains("utilizada"))
                ? Conflict(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Assigns caracteristicas to a specific room
        /// </summary>
        /// <param name="roomId">Room ID</param>
        /// <param name="assignmentDto">Caracteristicas assignment data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation result</returns>
        [HttpPost("rooms/{roomId:int}/assign")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> AssignCaracteristicasToRoom(
            int roomId,
            [FromBody] RoomCaracteristicaAssignmentDto assignmentDto,
            CancellationToken cancellationToken = default)
        {
            if (roomId <= 0)
            {
                return BadRequest(ApiResponse.Failure("ID de habitación inválido"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse.Failure(errors, "Datos de entrada inválidos"));
            }

            _logger.LogInformation("Assigning {Count} caracteristicas to room {RoomId} by user {UserId}", 
                assignmentDto.CaracteristicaIds?.Count() ?? 0, roomId, this.GetCurrentUserId());

            var result = await _caracteristicasService.AssignToRoomAsync(
                roomId, 
                assignmentDto.CaracteristicaIds ?? Enumerable.Empty<int>(), 
                cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return result.Errors.Contains("no encontrada") || result.Errors.Contains("no existen")
                ? NotFound(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Gets the icon image for a caracteristica
        /// </summary>
        /// <param name="id">Caracteristica ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Image file</returns>
        [HttpGet("{id:int}/image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCaracteristicaImage(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse.Failure("ID de característica inválido"));
            }

            var result = await _caracteristicasService.GetImageAsync(id, cancellationToken);
            
            if (result.IsSuccess && result.Data != null)
            {
                return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
            }

            return result.Errors.Contains("no encontrada")
                ? NotFound(result)
                : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        /// <summary>
        /// Health check endpoint for the Caracteristicas V1 service
        /// </summary>
        /// <returns>Service health status</returns>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new
            {
                service = "CaracteristicasService V1",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}