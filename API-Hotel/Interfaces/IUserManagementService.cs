using hotel.DTOs.Common;
using hotel.DTOs.V1;

namespace hotel.Interfaces;

public interface IUserManagementService
{
    Task<ApiResponse<List<UserManagementDto>>> GetAllUsersAsync(string currentUserId);
    Task<ApiResponse<List<RoleDto>>> GetAllRolesAsync();
    Task<ApiResponse<UserRolesResponseDto>> GetUserRolesAsync(string userId);
    Task<ApiResponse> UpdateUserRolesAsync(UpdateUserRolesRequestDto updateRequest);
    Task<ApiResponse> UpdateUserStatusAsync(BlockUserRequestDto blockRequest, string currentUserId);
    Task<ApiResponse> ChangeUserPasswordAsync(ChangeUserPasswordRequestDto changePasswordRequest);
    Task<ApiResponse> UpdateUserAsync(string userId, UpdateUserRequestDto updateRequest, string currentUserId);
}