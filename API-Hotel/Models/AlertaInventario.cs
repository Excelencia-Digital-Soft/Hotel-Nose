using hotel.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel.Models;

/// <summary>
/// Inventory alert model for automated stock monitoring
/// </summary>
[Table("AlertasInventario")]
public class AlertaInventario
{
    [Key]
    public int AlertaId { get; set; }

    [Required]
    public int InventarioId { get; set; }

    [Required]
    public int InstitucionID { get; set; }

    #region Alert Details

    /// <summary>
    /// Alert type: StockBajo, StockAlto, StockAgotado, ProximoVencimiento
    /// </summary>
    [Required]
    [StringLength(30)]
    public string TipoAlerta { get; set; } = string.Empty;

    /// <summary>
    /// Alert severity: Baja, Media, Alta, Critica
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Severidad { get; set; } = string.Empty;

    /// <summary>
    /// Alert message
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Current stock quantity that triggered the alert
    /// </summary>
    [Required]
    public int CantidadActual { get; set; }

    /// <summary>
    /// Threshold value that was breached
    /// </summary>
    public int? UmbralConfiguracion { get; set; }

    /// <summary>
    /// Whether the alert is active
    /// </summary>
    [Required]
    public bool EsActiva { get; set; } = true;

    /// <summary>
    /// Whether the alert has been acknowledged by a user
    /// </summary>
    [Required]
    public bool FueReconocida { get; set; } = false;

    #endregion

    #region Audit Fields

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaReconocimiento { get; set; }

    public DateTime? FechaResolucion { get; set; }

    /// <summary>
    /// User who acknowledged the alert
    /// </summary>
    [StringLength(450)]
    public string? UsuarioReconocimiento { get; set; }

    /// <summary>
    /// User who resolved the alert
    /// </summary>
    [StringLength(450)]
    public string? UsuarioResolucion { get; set; }

    /// <summary>
    /// Notes about alert acknowledgment
    /// </summary>
    [StringLength(500)]
    public string? NotasReconocimiento { get; set; }

    /// <summary>
    /// Notes about the alert resolution
    /// </summary>
    [StringLength(500)]
    public string? NotasResolucion { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Related inventory item
    /// </summary>
    [ForeignKey(nameof(InventarioId))]
    public virtual InventarioUnificado? Inventario { get; set; }

    /// <summary>
    /// Related institution
    /// </summary>
    [ForeignKey(nameof(InstitucionID))]
    public virtual Institucion? Institucion { get; set; }

    /// <summary>
    /// User who acknowledged the alert
    /// </summary>
    [ForeignKey(nameof(UsuarioReconocimiento))]
    public virtual ApplicationUser? UsuarioQueReconocio { get; set; }

    /// <summary>
    /// User who resolved the alert
    /// </summary>
    [ForeignKey(nameof(UsuarioResolucion))]
    public virtual ApplicationUser? UsuarioQueResolvio { get; set; }

    #endregion
}

/// <summary>
/// Inventory alert configuration model
/// </summary>
[Table("ConfiguracionAlertasInventario")]
public class ConfiguracionAlertaInventario
{
    [Key]
    public int ConfiguracionId { get; set; }

    [Required]
    public int InventarioId { get; set; }

    [Required]
    public int InstitucionID { get; set; }

    // Stock thresholds
    public int? StockMinimo { get; set; }
    public int? StockMaximo { get; set; }
    public int? StockCritico { get; set; }

    // Alert settings
    [Required]
    public bool AlertasStockBajoActivas { get; set; } = true;

    [Required]
    public bool AlertasStockAltoActivas { get; set; } = false;

    [Required]
    public bool AlertasStockCriticoActivas { get; set; } = true;

    // Notification settings
    [Required]
    public bool NotificacionEmailActiva { get; set; } = true;

    [Required]
    public bool NotificacionSmsActiva { get; set; } = false;

    [MaxLength(1000)]
    public string? EmailsNotificacion { get; set; }

    [MaxLength(500)]
    public string? TelefonosNotificacion { get; set; }

    [Required]
    public int FrecuenciaRevisionMinutos { get; set; } = 60;

    [Required]
    public bool EsActiva { get; set; } = true;

    // Audit fields
    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    [Required]
    [MaxLength(450)]
    public string UsuarioCreacion { get; set; } = string.Empty;

    [MaxLength(450)]
    public string? UsuarioActualizacion { get; set; }

    // Navigation properties
    [ForeignKey(nameof(InventarioId))]
    public virtual InventarioUnificado? Inventario { get; set; }

    [ForeignKey(nameof(InstitucionID))]
    public virtual Institucion? Institucion { get; set; }

    [ForeignKey(nameof(UsuarioCreacion))]
    public virtual ApplicationUser? CreadoPor { get; set; }

    [ForeignKey(nameof(UsuarioActualizacion))]
    public virtual ApplicationUser? ModificadoPor { get; set; }
}

/// <summary>
/// Inventory alert types
/// </summary>
public static class TipoAlertaInventario
{
    public const string StockBajo = "StockBajo";
    public const string StockAlto = "StockAlto";
    public const string StockAgotado = "StockAgotado";
    public const string StockCritico = "StockCritico";
    public const string ProximoVencimiento = "ProximoVencimiento";
    public const string ArticuloInactivo = "ArticuloInactivo";
    public const string DiscrepanciaInventario = "DiscrepanciaInventario";
}

/// <summary>
/// Alert severity levels
/// </summary>
public static class SeveridadAlerta
{
    public const string Baja = "Baja";
    public const string Media = "Media";
    public const string Alta = "Alta";
    public const string Critica = "Critica";
}