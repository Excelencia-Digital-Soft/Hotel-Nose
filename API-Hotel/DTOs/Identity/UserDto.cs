namespace hotel.DTOs.Identity;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public int? InstitucionId { get; set; }
    public string? InstitucionName { get; set; }
    public int? LegacyUserId { get; set; } // ID del usuario en la tabla legacy Usuarios
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public bool ForcePasswordChange { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}