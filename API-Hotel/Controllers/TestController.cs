using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hotel.Models.Identity;
using hotel.Data;

namespace hotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HotelDbContext _context;
    private readonly ILogger<TestController> _logger;

    public TestController(UserManager<ApplicationUser> userManager, HotelDbContext context, ILogger<TestController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    [HttpGet("original-passwords")]
    public async Task<IActionResult> GetOriginalPasswords()
    {
        try
        {
            var usuarios = await _context.Usuarios
                .Select(u => new 
                {
                    u.UsuarioId,
                    u.NombreUsuario,
                    u.Contraseña,
                    HashLength = u.Contraseña.Length,
                    Email = u.NombreUsuario.Contains("@") ? u.NombreUsuario : $"{u.NombreUsuario}@hotel.fake"
                })
                .ToListAsync();

            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("set-user-password/{email}")]
    public async Task<IActionResult> SetUserPassword(string email, [FromBody] SetPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"User {email} not found");
            }

            // Generate a new BCrypt hash for the password
            var newHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 11);
            
            // Update the user's password hash directly
            user.PasswordHash = newHash;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    Success = true,
                    Message = $"Password for {email} updated successfully",
                    NewHash = newHash,
                    TestVerification = BCrypt.Net.BCrypt.Verify(request.Password, newHash)
                });
            }
            else
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to update password",
                    Errors = result.Errors.Select(e => e.Description)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting password for user {Email}", email);
            return BadRequest(new
            {
                Success = false,
                Message = "Error setting password",
                Error = ex.Message
            });
        }
    }
}

public class SetPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}