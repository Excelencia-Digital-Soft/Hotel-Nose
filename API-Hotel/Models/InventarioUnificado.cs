using hotel.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel.Models;

/// <summary>
/// Unified inventory model combining room-specific and general inventory
/// </summary>
[Table("InventarioUnificado")]
public class InventarioUnificado
{
    [Key]
    public int InventarioId { get; set; }

    [Required]
    public int ArticuloId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Cantidad { get; set; } = 0;

    [Required]
    public int InstitucionID { get; set; }

    #region Location Tracking

    /// <summary>
    /// Location type: 0 = General, 1 = Room, 2 = Warehouse
    /// </summary>
    [Required]
    public int TipoUbicacion { get; set; } = 0;

    /// <summary>
    /// Location ID (HabitacionId for rooms, null for general inventory)
    /// </summary>
    public int? UbicacionId { get; set; }

    #endregion

    #region Audit Fields

    [Required]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public DateTime? FechaUltimaActualizacion { get; set; }

    /// <summary>
    /// User who created this entry (ASP.NET Identity UserId)
    /// </summary>
    [StringLength(450)]
    public string? UsuarioRegistro { get; set; }

    /// <summary>
    /// User who last updated this entry
    /// </summary>
    [StringLength(450)]
    public string? UsuarioUltimaActualizacion { get; set; }

    #endregion

    #region Soft Delete

    public bool? Anulado { get; set; } = false;

    public DateTime? FechaAnulacion { get; set; }

    [StringLength(450)]
    public string? UsuarioAnulacion { get; set; }

    [StringLength(200)]
    public string? MotivoAnulacion { get; set; }

    #endregion

    #region Stock Management

    /// <summary>
    /// Minimum stock level for reorder alerts
    /// </summary>
    public int? CantidadMinima { get; set; } = 0;

    /// <summary>
    /// Maximum stock level
    /// </summary>
    public int? CantidadMaxima { get; set; }

    /// <summary>
    /// Reorder point
    /// </summary>
    public int? PuntoReorden { get; set; } = 0;

    [StringLength(500)]
    public string? Notas { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Related article
    /// </summary>
    [ForeignKey(nameof(ArticuloId))]
    public virtual Articulos? Articulo { get; set; }

    /// <summary>
    /// Related institution
    /// </summary>
    [ForeignKey(nameof(InstitucionID))]
    public virtual Institucion? Institucion { get; set; }

    /// <summary>
    /// Related room (if TipoUbicacion = 1)
    /// </summary>
    [ForeignKey(nameof(UbicacionId))]
    public virtual Habitaciones? Habitacion { get; set; }

    /// <summary>
    /// User who created this entry
    /// </summary>
    [ForeignKey(nameof(UsuarioRegistro))]
    public virtual ApplicationUser? CreadoPor { get; set; }

    /// <summary>
    /// User who last updated this entry
    /// </summary>
    [ForeignKey(nameof(UsuarioUltimaActualizacion))]
    public virtual ApplicationUser? ModificadoPor { get; set; }

    #endregion
}

/// <summary>
/// Inventory location types
/// </summary>
public enum TipoUbicacionInventario
{
    /// <summary>
    /// General inventory (institution-wide)
    /// </summary>
    General = 0,

    /// <summary>
    /// Room-specific inventory
    /// </summary>
    Habitacion = 1,

    /// <summary>
    /// Warehouse inventory (future use)
    /// </summary>
    Almacen = 2
}