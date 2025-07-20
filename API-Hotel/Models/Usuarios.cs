namespace hotel.Models;

[Obsolete("Usar Identity")]
public partial class Usuarios
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = null!;
    public string Contraseña { get; set; } = null!;
    public int RolId { get; set; }

    public virtual ICollection<UsuariosInstituciones> UsuariosInstituciones { get; set; } =
        new List<UsuariosInstituciones>();

    public virtual ICollection<Cierre> Cierre { get; } = new List<Cierre>();
    public virtual ICollection<MovimientosUsuarios> MovimientosUsuarios { get; } =
        new List<MovimientosUsuarios>();
    public virtual Roles Rol { get; set; } = null!;
}

