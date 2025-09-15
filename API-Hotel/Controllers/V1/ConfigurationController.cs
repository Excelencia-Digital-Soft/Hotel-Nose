using System.Security.Claims;
using hotel.Data;
using hotel.DTOs;
using hotel.DTOs.Common;
using hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotel.Controllers.V1;

[ApiController]
[Route("api/v1/configuration")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ConfigurationController : ControllerBase
{
    private readonly HotelDbContext _context;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(HotelDbContext context, ILogger<ConfigurationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get timer update interval configuration
    /// </summary>
    /// <returns>Timer update interval in minutes</returns>
    /// <remarks>
    /// Requires JWT Bearer token from V1 authentication endpoint.
    /// 
    /// Example:
    /// Authorization: Bearer {your_jwt_token}
    /// 
    /// Get token from: POST /api/v1/authentication/login
    /// </remarks>
    [HttpGet("timer-update-interval")]
    [ProducesResponseType(
        typeof(ApiResponse<TimerUpdateIntervalResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<ApiResponse<TimerUpdateIntervalResponseDto>>
    > GetTimerUpdateInterval()
    {
        try
        {
            var institucionId = GetCurrentInstitucionId();

            var config = await _context
                .Configuraciones.Where(c =>
                    c.Clave == "TIMER_UPDATE_INTERVAL"
                    && c.Activo
                    && (c.InstitucionId == institucionId || c.InstitucionId == null)
                )
                .OrderBy(c => c.InstitucionId == null ? 1 : 0) // Prioritize institution-specific config
                .FirstOrDefaultAsync();

            if (config == null)
            {
                // Return default value if no configuration found
                var defaultResponse = new TimerUpdateIntervalResponseDto
                {
                    IntervalMinutos = 10, // Default 10 minutes
                    Descripcion = "Default timer update interval",
                    FechaModificacion = null,
                };

                return Ok(
                    ApiResponse<TimerUpdateIntervalResponseDto>.Success(
                        defaultResponse,
                        "Using default timer update interval"
                    )
                );
            }

            if (!int.TryParse(config.Valor, out int intervalMinutos))
            {
                _logger.LogWarning("Invalid timer update interval value: {Value}", config.Valor);
                return BadRequest(
                    ApiResponse.Failure("Invalid timer update interval configuration")
                );
            }

            var response = new TimerUpdateIntervalResponseDto
            {
                IntervalMinutos = intervalMinutos,
                Descripcion = config.Descripcion,
                FechaModificacion = config.FechaModificacion,
            };

            return Ok(ApiResponse<TimerUpdateIntervalResponseDto>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving timer update interval");
            return StatusCode(500, ApiResponse.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Update timer update interval configuration
    /// </summary>
    /// <param name="dto">Timer update interval data</param>
    /// <returns>Updated timer update interval configuration</returns>
    [HttpPut("timer-update-interval")]
    [ProducesResponseType(
        typeof(ApiResponse<TimerUpdateIntervalResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    public async Task<
        ActionResult<ApiResponse<TimerUpdateIntervalResponseDto>>
    > UpdateTimerUpdateInterval([FromBody] TimerUpdateIntervalDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse.Failure(errors, "Invalid request data"));
        }

        try
        {
            var institucionId = GetCurrentInstitucionId();

            var config = await _context
                .Configuraciones.Where(c =>
                    c.Clave == "TIMER_UPDATE_INTERVAL" && c.InstitucionId == institucionId
                )
                .FirstOrDefaultAsync();

            if (config == null)
            {
                // Create new configuration
                config = new Configuracion
                {
                    Clave = "TIMER_UPDATE_INTERVAL",
                    Valor = dto.IntervalMinutos.ToString(),
                    Descripcion = dto.Descripcion ?? "Timer update interval configuration",
                    Categoria = "SYSTEM",
                    InstitucionId = institucionId,
                    FechaCreacion = DateTime.UtcNow,
                    FechaModificacion = DateTime.UtcNow,
                    Activo = true,
                };

                _context.Configuraciones.Add(config);
                await _context.SaveChangesAsync();

                var createdResponse = new TimerUpdateIntervalResponseDto
                {
                    IntervalMinutos = dto.IntervalMinutos,
                    Descripcion = config.Descripcion,
                    FechaModificacion = config.FechaModificacion,
                };

                return CreatedAtAction(
                    nameof(GetTimerUpdateInterval),
                    null,
                    ApiResponse<TimerUpdateIntervalResponseDto>.Success(
                        createdResponse,
                        "Timer update interval configuration created"
                    )
                );
            }
            else
            {
                // Update existing configuration
                config.Valor = dto.IntervalMinutos.ToString();
                config.Descripcion = dto.Descripcion ?? config.Descripcion;
                config.FechaModificacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var updatedResponse = new TimerUpdateIntervalResponseDto
                {
                    IntervalMinutos = dto.IntervalMinutos,
                    Descripcion = config.Descripcion,
                    FechaModificacion = config.FechaModificacion,
                };

                return Ok(
                    ApiResponse<TimerUpdateIntervalResponseDto>.Success(
                        updatedResponse,
                        "Timer update interval configuration updated"
                    )
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating timer update interval");
            return StatusCode(500, ApiResponse.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Get all configuration settings (for administrative purposes)
    /// </summary>
    /// <param name="categoria">Optional filter by category</param>
    /// <returns>List of configuration settings</returns>
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<ConfiguracionDto>>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<ApiResponse<IEnumerable<ConfiguracionDto>>>> GetConfigurations(
        [FromQuery] string? categoria = null
    )
    {
        try
        {
            var institucionId = GetCurrentInstitucionId();

            var query = _context.Configuraciones.Where(c =>
                c.Activo && (c.InstitucionId == institucionId || c.InstitucionId == null)
            );

            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(c => c.Categoria == categoria);
            }

            var configs = await query.OrderBy(c => c.Categoria).ThenBy(c => c.Clave).ToListAsync();

            var response = configs.Select(c => new ConfiguracionDto
            {
                ConfiguracionId = c.ConfiguracionId,
                Clave = c.Clave,
                Valor = c.Valor,
                Descripcion = c.Descripcion,
                Categoria = c.Categoria,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion,
                Activo = c.Activo,
                InstitucionId = c.InstitucionId,
            });

            return Ok(ApiResponse<IEnumerable<ConfiguracionDto>>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configurations");
            return StatusCode(500, ApiResponse.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Get specific configuration by key
    /// </summary>
    /// <param name="clave">Configuration key</param>
    /// <returns>Configuration setting</returns>
    [HttpGet("{clave}")]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ConfiguracionDto>>> GetConfiguration(string clave)
    {
        try
        {
            var institucionId = GetCurrentInstitucionId();

            var config = await _context
                .Configuraciones.Where(c =>
                    c.Clave == clave
                    && c.Activo
                    && (c.InstitucionId == institucionId || c.InstitucionId == null)
                )
                .OrderBy(c => c.InstitucionId == null ? 1 : 0) // Prioritize institution-specific config
                .FirstOrDefaultAsync();

            if (config == null)
            {
                return NotFound(ApiResponse.Failure($"Configuration with key '{clave}' not found"));
            }

            var response = new ConfiguracionDto
            {
                ConfiguracionId = config.ConfiguracionId,
                Clave = config.Clave,
                Valor = config.Valor,
                Descripcion = config.Descripcion,
                Categoria = config.Categoria,
                FechaCreacion = config.FechaCreacion,
                FechaModificacion = config.FechaModificacion,
                Activo = config.Activo,
                InstitucionId = config.InstitucionId,
            };

            return Ok(ApiResponse<ConfiguracionDto>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration with key: {Key}", clave);
            return StatusCode(500, ApiResponse.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Create new configuration setting
    /// </summary>
    /// <param name="dto">Configuration data</param>
    /// <returns>Created configuration setting</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ConfiguracionDto>>> CreateConfiguration(
        [FromBody] ConfiguracionCreateDto dto
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

        try
        {
            var institucionId = GetCurrentInstitucionId();

            // Check if configuration already exists
            var existingConfig = await _context
                .Configuraciones.Where(c =>
                    c.Clave == dto.Clave && c.InstitucionId == (dto.InstitucionId ?? institucionId)
                )
                .FirstOrDefaultAsync();

            if (existingConfig != null)
            {
                return Conflict(
                    ApiResponse.Failure($"Configuration with key '{dto.Clave}' already exists")
                );
            }

            var config = new Configuracion
            {
                Clave = dto.Clave,
                Valor = dto.Valor,
                Descripcion = dto.Descripcion,
                Categoria = dto.Categoria,
                InstitucionId = dto.InstitucionId ?? institucionId,
                FechaCreacion = DateTime.UtcNow,
                Activo = true,
            };

            _context.Configuraciones.Add(config);
            await _context.SaveChangesAsync();

            var response = new ConfiguracionDto
            {
                ConfiguracionId = config.ConfiguracionId,
                Clave = config.Clave,
                Valor = config.Valor,
                Descripcion = config.Descripcion,
                Categoria = config.Categoria,
                FechaCreacion = config.FechaCreacion,
                FechaModificacion = config.FechaModificacion,
                Activo = config.Activo,
                InstitucionId = config.InstitucionId,
            };

            return CreatedAtAction(
                nameof(GetConfiguration),
                new { clave = config.Clave },
                ApiResponse<ConfiguracionDto>.Success(
                    response,
                    "Configuration created successfully"
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating configuration");
            return StatusCode(500, ApiResponse.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Update existing configuration setting
    /// </summary>
    /// <param name="clave">Configuration key</param>
    /// <param name="dto">Updated configuration data</param>
    /// <returns>Updated configuration setting</returns>
    [HttpPut("{clave}")]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ConfiguracionDto>>> UpdateConfiguration(
        string clave,
        [FromBody] ConfiguracionUpdateDto dto
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

        try
        {
            var institucionId = GetCurrentInstitucionId();

            var config = await _context
                .Configuraciones.Where(c =>
                    c.Clave == clave
                    && (c.InstitucionId == institucionId || c.InstitucionId == null)
                )
                .FirstOrDefaultAsync();

            if (config == null)
            {
                return NotFound(ApiResponse.Failure($"Configuration with key '{clave}' not found"));
            }

            config.Valor = dto.Valor;
            config.Descripcion = dto.Descripcion ?? config.Descripcion;
            config.Categoria = dto.Categoria ?? config.Categoria;
            config.Activo = dto.Activo ?? config.Activo;
            config.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new ConfiguracionDto
            {
                ConfiguracionId = config.ConfiguracionId,
                Clave = config.Clave,
                Valor = config.Valor,
                Descripcion = config.Descripcion,
                Categoria = config.Categoria,
                FechaCreacion = config.FechaCreacion,
                FechaModificacion = config.FechaModificacion,
                Activo = config.Activo,
                InstitucionId = config.InstitucionId,
            };

            return Ok(
                ApiResponse<ConfiguracionDto>.Success(
                    response,
                    "Configuration updated successfully"
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration with key: {Key}", clave);
            return StatusCode(500, ApiResponse.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Delete configuration setting (soft delete)
    /// </summary>
    /// <param name="clave">Configuration key</param>
    /// <returns>Delete confirmation</returns>
    [HttpDelete("{clave}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteConfiguration(string clave)
    {
        try
        {
            var institucionId = GetCurrentInstitucionId();

            var config = await _context
                .Configuraciones.Where(c =>
                    c.Clave == clave
                    && (c.InstitucionId == institucionId || c.InstitucionId == null)
                )
                .FirstOrDefaultAsync();

            if (config == null)
            {
                return NotFound(ApiResponse.Failure($"Configuration with key '{clave}' not found"));
            }

            // Soft delete
            config.Activo = false;
            config.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Success("Configuration deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration with key: {Key}", clave);
            return StatusCode(500, ApiResponse.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Test endpoint to verify controller is working (no authentication required)
    /// </summary>
    /// <returns>Test response</returns>
    [HttpGet("test")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> TestEndpoint()
    {
        var response = new
        {
            message = "ConfigurationController is working correctly",
            timestamp = DateTime.UtcNow,
            version = "v1",
            authenticationScheme = "Bearer JWT",
            note = "This endpoint requires no authentication. Use GET /timer-update-interval with Bearer token for authenticated access."
        };
        
        return Ok(ApiResponse<object>.Success(response, "Test endpoint successful"));
    }

    /// <summary>
    /// Simple health check endpoint with no authentication (for debugging 302 issues)
    /// </summary>
    /// <returns>Simple health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Get authentication status and user claims (for debugging)
    /// </summary>
    /// <returns>Authentication status and claims</returns>
    [HttpGet("auth-status")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<object>> GetAuthStatus()
    {
        var response = new
        {
            isAuthenticated = User.Identity?.IsAuthenticated ?? false,
            userName = User.Identity?.Name,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
            institucionId = GetCurrentInstitucionId()
        };
        
        return Ok(ApiResponse<object>.Success(response, "Authentication status retrieved"));
    }

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
        return null;
    }
}

