using hotel.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel.Models;

/// <summary>
/// Inventory movement tracking model for audit trail and traceability
/// </summary>
[Table("MovimientosInventario")]
public class MovimientoInventario
{
    [Key]
    public int MovimientoId { get; set; }

    [Required]
    public int InventarioId { get; set; }

    [Required]
    public int InstitucionID { get; set; }

    #region Movement Details

    /// <summary>
    /// Movement type: Entrada, Salida, Transferencia, Ajuste, Consumo
    /// </summary>
    [Required]
    [StringLength(20)]
    public string TipoMovimiento { get; set; } = string.Empty;

    /// <summary>
    /// Quantity before the movement
    /// </summary>
    [Required]
    public int CantidadAnterior { get; set; }

    /// <summary>
    /// Quantity after the movement
    /// </summary>
    [Required]
    public int CantidadNueva { get; set; }

    /// <summary>
    /// Quantity changed (positive or negative)
    /// </summary>
    [Required]
    public int CantidadCambiada { get; set; }

    /// <summary>
    /// Reason or description for the movement
    /// </summary>
    [StringLength(500)]
    public string? Motivo { get; set; }

    /// <summary>
    /// Reference document number (e.g., invoice, transfer order)
    /// </summary>
    [StringLength(100)]
    public string? NumeroDocumento { get; set; }

    #endregion

    #region Transfer Details (if applicable)

    /// <summary>
    /// Related transfer ID (if this movement is part of a transfer)
    /// </summary>
    public int? TransferenciaId { get; set; }

    /// <summary>
    /// Source location type for transfers
    /// </summary>
    public int? TipoUbicacionOrigen { get; set; }

    /// <summary>
    /// Source location ID for transfers
    /// </summary>
    public int? UbicacionIdOrigen { get; set; }

    /// <summary>
    /// Destination location type for transfers
    /// </summary>
    public int? TipoUbicacionDestino { get; set; }

    /// <summary>
    /// Destination location ID for transfers
    /// </summary>
    public int? UbicacionIdDestino { get; set; }

    #endregion

    #region Audit Fields

    [Required]
    public DateTime FechaMovimiento { get; set; } = DateTime.Now;

    /// <summary>
    /// User who made the movement (ASP.NET Identity UserId)
    /// </summary>
    [Required]
    [StringLength(450)]
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>
    /// IP address of the client making the movement
    /// </summary>
    [StringLength(45)]
    public string? DireccionIP { get; set; }

    /// <summary>
    /// Additional metadata (JSON format)
    /// </summary>
    [Column(TypeName = "text")]
    public string? Metadata { get; set; }

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
    /// User who made the movement
    /// </summary>
    [ForeignKey(nameof(UsuarioId))]
    public virtual ApplicationUser? Usuario { get; set; }

    /// <summary>
    /// Related transfer (if this movement is part of a transfer)
    /// </summary>
    [ForeignKey(nameof(TransferenciaId))]
    public virtual TransferenciaInventario? Transferencia { get; set; }

    #endregion
}

/// <summary>
/// Inventory movement types
/// </summary>
public static class TipoMovimientoInventario
{
    public const string Entrada = "Entrada";
    public const string Salida = "Salida";
    public const string Transferencia = "Transferencia";
    public const string Ajuste = "Ajuste";
    public const string Consumo = "Consumo";
    public const string Devolucion = "Devolucion";
    public const string Perdida = "Perdida";
    public const string Sincronizacion = "Sincronizacion";
}