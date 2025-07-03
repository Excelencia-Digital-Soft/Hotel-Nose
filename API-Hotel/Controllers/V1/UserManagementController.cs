using System.Security.Claims;
using hotel.DTOs.Common;
using hotel.DTOs.V1;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers.V1;

[ApiController]
[Route("api/v1/user-management")]
[Authorize(Roles = "Administrator, Director")]
[Produces("application/json")]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        IUserManagementService userManagementService,
        ILogger<UserManagementController> logger
    )
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with their roles
    /// </summary>
    /// <returns>List of all users with their assigned roles</returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<List<UserManagementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<List<UserManagementDto>>),
        StatusCodes.Status403Forbidden
    )]
    public async Task<ActionResult<ApiResponse<List<UserManagementDto>>>> GetAllUsers()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(ApiResponse<List<UserManagementDto>>.Failure("User not authenticated"));
        }

        var result = await _userManagementService.GetAllUsersAsync(currentUserId);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all available roles
    /// </summary>
    /// <returns>List of all available roles</returns>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAllRoles()
    {
        var result = await _userManagementService.GetAllRolesAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get roles for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User roles information</returns>
    [HttpGet("users/{userId}/roles")]
    [ProducesResponseType(typeof(ApiResponse<UserRolesResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserRolesResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserRolesResponseDto>>> GetUserRoles(string userId)
    {
        var result = await _userManagementService.GetUserRolesAsync(userId);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update user roles
    /// </summary>
    /// <param name="updateRequest">User roles update request</param>
    /// <returns>Update result</returns>
    [HttpPut("users/roles")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateUserRoles(
        [FromBody] UpdateUserRolesRequestDto updateRequest
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

        var result = await _userManagementService.UpdateUserRolesAsync(updateRequest);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Block or unblock a user
    /// </summary>
    /// <param name="blockRequest">Block user request</param>
    /// <returns>Block/unblock result</returns>
    [HttpPut("users/status")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateUserStatus(
        [FromBody] BlockUserRequestDto blockRequest
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

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(ApiResponse.Failure("User not authenticated"));
        }

        var result = await _userManagementService.UpdateUserStatusAsync(
            blockRequest,
            currentUserId
        );

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Change password for any user by ID
    /// </summary>
    /// <param name="changePasswordRequest">Change password request</param>
    /// <returns>Password change result</returns>
    [HttpPut("users/password")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> ChangeUserPassword(
        [FromBody] ChangeUserPasswordRequestDto changePasswordRequest
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

        var result = await _userManagementService.ChangeUserPasswordAsync(changePasswordRequest);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update user data and roles
    /// </summary>
    /// <param name="userId">User ID to update</param>
    /// <param name="updateRequest">User update request with optional data and roles</param>
    /// <returns>Update result</returns>
    [HttpPut("users/{userId}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateUser(
        string userId,
        [FromBody] UpdateUserRequestDto updateRequest
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

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(ApiResponse.Failure("User not authenticated"));
        }

        var result = await _userManagementService.UpdateUserAsync(
            userId,
            updateRequest,
            currentUserId
        );

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
