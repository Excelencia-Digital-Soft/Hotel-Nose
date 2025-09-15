using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Inventory;

/// <summary>
/// Inventory transfer response DTO
/// </summary>
public class TransferenciaInventarioDto
{
    public int TransferenciaId { get; set; }
    public int InstitucionId { get; set; }
    public string NumeroTransferencia { get; set; } = string.Empty;

    // Transfer details
    public int TipoUbicacionOrigen { get; set; }
    public int? UbicacionIdOrigen { get; set; }
    public string? UbicacionNombreOrigen { get; set; }
    public int TipoUbicacionDestino { get; set; }
    public int? UbicacionIdDestino { get; set; }
    public string? UbicacionNombreDestino { get; set; }

    public string Estado { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public string? Notas { get; set; }
    public DateTime? FechaEsperada { get; set; }

    // Approval workflow
    public bool RequiereAprobacion { get; set; }
    public string? UsuarioAprobacion { get; set; }
    public string? UsuarioNombreAprobacion { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? ComentariosAprobacion { get; set; }

    public string? UsuarioRechazo { get; set; }
    public string? UsuarioNombreRechazo { get; set; }
    public DateTime? FechaRechazo { get; set; }
    public string? MotivoRechazo { get; set; }

    // Completion details
    public string? UsuarioCompletado { get; set; }
    public string? UsuarioNombreCompletado { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public string? NotasCompletado { get; set; }

    // Audit information
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string UsuarioCreacion { get; set; } = string.Empty;
    public string? UsuarioNombreCreacion { get; set; }

    // Transfer items
    public List<DetalleTransferenciaDto> Detalles { get; set; } = new();

    // Summary information
    public int TotalArticulos { get; set; }
    public int ArticulosCompletados { get; set; }
    public decimal PorcentajeCompletado { get; set; }
}

/// <summary>
/// Transfer detail item DTO
/// </summary>
public class DetalleTransferenciaDto
{
    public int DetalleId { get; set; }
    public int TransferenciaId { get; set; }
    public int InventarioId { get; set; }
    public int ArticuloId { get; set; }
    public string ArticuloNombre { get; set; } = string.Empty;
    public string? ArticuloDescripcion { get; set; }
    public decimal ArticuloPrecio { get; set; }

    public int CantidadSolicitada { get; set; }
    public int? CantidadTransferida { get; set; }
    public int? CantidadDisponible { get; set; }
    public string? Notas { get; set; }
    public bool? FueTransferido { get; set; }
    public string? MotivoFallo { get; set; }
}

/// <summary>
/// Create transfer request DTO
/// </summary>
public class TransferenciaCreateDto
{
    [Required]
    public int TipoUbicacionOrigen { get; set; }

    public int? UbicacionIdOrigen { get; set; }

    [Required]
    public int TipoUbicacionDestino { get; set; }

    public int? UbicacionIdDestino { get; set; }

    [Required]
    [StringLength(20)]
    public string Prioridad { get; set; } = "Media";

    [StringLength(500)]
    public string? Motivo { get; set; }

    [StringLength(1000)]
    public string? Notas { get; set; }

    public DateTime? FechaEsperada { get; set; }

    public bool RequiereAprobacion { get; set; } = true;

    [Required]
    [MinLength(1)]
    public List<DetalleTransferenciaCreateDto> Detalles { get; set; } = new();
}

/// <summary>
/// Transfer detail create DTO
/// </summary>
public class DetalleTransferenciaCreateDto
{
    [Required]
    public int InventarioId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int CantidadSolicitada { get; set; }

    [StringLength(500)]
    public string? Notas { get; set; }
}

/// <summary>
/// Batch transfer create DTO
/// </summary>
public class TransferenciaBatchCreateDto
{
    [Required]
    [MinLength(1)]
    public List<TransferenciaCreateDto> Transferencias { get; set; } = new();

    /// <summary>
    /// Process all transfers atomically (all or nothing)
    /// </summary>
    public bool ProcesamientoAtomico { get; set; } = false;

    /// <summary>
    /// Default priority for all transfers if not specified
    /// </summary>
    [StringLength(20)]
    public string? PrioridadPorDefecto { get; set; } = "Media";

    /// <summary>
    /// Default approval requirement if not specified
    /// </summary>
    public bool? RequiereAprobacionPorDefecto { get; set; } = true;
}

/// <summary>
/// Transfer approval DTO
/// </summary>
public class TransferenciaAprobacionDto
{
    [StringLength(500)]
    public string? Comentarios { get; set; }

    /// <summary>
    /// Approve specific items only (if not provided, approves all)
    /// </summary>
    public List<int>? DetalleIds { get; set; }
}

/// <summary>
/// Transfer rejection DTO
/// </summary>
public class TransferenciaRechazoDto
{
    [Required]
    [StringLength(500)]
    public string MotivoRechazo { get; set; } = string.Empty;

    /// <summary>
    /// Reject specific items only (if not provided, rejects all)
    /// </summary>
    public List<int>? DetalleIds { get; set; }
}

/// <summary>
/// Transfer completion DTO
/// </summary>
public class TransferenciaCompletadoDto
{
    [Required]
    [MinLength(1)]
    public List<DetalleCompletadoDto> Detalles { get; set; } = new();

    [StringLength(500)]
    public string? NotasCompletado { get; set; }
}

/// <summary>
/// Detail completion DTO
/// </summary>
public class DetalleCompletadoDto
{
    [Required]
    public int DetalleId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int CantidadTransferida { get; set; }

    public bool FueTransferido { get; set; } = true;

    [StringLength(200)]
    public string? MotivoFallo { get; set; }
}

/// <summary>
/// Transfer filter request DTO
/// </summary>
public class TransferenciaFiltroRequestDto
{
    public string? Estado { get; set; }
    public string? Prioridad { get; set; }
    public int? TipoUbicacionOrigen { get; set; }
    public int? UbicacionIdOrigen { get; set; }
    public int? TipoUbicacionDestino { get; set; }
    public int? UbicacionIdDestino { get; set; }
    public string? UsuarioCreacion { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool? SoloPendientes { get; set; }
    public bool? SoloRequierenAprobacion { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamañoPagina { get; set; } = 20;
    public string? OrdenarPor { get; set; } = "FechaCreacion";
    public bool OrdenDescendente { get; set; } = true;
}

/// <summary>
/// Transfer summary DTO
/// </summary>
public class TransferenciaResumenDto
{
    public int TotalTransferencias { get; set; }
    public int TransferenciasPendientes { get; set; }
    public int TransferenciasAprobadas { get; set; }
    public int TransferenciasCompletadas { get; set; }
    public int TransferenciasRechazadas { get; set; }

    // Statistics by priority
    public Dictionary<string, int> TransferenciasPorPrioridad { get; set; } = new();

    // Statistics by location
    public Dictionary<string, int> TransferenciasPorOrigen { get; set; } = new();
    public Dictionary<string, int> TransferenciasPorDestino { get; set; } = new();

    // Recent transfers (last 5)
    public List<TransferenciaInventarioDto> TransferenciasRecientes { get; set; } = new();
}

