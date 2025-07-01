namespace hotel.DTOs.Identity;

public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public DateTime? TokenExpiration { get; set; }
    public UserDto? User { get; set; }
}