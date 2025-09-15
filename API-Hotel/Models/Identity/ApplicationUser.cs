using Microsoft.AspNetCore.Identity;

namespace hotel.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? InstitucionId { get; set; }
    public int? LegacyUserId { get; set; } // ID del usuario en la tabla legacy Usuarios
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ForcePasswordChange { get; set; } = false;
    
    // Navigation properties
    public Institucion? Institucion { get; set; }
    public ICollection<UsuariosInstituciones> UsuariosInstituciones { get; set; } = new List<UsuariosInstituciones>();
}