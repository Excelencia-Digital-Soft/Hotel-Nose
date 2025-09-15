namespace hotel.Models;

public partial class Institucion
{
    public int InstitucionId { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? Descripcion { get; set; }
    public int? TipoID { get; set; }
    public DateTime? FechaAnulado { get; set; }

    public virtual ICollection<Acompanantes> Acompanantes { get; set; } = new List<Acompanantes>();
    public virtual ICollection<Cierre> Cierre { get; set; } = new List<Cierre>();
    public virtual ICollection<CuadreCierre> CuadreCierre { get; set; } = new List<CuadreCierre>();
    public virtual ICollection<Encargos> Encargos { get; set; } = new List<Encargos>();
    public virtual ICollection<Habitaciones> Habitaciones { get; set; } = new List<Habitaciones>();
    public virtual ICollection<InventarioInicial> InventarioInicial { get; set; } = new List<InventarioInicial>();
    public virtual ICollection<Inventarios> Inventarios { get; set; } = new List<Inventarios>();
    public virtual ICollection<Pagos> Pagos { get; set; } = new List<Pagos>();
    public virtual ICollection<Reservas> Reservas { get; set; } = new List<Reservas>();
    public virtual ICollection<UsuariosInstituciones> UsuariosInstituciones { get; set; } = new List<UsuariosInstituciones>();
    public virtual ICollection<Visitas> Visitas { get; set; } = new List<Visitas>();
}
