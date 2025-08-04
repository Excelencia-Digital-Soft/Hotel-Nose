using hotel.Auth;
using hotel.Data;
using hotel.DTOs.Auth;
using hotel.DTOs.Common;
using hotel.Interfaces;
using hotel.Models;
using hotel.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace hotel.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;
    private readonly HotelDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IRegistrosService _registrosService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtService jwtService,
        HotelDbContext context,
        ILogger<AuthService> logger,
        IRegistrosService registrosService
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _context = context;
        _logger = logger;
        _registrosService = registrosService;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null || !user.IsActive)
            {
                // Registrar auditoría de intento de login con usuario inexistente o inactivo
                await _registrosService.LogSecurityAsync(
                    $"Intento de login fallido para email: {loginRequest.Email} - Usuario no encontrado o inactivo",
                    ModuloSistema.USUARIOS,
                    1, // Institución por defecto ya que no tenemos usuario
                    null,
                    null, // direccionIP se puede obtener del contexto HTTP si es necesario
                    System.Text.Json.JsonSerializer.Serialize(
                        new
                        {
                            Email = loginRequest.Email,
                            FailedLoginTime = DateTime.UtcNow,
                            Reason = user == null ? "User not found" : "User inactive",
                        }
                    )
                );

                return ApiResponse<LoginResponseDto>.Failure(
                    "Invalid email or password",
                    "Authentication failed"
                );
            }

            // Verify password using Identity's password verification
            var isPasswordValid = await _userManager.CheckPasswordAsync(
                user,
                loginRequest.Password
            );

            if (!isPasswordValid)
            {
                // Registrar auditoría de login fallido
                await _registrosService.LogSecurityAsync(
                    $"Intento de login fallido para usuario: {user.UserName} ({user.Email}) - Contraseña incorrecta",
                    ModuloSistema.USUARIOS,
                    user.InstitucionId ?? 1,
                    user.Id,
                    null, // direccionIP se puede obtener del contexto HTTP si es necesario
                    System.Text.Json.JsonSerializer.Serialize(
                        new
                        {
                            UserId = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            FailedLoginTime = DateTime.UtcNow,
                            Reason = "Invalid password",
                            InstitucionId = user.InstitucionId,
                        }
                    )
                );

                return ApiResponse<LoginResponseDto>.Failure(
                    "Invalid email or password",
                    "Authentication failed"
                );
            }

            // Update last login and save any password hash changes
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Registrar auditoría de login exitoso
            await _registrosService.LogSecurityAsync(
                $"Login exitoso para usuario: {user.UserName} ({user.Email})",
                ModuloSistema.USUARIOS,
                user.InstitucionId ?? 1,
                user.Id,
                null, // direccionIP se puede obtener del contexto HTTP si es necesario
                System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        LoginTime = DateTime.UtcNow,
                        InstitucionId = user.InstitucionId,
                    }
                )
            );

            // Generate token
            var token = await _jwtService.GenerateTokenAsync(user);
            var tokenExpiration = DateTime.UtcNow.AddHours(24);

            var userInfo = await MapToUserInfoDto(user);

            var loginResponse = new LoginResponseDto
            {
                Token = token,
                TokenExpiration = tokenExpiration,
                User = userInfo,
            };

            return ApiResponse<LoginResponseDto>.Success(loginResponse, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", loginRequest.Email);
            return ApiResponse<LoginResponseDto>.Failure(
                "An error occurred during login",
                "Login failed"
            );
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> RegisterAsync(
        RegisterRequestDto registerRequest
    )
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);
            if (existingUser != null)
            {
                return ApiResponse<LoginResponseDto>.Failure(
                    "User with this email already exists",
                    "Registration failed"
                );
            }

            // Create new user
            var user = new ApplicationUser
            {
                Email = registerRequest.Email,
                UserName = registerRequest.UserName ?? registerRequest.Email,
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                PhoneNumber = registerRequest.PhoneNumber,
                InstitucionId = registerRequest.InstitucionId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true, // Auto-confirm for now
                ForcePasswordChange = false,
            };

            var result = await _userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<LoginResponseDto>.Failure(errors, "Registration failed");
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "Mucama");

            // Generate token
            var token = await _jwtService.GenerateTokenAsync(user);
            var tokenExpiration = DateTime.UtcNow.AddHours(24);

            var userInfo = await MapToUserInfoDto(user);

            var loginResponse = new LoginResponseDto
            {
                Token = token,
                TokenExpiration = tokenExpiration,
                User = userInfo,
            };

            return ApiResponse<LoginResponseDto>.Success(
                loginResponse,
                "User registered successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error during registration for user {Email}",
                registerRequest.Email
            );
            return ApiResponse<LoginResponseDto>.Failure(
                "An error occurred during registration",
                "Registration failed"
            );
        }
    }

    public async Task<ApiResponse> ChangePasswordAsync(
        string userId,
        ChangePasswordRequestDto changePasswordRequest
    )
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.Failure("User not found", "Password change failed");
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                changePasswordRequest.CurrentPassword,
                changePasswordRequest.NewPassword
            );
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.Failure(errors, "Password change failed");
            }

            return ApiResponse.Success("Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return ApiResponse.Failure(
                "An error occurred while changing password",
                "Password change failed"
            );
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> ForceChangePasswordAsync(
        string userId,
        ForceChangePasswordRequestDto forceChangePasswordRequest
    )
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<LoginResponseDto>.Failure(
                    "User not found",
                    "Password change failed"
                );
            }

            // For forced password change, we don't require the current password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                forceChangePasswordRequest.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<LoginResponseDto>.Failure(errors, "Password change failed");
            }

            // Mark the user as no longer requiring password change
            user.ForcePasswordChange = false;
            await _userManager.UpdateAsync(user);

            // Generate new token with updated claims
            var newToken = await _jwtService.GenerateTokenAsync(user);
            var tokenExpiration = DateTime.UtcNow.AddHours(24);

            var userInfo = await MapToUserInfoDto(user);

            var loginResponse = new LoginResponseDto
            {
                Token = newToken,
                TokenExpiration = tokenExpiration,
                User = userInfo,
            };

            return ApiResponse<LoginResponseDto>.Success(
                loginResponse,
                "Password changed successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forced password change for user {UserId}", userId);
            return ApiResponse<LoginResponseDto>.Failure(
                "An error occurred during password change",
                "Password change failed"
            );
        }
    }

    public async Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserInfoDto>.Failure("User not found");
            }

            var userInfo = await MapToUserInfoDto(user);
            return ApiResponse<UserInfoDto>.Success(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user {UserId}", userId);
            return ApiResponse<UserInfoDto>.Failure(
                "An error occurred while retrieving user information"
            );
        }
    }

    public async Task<ApiResponse> LogoutAsync()
    {
        try
        {
            await _signInManager.SignOutAsync();
            return ApiResponse.Success("Logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return ApiResponse.Failure("An error occurred during logout");
        }
    }

    private async Task<UserInfoDto> MapToUserInfoDto(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        // Temporalmente comentamos el acceso a Instituciones debido a un problema de esquema
        // TODO: Arreglar el esquema de la tabla Instituciones
        string? institucionName = null;
        /*
        var institucion = user.InstitucionId.HasValue
            ? await _context.Instituciones.FindAsync(user.InstitucionId.Value)
            : null;
        institucionName = institucion?.Nombre;
        */

        return new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            InstitucionId = user.InstitucionId,
            InstitucionName = institucionName,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            ForcePasswordChange = user.ForcePasswordChange,
            Roles = roles,
        };
    }
}
