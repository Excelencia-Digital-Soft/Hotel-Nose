using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Password { get; set; } = string.Empty;
}