using hotel.DTOs.Common;
using hotel.DTOs.V1;
using hotel.Interfaces;
using hotel.Models.Identity;
using hotel.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly HotelDbContext _context;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        HotelDbContext context,
        ILogger<UserManagementService> logger
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<UserManagementDto>>> GetAllUsersAsync(string currentUserId)
    {
        try
        {
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return ApiResponse<List<UserManagementDto>>.Failure("Current user not found");
            }

            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            var isAdministrator = currentUserRoles.Contains("Administrator");

            IQueryable<ApplicationUser> usersQuery = _userManager.Users.Include(u => u.Institucion);

            // Si no es Administrator, filtrar por institución
            if (!isAdministrator)
            {
                if (currentUser.InstitucionId == null)
                {
                    return ApiResponse<List<UserManagementDto>>.Failure("Current user has no institution assigned");
                }
                usersQuery = usersQuery.Where(u => u.InstitucionId == currentUser.InstitucionId);
            }

            var users = await usersQuery.ToListAsync();
            var userDtos = new List<UserManagementDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userDtos.Add(
                    new UserManagementDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber,
                        InstitucionId = user.InstitucionId,
                        InstitucionName = user.Institucion?.Nombre,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt,
                        IsActive = user.IsActive,
                        ForcePasswordChange = user.ForcePasswordChange,
                        Roles = roles,
                    }
                );
            }

            return ApiResponse<List<UserManagementDto>>.Success(
                userDtos,
                "Users retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return ApiResponse<List<UserManagementDto>>.Failure("Failed to retrieve users");
        }
    }

    public async Task<ApiResponse<List<RoleDto>>> GetAllRolesAsync()
    {
        try
        {
            var roles = await _roleManager.Roles.ToListAsync();

            var roleDtos = roles
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name ?? string.Empty,
                    Description = r.Description,
                    CreatedAt = r.CreatedAt,
                    IsActive = r.IsActive,
                })
                .ToList();

            return ApiResponse<List<RoleDto>>.Success(roleDtos, "Roles retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return ApiResponse<List<RoleDto>>.Failure("Failed to retrieve roles");
        }
    }

    public async Task<ApiResponse<UserRolesResponseDto>> GetUserRolesAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserRolesResponseDto>.Failure("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userRolesDto = new UserRolesResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName,
                Roles = roles,
            };

            return ApiResponse<UserRolesResponseDto>.Success(
                userRolesDto,
                "User roles retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles for user {UserId}", userId);
            return ApiResponse<UserRolesResponseDto>.Failure("Failed to retrieve user roles");
        }
    }

    public async Task<ApiResponse> UpdateUserRolesAsync(UpdateUserRolesRequestDto updateRequest)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(updateRequest.UserId);
            if (user == null)
            {
                return ApiResponse.Failure("User not found");
            }

            // Verify that all requested roles exist
            foreach (var roleName in updateRequest.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    return ApiResponse.Failure($"Role '{roleName}' does not exist");
                }
            }

            // Get current user roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove all current roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                var errors = removeResult.Errors.Select(e => e.Description).ToList();
                return ApiResponse.Failure(errors, "Failed to remove current roles");
            }

            // Add new roles
            if (updateRequest.Roles.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, updateRequest.Roles);
                if (!addResult.Succeeded)
                {
                    var errors = addResult.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.Failure(errors, "Failed to add new roles");
                }
            }

            _logger.LogInformation(
                "User roles updated successfully for user {UserId}",
                updateRequest.UserId
            );
            return ApiResponse.Success("User roles updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating user roles for user {UserId}",
                updateRequest.UserId
            );
            return ApiResponse.Failure("Failed to update user roles");
        }
    }

    public async Task<ApiResponse> UpdateUserStatusAsync(
        BlockUserRequestDto blockRequest,
        string currentUserId
    )
    {
        try
        {
            // Prevent administrators from blocking themselves
            if (currentUserId == blockRequest.UserId)
            {
                return ApiResponse.Failure("You cannot modify your own account status");
            }

            var user = await _userManager.FindByIdAsync(blockRequest.UserId);
            if (user == null)
            {
                return ApiResponse.Failure("User not found");
            }

            user.IsActive = blockRequest.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.Failure(errors, "Failed to update user status");
            }

            var statusMessage = blockRequest.IsActive ? "activated" : "blocked";
            _logger.LogInformation(
                "User {statusMessage} successfully: {UserId}",
                statusMessage,
                blockRequest.UserId
            );
            return ApiResponse.Success($"User {statusMessage} successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating user status for user {UserId}",
                blockRequest.UserId
            );
            return ApiResponse.Failure("Failed to update user status");
        }
    }

    public async Task<ApiResponse> ChangeUserPasswordAsync(
        ChangeUserPasswordRequestDto changePasswordRequest
    )
    {
        try
        {
            var user = await _userManager.FindByIdAsync(changePasswordRequest.UserId);
            if (user == null)
            {
                return ApiResponse.Failure("User not found");
            }

            // Generate a password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset the password using the token
            var result = await _userManager.ResetPasswordAsync(
                user,
                resetToken,
                changePasswordRequest.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.Failure(errors, "Failed to change user password");
            }

            // Optionally set ForcePasswordChange to false since admin changed it
            user.ForcePasswordChange = false;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation(
                "Password changed successfully for user {UserId}",
                changePasswordRequest.UserId
            );
            return ApiResponse.Success("User password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error changing password for user {UserId}",
                changePasswordRequest.UserId
            );
            return ApiResponse.Failure("Failed to change user password");
        }
    }

    public async Task<ApiResponse> UpdateUserAsync(
        string userId,
        UpdateUserRequestDto updateRequest,
        string currentUserId
    )
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.Failure("User not found");
            }

            // Update user properties if provided
            if (!string.IsNullOrEmpty(updateRequest.Email))
            {
                user.Email = updateRequest.Email;
                user.NormalizedEmail = updateRequest.Email.ToUpperInvariant();
            }

            if (!string.IsNullOrEmpty(updateRequest.UserName))
            {
                user.UserName = updateRequest.UserName;
                user.NormalizedUserName = updateRequest.UserName.ToUpperInvariant();
            }

            if (updateRequest.FirstName != null)
                user.FirstName = updateRequest.FirstName;

            if (updateRequest.LastName != null)
                user.LastName = updateRequest.LastName;

            if (updateRequest.PhoneNumber != null)
                user.PhoneNumber = updateRequest.PhoneNumber;

            if (updateRequest.InstitucionId.HasValue)
                user.InstitucionId = updateRequest.InstitucionId.Value;

            if (updateRequest.ForcePasswordChange.HasValue)
                user.ForcePasswordChange = updateRequest.ForcePasswordChange.Value;

            // Handle IsActive status update
            if (updateRequest.IsActive.HasValue)
            {
                // Prevent administrators from blocking themselves
                if (currentUserId == userId && !updateRequest.IsActive.Value)
                {
                    return ApiResponse.Failure("You cannot deactivate your own account");
                }
                user.IsActive = updateRequest.IsActive.Value;
            }

            // Update user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                return ApiResponse.Failure(errors, "Failed to update user");
            }

            // Update roles if provided
            if (updateRequest.Roles != null)
            {
                // Verify that all requested roles exist
                foreach (var roleName in updateRequest.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        return ApiResponse.Failure($"Role '{roleName}' does not exist");
                    }
                }

                // Get current user roles
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove all current roles
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    var errors = removeResult.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.Failure(errors, "Failed to remove current roles");
                }

                // Add new roles
                if (updateRequest.Roles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, updateRequest.Roles);
                    if (!addResult.Succeeded)
                    {
                        var errors = addResult.Errors.Select(e => e.Description).ToList();
                        return ApiResponse.Failure(errors, "Failed to add new roles");
                    }
                }
            }

            _logger.LogInformation("User updated successfully: {UserId}", userId);
            return ApiResponse.Success("User updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return ApiResponse.Failure("Failed to update user");
        }
    }
}

