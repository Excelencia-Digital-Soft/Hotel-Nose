using hotel.Models.Identity;

namespace hotel.Models
{
    public class UsuariosInstituciones
    {
        public int UsuarioId { get; set; }
        public int InstitucionID { get; set; }
        
        // Para compatibilidad con AspNetUsers (Identity)
        public string? UserId { get; set; }

        public virtual Usuarios Usuario { get; set; } = null!;
        public virtual Institucion Institucion { get; set; } = null!;
        
        // Navigation property para ApplicationUser (Identity)
        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
