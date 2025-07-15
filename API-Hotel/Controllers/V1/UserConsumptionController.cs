using System.Security.Claims;
using hotel.DTOs.Common;
using hotel.DTOs.UserConsumption;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers.V1;

[ApiController]
[Route("api/v1/user-consumption")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UserConsumptionController : ControllerBase
{
    private readonly IUserConsumptionService _userConsumptionService;
    private readonly ILogger<UserConsumptionController> _logger;

    public UserConsumptionController(
        IUserConsumptionService userConsumptionService,
        ILogger<UserConsumptionController> logger
    )
    {
        _userConsumptionService =
            userConsumptionService
            ?? throw new ArgumentNullException(nameof(userConsumptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("my-consumption")]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<UserConsumptionDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserConsumptionDto>>>> GetMyConsumption(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID not found"));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _userConsumptionService.GetUserConsumptionAsync(
            userId,
            institucionId.Value,
            startDate,
            endDate,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpGet("my-summary")]
    [ProducesResponseType(typeof(ApiResponse<UserConsumptionSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserConsumptionSummaryDto>>> GetMyConsumptionSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID not found"));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _userConsumptionService.GetUserConsumptionSummaryAsync(
            userId,
            institucionId.Value,
            startDate,
            endDate,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpGet("my-consumption/by-service")]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<UserConsumptionByServiceDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<ApiResponse<IEnumerable<UserConsumptionByServiceDto>>>
    > GetMyConsumptionByService(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID not found"));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _userConsumptionService.GetConsumptionByServiceAsync(
            userId,
            institucionId.Value,
            startDate,
            endDate,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<UserConsumptionDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Administrator,Director")]
    public async Task<
        ActionResult<ApiResponse<IEnumerable<UserConsumptionDto>>>
    > GetUserConsumption(
        string userId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _userConsumptionService.GetUserConsumptionAsync(
            userId,
            institucionId.Value,
            startDate,
            endDate,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpGet("user/{userId}/summary")]
    [ProducesResponseType(typeof(ApiResponse<UserConsumptionSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Administrator,Director")]
    public async Task<
        ActionResult<ApiResponse<UserConsumptionSummaryDto>>
    > GetUserConsumptionSummary(
        string userId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _userConsumptionService.GetUserConsumptionSummaryAsync(
            userId,
            institucionId.Value,
            startDate,
            endDate,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpGet("all")]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<UserConsumptionDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Administrator,Director")]
    public async Task<
        ActionResult<ApiResponse<IEnumerable<UserConsumptionDto>>>
    > GetAllUsersConsumption(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _userConsumptionService.GetAllUsersConsumptionAsync(
            institucionId.Value,
            startDate,
            endDate,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserConsumptionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserConsumptionDto>>> RegisterConsumption(
        [FromBody] UserConsumptionCreateDto createDto,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors));
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID not found"));
        }

        var institucionId = GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _userConsumptionService.RegisterConsumptionAsync(
            createDto,
            userId,
            institucionId.Value,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetMyConsumption), new { }, result);
        }

        return StatusCode(500, result);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(
            new
            {
                service = "UserConsumptionService V1",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
            }
        );
    }

    #region Private Methods
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
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

        _logger.LogWarning(
            "Institution ID not found in user claims for user {UserId}",
            User.FindFirstValue(ClaimTypes.NameIdentifier)
        );

        return null;
    }
    #endregion
}

