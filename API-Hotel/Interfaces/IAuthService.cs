using hotel.DTOs.Auth;
using hotel.DTOs.Common;

namespace hotel.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest);
    Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterRequestDto registerRequest);
    Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordRequestDto changePasswordRequest);
    Task<ApiResponse<LoginResponseDto>> ForceChangePasswordAsync(string userId, ForceChangePasswordRequestDto forceChangePasswordRequest);
    Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(string userId);
    Task<ApiResponse> LogoutAsync();
}