namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for user information (AspNetUsers)
/// </summary>
public class UsuarioDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? NombreCompleto { get; set; }
}