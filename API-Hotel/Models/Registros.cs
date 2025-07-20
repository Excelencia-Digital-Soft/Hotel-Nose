using System.ComponentModel.DataAnnotations;
using hotel.Models.Identity;

namespace hotel.Models;

public enum TipoRegistro
{
    INFO = 1,
    WARNING = 2,
    ERROR = 3,
    AUDIT = 4,
    DEBUG = 5,
    SECURITY = 6,
}

public enum ModuloSistema
{
    RESERVAS = 1,
    PAGOS = 2,
    HABITACIONES = 3,
    USUARIOS = 4,
    CONSUMOS = 5,
    PROMOCIONES = 6,
    CONFIGURACION = 7,
    INVENTARIO = 8,
    REPORTES = 9,
    SISTEMA = 10,
}

public partial class Registros
{
    [Key]
    public int RegistroID { get; set; }

    [Required]
    [StringLength(2000)]
    public string Contenido { get; set; } = string.Empty;

    [Required]
    public TipoRegistro TipoRegistro { get; set; } = TipoRegistro.INFO;

    [Required]
    public ModuloSistema Modulo { get; set; } = ModuloSistema.SISTEMA;

    public int? ReservaId { get; set; } // Nullable en caso de registros sin reserva

    public string? UsuarioId { get; set; } // Usuario de AspNetUsers

    [Required]
    public int InstitucionID { get; set; } // Para multi-tenancy

    [Required]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public string? DetallesAdicionales { get; set; } // JSON con detalles adicionales

    public string? DireccionIP { get; set; } // IP del usuario que generó el registro

    public bool? Anulado { get; set; } // Para soft delete

    // Propiedades de navegación
    public Reservas? Reserva { get; set; }
    public ApplicationUser? Usuario { get; set; }
    public Institucion? Institucion { get; set; }
}
