using hotel.DTOs.Common;
using hotel.DTOs.Categorias;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace hotel.Controllers.V1;

[ApiController]
[Route("api/v1/categorias")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriasService _categoriasService;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(
        ICategoriasService categoriasService,
        ILogger<CategoriasController> logger)
    {
        _categoriasService = categoriasService ?? throw new ArgumentNullException(nameof(categoriasService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoriaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoriaDto>>>> GetCategorias(
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _categoriasService.GetAllAsync(institucionId.Value, cancellationToken);
        
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoriaDto>>> GetCategoria(
        int id,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _categoriasService.GetByIdAsync(id, institucionId.Value, cancellationToken);
        
        if (!result.IsSuccess && result.Errors.Any(e => e.Contains("not found")))
        {
            return NotFound(result);
        }
        
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoriaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CategoriaDto>>> CreateCategoria(
        [FromBody] CategoriaCreateDto createDto,
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
        var result = await _categoriasService.CreateAsync(createDto, institucionId.Value, userId, cancellationToken);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetCategoria),
                new { id = result.Data!.CategoriaId },
                result);
        }
        
        return StatusCode(500, result);
    }

    [HttpPost("with-image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<CategoriaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CategoriaDto>>> CreateCategoriaWithImage(
        [FromForm] CategoriaCreateWithImageDto createDto,
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
        var result = await _categoriasService.CreateWithImageAsync(createDto, institucionId.Value, userId, cancellationToken);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetCategoria),
                new { id = result.Data!.CategoriaId },
                result);
        }
        
        return StatusCode(500, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoriaDto>>> UpdateCategoria(
        int id,
        [FromBody] CategoriaUpdateDto updateDto,
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
        var result = await _categoriasService.UpdateAsync(id, updateDto, institucionId.Value, userId, cancellationToken);
        
        if (!result.IsSuccess && result.Errors.Any(e => e.Contains("not found")))
        {
            return NotFound(result);
        }
        
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

   [HttpPatch("{id:int}/image")]
[Consumes("multipart/form-data")]
[ProducesResponseType(typeof(ApiResponse<CategoriaDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
public async Task<ActionResult<ApiResponse<CategoriaDto>>> UpdateCategoriaImage(
    int id,
    [FromForm] CategoriaUpdateImageDto updateDto,
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

    var imagen = updateDto.Imagen;
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
    var result = await _categoriasService.UpdateImageAsync(id, imagen, institucionId.Value, userId, cancellationToken);

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
    public async Task<ActionResult<ApiResponse>> DeleteCategoria(
        int id,
        CancellationToken cancellationToken = default)
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _categoriasService.DeleteAsync(id, institucionId.Value, cancellationToken);
        
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
    public async Task<ActionResult<ApiResponse>> ToggleCategoriaStatus(
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
        var result = await _categoriasService.ToggleStatusAsync(id, request.Anulado, institucionId.Value, userId, cancellationToken);
        
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
    public async Task<IActionResult> GetCategoriaImage(
        int id,
        CancellationToken cancellationToken = default)
    {
        // For public image access, we might need to get institucionId from the category itself
        // or make images publicly accessible. For now, we'll try to get it from token if available
        var institucionId = GetCurrentInstitucionId();
        
        // If no institution from token, we need to get it from the category
        if (!institucionId.HasValue)
        {
            var categoriaResult = await _categoriasService.GetByIdAsync(id, 0, cancellationToken); // temp workaround
            if (!categoriaResult.IsSuccess)
            {
                return NotFound(new { message = "Category not found" });
            }
            // In a real scenario, you'd need to modify this logic or make images publicly accessible
        }

        var imageResult = await _categoriasService.GetImageAsync(id, institucionId ?? 0, cancellationToken);
        var contentTypeResult = await _categoriasService.GetImageContentTypeAsync(id, institucionId ?? 0, cancellationToken);
        
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
            service = "CategoriasService V1", 
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