using hotel.DTOs.Common;
using hotel.DTOs.Articulos;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace hotel.Controllers.V1;

[ApiController]
[Route("api/v1/articulos")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ArticulosController : ControllerBase
{
    private readonly IArticulosService _articulosService;
    private readonly ILogger<ArticulosController> _logger;

    public ArticulosController(
        IArticulosService articulosService,
        ILogger<ArticulosController> logger)
    {
        _articulosService = articulosService ?? throw new ArgumentNullException(nameof(articulosService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticuloDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticuloDto>>>> GetArticulos(
        [FromQuery] int? categoriaId = null,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _articulosService.GetAllAsync(
            institucionId.Value, categoriaId, cancellationToken);
        
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ArticuloDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticuloDto>>> GetArticulo(
        int id,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _articulosService.GetByIdAsync(id, institucionId.Value, cancellationToken);
        
        if (!result.IsSuccess && result.Errors.Any(e => e.Contains("not found")))
        {
            return NotFound(result);
        }
        
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ArticuloDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ArticuloDto>>> CreateArticulo(
        [FromBody] ArticuloCreateDto createDto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = GetCurrentUserId();
        var result = await _articulosService.CreateAsync(createDto, institucionId.Value, userId, cancellationToken);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetArticulo),
                new { id = result.Data!.ArticuloId },
                result);
        }
        
        return StatusCode(500, result);
    }

    [HttpPost("with-image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ArticuloDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ArticuloDto>>> CreateArticuloWithImage(
        [FromForm] ArticuloCreateWithImageDto createDto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = GetCurrentUserId();
        var result = await _articulosService.CreateWithImageAsync(createDto, institucionId.Value, userId, cancellationToken);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetArticulo),
                new { id = result.Data!.ArticuloId },
                result);
        }
        
        return StatusCode(500, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ArticuloDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticuloDto>>> UpdateArticulo(
        int id,
        [FromBody] ArticuloUpdateDto updateDto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = GetCurrentUserId();
        var result = await _articulosService.UpdateAsync(id, updateDto, institucionId.Value, userId, cancellationToken);
        
        if (!result.IsSuccess && result.Errors.Any(e => e.Contains("not found")))
        {
            return NotFound(result);
        }
        
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpPatch("{id:int}/image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ArticuloDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticuloDto>>> UpdateArticuloImage(
        int id,
        [FromForm] IFormFile imagen,
        CancellationToken cancellationToken = default)
    {
        if (imagen == null || imagen.Length == 0)
        {
            return BadRequest(ApiResponse.Failure("Image file is required"));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = GetCurrentUserId();
        var result = await _articulosService.UpdateImageAsync(id, imagen, institucionId.Value, userId, cancellationToken);
        
        if (!result.IsSuccess && result.Errors.Any(e => e.Contains("not found")))
        {
            return NotFound(result);
        }
        
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    [Authorize(Roles = "Administrator,Director")]
    public async Task<ActionResult<ApiResponse>> DeleteArticulo(
        int id,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _articulosService.DeleteAsync(id, institucionId.Value, cancellationToken);
        
        if (!result.IsSuccess && result.Errors.Any(e => e.Contains("not found")))
        {
            return NotFound(result);
        }
        
        if (!result.IsSuccess && result.Errors.Any(e => e.Contains("Cannot delete")))
        {
            return Conflict(result);
        }
        
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Administrator,Director,Manager")]
    public async Task<ActionResult<ApiResponse>> ToggleArticuloStatus(
        int id,
        [FromBody] ToggleStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = GetCurrentUserId();
        var result = await _articulosService.ToggleStatusAsync(id, request.Anulado, institucionId.Value, userId, cancellationToken);
        
        if (!result.IsSuccess && result.Errors.Any(e => e.Contains("not found")))
        {
            return NotFound(result);
        }
        
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpGet("{id:int}/image")]
    [ProducesResponseType(typeof(hotel.Interfaces.FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [AllowAnonymous] // Allow public access to images
    public async Task<IActionResult> GetArticuloImage(
        int id,
        CancellationToken cancellationToken = default)
    {
        // For public image access, we might need to get institucionId from the article itself
        // or make images publicly accessible. For now, we'll try to get it from token if available
        var institucionId = GetCurrentInstitucionId();
        
        // If no institution from token, we need to get it from the article
        if (!institucionId.HasValue)
        {
            var articuloResult = await _articulosService.GetByIdAsync(id, 0, cancellationToken); // temp workaround
            if (!articuloResult.IsSuccess)
            {
                return NotFound(new { message = "Article not found" });
            }
            // In a real scenario, you'd need to modify this logic or make images publicly accessible
        }

        var imageResult = await _articulosService.GetImageAsync(id, institucionId ?? 0, cancellationToken);
        var contentTypeResult = await _articulosService.GetImageContentTypeAsync(id, institucionId ?? 0, cancellationToken);
        
        if (!imageResult.IsSuccess || !contentTypeResult.IsSuccess)
        {
            return NotFound(new { message = "Image not found" });
        }

        return File(imageResult.Data!, contentTypeResult.Data!);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            service = "ArticulosService V1", 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    #region Private Methods
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

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