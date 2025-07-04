using System.Security.Claims;
using hotel.Auth;
using hotel.Data;
using hotel.DTOs.Identity;
using hotel.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers;

/// <summary>
/// DEPRECATED: Use Controllers/V1/AuthenticationController en su lugar
/// </summary>
[Obsolete("Este controlador está deprecated. Use V1/AuthenticationController en su lugar.", true)]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly JwtService _jwtService;
    private readonly HotelDbContext _context;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        JwtService jwtService,
        HotelDbContext context
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                new AuthResponseDto { Success = false, Message = "Invalid model state" }
            );
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return BadRequest(
                new AuthResponseDto
                {
                    Success = false,
                    Message = "User with this email already exists",
                }
            );
        }

        // Create new user with fake email if username provided
        string email = registerDto.Email;
        string userName = registerDto.UserName ?? registerDto.Email;

        // If email doesn't contain @, treat it as username and create fake email
        if (!email.Contains("@"))
        {
            userName = email;
            email = $"{userName}@hotel.fake";
        }

        var user = new ApplicationUser
        {
            Email = email,
            UserName = userName,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = registerDto.PhoneNumber,
            InstitucionId = registerDto.InstitucionId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ForcePasswordChange = false
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(
                new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                }
            );
        }

        // Assign default role (you can modify this logic)
        await _userManager.AddToRoleAsync(user, "User");

        // Generate token
        var token = await _jwtService.GenerateTokenAsync(user);
        var tokenExpiration = DateTime.UtcNow.AddHours(24);

        var userDto = await MapToUserDto(user);

        return Ok(
            new AuthResponseDto
            {
                Success = true,
                Message = "User registered successfully",
                Token = token,
                TokenExpiration = tokenExpiration,
                User = userDto,
            }
        );
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                new AuthResponseDto { Success = false, Message = "Invalid model state" }
            );
        }

        ApplicationUser? user = null;

        // Detect if input contains @ symbol (email) or not (username)
        if (loginDto.Email.Contains("@"))
        {
            // Login with email
            user = await _userManager.FindByEmailAsync(loginDto.Email);
        }
        else
        {
            // Login with username - first try to find by UserName
            user = await _userManager.FindByNameAsync(loginDto.Email);

            // If not found by username, try to find by fake email format
            if (user == null)
            {
                string fakeEmail = $"{loginDto.Email}@hotel.fake";
                user = await _userManager.FindByEmailAsync(fakeEmail);
            }
        }

        if (user == null || !user.IsActive)
        {
            return Unauthorized(
                new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid username/email or password",
                }
            );
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(
                new AuthResponseDto { Success = false, Message = "Invalid email or password" }
            );
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate token
        var token = await _jwtService.GenerateTokenAsync(user);
        var tokenExpiration = DateTime.UtcNow.AddHours(24);

        var userDto = await MapToUserDto(user);

        return Ok(
            new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                TokenExpiration = tokenExpiration,
                User = userDto,
            }
        );
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var userDto = await MapToUserDto(user);
        return Ok(userDto);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> ChangePassword(
        ChangePasswordDto changePasswordDto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                new AuthResponseDto { Success = false, Message = "Invalid model state" }
            );
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new AuthResponseDto { Success = false, Message = "User not found" });
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            changePasswordDto.CurrentPassword,
            changePasswordDto.NewPassword
        );
        if (!result.Succeeded)
        {
            return BadRequest(
                new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                }
            );
        }

        return Ok(
            new AuthResponseDto { Success = true, Message = "Password changed successfully" }
        );
    }

    [HttpPost("force-password-change")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> ForcePasswordChange(
        ForcePasswordChangeDto forcePasswordChangeDto
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                new AuthResponseDto { Success = false, Message = "Invalid model state" }
            );
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new AuthResponseDto { Success = false, Message = "User not found" });
        }

        // For forced password change, we don't require the current password
        // Generate a random token for password reset
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(
            user,
            token,
            forcePasswordChangeDto.NewPassword
        );

        if (!result.Succeeded)
        {
            return BadRequest(
                new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                }
            );
        }

        // Mark the user as no longer requiring password change
        user.ForcePasswordChange = false;
        await _userManager.UpdateAsync(user);

        // Generate new token with updated claims
        var newToken = await _jwtService.GenerateTokenAsync(user);
        var tokenExpiration = DateTime.UtcNow.AddHours(24);

        var userDto = await MapToUserDto(user);

        return Ok(
            new AuthResponseDto
            {
                Success = true,
                Message = "Password changed successfully",
                Token = newToken,
                TokenExpiration = tokenExpiration,
                User = userDto,
            }
        );
    }

    private async Task<UserDto> MapToUserDto(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var institucion = user.InstitucionId.HasValue
            ? await _context.Instituciones.FindAsync(user.InstitucionId.Value)
            : null;

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            InstitucionId = user.InstitucionId,
            InstitucionName = institucion?.Nombre,
            LegacyUserId = user.LegacyUserId,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            ForcePasswordChange = user.ForcePasswordChange,
            Roles = roles,
        };
    }
}

