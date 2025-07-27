using hotel.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel.Models;

/// <summary>
/// Inventory transfer model with approval workflow
/// </summary>
[Table("TransferenciasInventario")]
public class TransferenciaInventario
{
    [Key]
    public int TransferenciaId { get; set; }

    [Required]
    public int InstitucionID { get; set; }

    #region Transfer Details

    /// <summary>
    /// Transfer request number (auto-generated)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string NumeroTransferencia { get; set; } = string.Empty;

    /// <summary>
    /// Source location type
    /// </summary>
    [Required]
    public int TipoUbicacionOrigen { get; set; }

    /// <summary>
    /// Source location ID
    /// </summary>
    public int? UbicacionIdOrigen { get; set; }

    /// <summary>
    /// Destination location type
    /// </summary>
    [Required]
    public int TipoUbicacionDestino { get; set; }

    /// <summary>
    /// Destination location ID
    /// </summary>
    public int? UbicacionIdDestino { get; set; }

    /// <summary>
    /// Transfer status: Pendiente, Aprobada, Rechazada, Completada, Cancelada
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = EstadoTransferencia.Pendiente;

    /// <summary>
    /// Transfer priority: Baja, Media, Alta, Urgente
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Prioridad { get; set; } = PrioridadTransferencia.Media;

    /// <summary>
    /// Reason for the transfer
    /// </summary>
    [StringLength(500)]
    public string? Motivo { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    [StringLength(1000)]
    public string? Notas { get; set; }

    /// <summary>
    /// Expected completion date
    /// </summary>
    public DateTime? FechaEsperada { get; set; }

    #endregion

    #region Approval Workflow

    /// <summary>
    /// Whether this transfer requires approval
    /// </summary>
    [Required]
    public bool RequiereAprobacion { get; set; } = true;

    /// <summary>
    /// User who approved the transfer
    /// </summary>
    [StringLength(450)]
    public string? UsuarioAprobacion { get; set; }

    /// <summary>
    /// Date when transfer was approved
    /// </summary>
    public DateTime? FechaAprobacion { get; set; }

    /// <summary>
    /// Approval comments
    /// </summary>
    [StringLength(500)]
    public string? ComentariosAprobacion { get; set; }

    /// <summary>
    /// User who rejected the transfer (if applicable)
    /// </summary>
    [StringLength(450)]
    public string? UsuarioRechazo { get; set; }

    /// <summary>
    /// Date when transfer was rejected
    /// </summary>
    public DateTime? FechaRechazo { get; set; }

    /// <summary>
    /// Rejection reason
    /// </summary>
    [StringLength(500)]
    public string? MotivoRechazo { get; set; }

    #endregion

    #region Completion Details

    /// <summary>
    /// User who completed the transfer
    /// </summary>
    [StringLength(450)]
    public string? UsuarioCompletado { get; set; }

    /// <summary>
    /// Date when transfer was completed
    /// </summary>
    public DateTime? FechaCompletado { get; set; }

    /// <summary>
    /// Completion notes
    /// </summary>
    [StringLength(500)]
    public string? NotasCompletado { get; set; }

    #endregion

    #region Audit Fields

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    /// <summary>
    /// User who created the transfer request
    /// </summary>
    [Required]
    [StringLength(450)]
    public string UsuarioCreacion { get; set; } = string.Empty;

    /// <summary>
    /// User who last updated the transfer
    /// </summary>
    [StringLength(450)]
    public string? UsuarioActualizacion { get; set; }

    /// <summary>
    /// IP address of the client who created the transfer
    /// </summary>
    [StringLength(45)]
    public string? DireccionIP { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Related institution
    /// </summary>
    [ForeignKey(nameof(InstitucionID))]
    public virtual Institucion? Institucion { get; set; }

    /// <summary>
    /// User who created the transfer
    /// </summary>
    [ForeignKey(nameof(UsuarioCreacion))]
    public virtual ApplicationUser? CreadoPor { get; set; }

    /// <summary>
    /// User who approved the transfer
    /// </summary>
    [ForeignKey(nameof(UsuarioAprobacion))]
    public virtual ApplicationUser? AprobadoPor { get; set; }

    /// <summary>
    /// User who completed the transfer
    /// </summary>
    [ForeignKey(nameof(UsuarioCompletado))]
    public virtual ApplicationUser? CompletadoPor { get; set; }

    /// <summary>
    /// Transfer detail items
    /// </summary>
    public virtual ICollection<DetalleTransferenciaInventario> Detalles { get; set; } = new List<DetalleTransferenciaInventario>();

    /// <summary>
    /// Inventory movements generated by this transfer
    /// </summary>
    public virtual ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();

    #endregion
}

/// <summary>
/// Transfer detail items model
/// </summary>
[Table("DetallesTransferenciaInventario")]
public class DetalleTransferenciaInventario
{
    [Key]
    public int DetalleId { get; set; }

    [Required]
    public int TransferenciaId { get; set; }

    [Required]
    public int InventarioId { get; set; }

    [Required]
    public int ArticuloId { get; set; }

    #region Transfer Item Details

    /// <summary>
    /// Requested quantity to transfer
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int CantidadSolicitada { get; set; }

    /// <summary>
    /// Actually transferred quantity
    /// </summary>
    public int? CantidadTransferida { get; set; }

    /// <summary>
    /// Available quantity at the time of request
    /// </summary>
    public int? CantidadDisponible { get; set; }

    /// <summary>
    /// Item-specific notes
    /// </summary>
    [StringLength(500)]
    public string? Notas { get; set; }

    /// <summary>
    /// Whether this item was successfully transferred
    /// </summary>
    public bool? FueTransferido { get; set; }

    /// <summary>
    /// Reason if transfer failed for this item
    /// </summary>
    [StringLength(200)]
    public string? MotivoFallo { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Parent transfer
    /// </summary>
    [ForeignKey(nameof(TransferenciaId))]
    public virtual TransferenciaInventario? Transferencia { get; set; }

    /// <summary>
    /// Related inventory item
    /// </summary>
    [ForeignKey(nameof(InventarioId))]
    public virtual InventarioUnificado? Inventario { get; set; }

    /// <summary>
    /// Related article
    /// </summary>
    [ForeignKey(nameof(ArticuloId))]
    public virtual Articulos? Articulo { get; set; }

    #endregion
}

/// <summary>
/// Transfer status constants
/// </summary>
public static class EstadoTransferencia
{
    public const string Pendiente = "Pendiente";
    public const string Aprobada = "Aprobada";
    public const string Rechazada = "Rechazada";
    public const string EnProceso = "EnProceso";
    public const string Completada = "Completada";
    public const string Cancelada = "Cancelada";
    public const string ParcialmenteCompletada = "ParcialmenteCompletada";
}

/// <summary>
/// Transfer priority constants
/// </summary>
public static class PrioridadTransferencia
{
    public const string Baja = "Baja";
    public const string Media = "Media";
    public const string Alta = "Alta";
    public const string Urgente = "Urgente";
}