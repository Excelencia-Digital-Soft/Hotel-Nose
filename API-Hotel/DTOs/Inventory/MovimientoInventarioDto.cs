using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Inventory;

/// <summary>
/// Inventory movement response DTO
/// </summary>
public class MovimientoInventarioDto
{
    public int MovimientoId { get; set; }
    public int InventarioId { get; set; }
    public int InstitucionId { get; set; }

    // Movement details
    public string TipoMovimiento { get; set; } = string.Empty;
    public int CantidadAnterior { get; set; }
    public int CantidadNueva { get; set; }
    public int CantidadCambiada { get; set; }
    public string? Motivo { get; set; }
    public string? NumeroDocumento { get; set; }

    // Transfer details (if applicable)
    public int? TransferenciaId { get; set; }
    public int? TipoUbicacionOrigen { get; set; }
    public int? UbicacionIdOrigen { get; set; }
    public string? UbicacionNombreOrigen { get; set; }
    public int? TipoUbicacionDestino { get; set; }
    public int? UbicacionIdDestino { get; set; }
    public string? UbicacionNombreDestino { get; set; }

    // Audit information
    public DateTime FechaMovimiento { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string? NombreUsuario { get; set; }
    public string? DireccionIP { get; set; }

    // Additional metadata
    public Dictionary<string, string>? Metadata { get; set; }

    // Related item information
    public int ArticuloId { get; set; }
    public string NombreArticulo { get; set; } = string.Empty;
    public string CodigoArticulo { get; set; } = string.Empty;
    public string? ArticuloDescripcion { get; set; }
    public decimal ArticuloPrecio { get; set; }
}

/// <summary>
/// Create inventory movement DTO
/// </summary>
public class MovimientoInventarioCreateDto
{
    [Required]
    public int InventarioId { get; set; }

    [Required]
    [StringLength(20)]
    public string TipoMovimiento { get; set; } = string.Empty;

    [Required]
    public int CantidadAnterior { get; set; }

    [Required]
    public int CantidadNueva { get; set; }

    [Required]
    public int CantidadCambiada { get; set; }

    [StringLength(500)]
    public string? Motivo { get; set; }

    [StringLength(100)]
    public string? NumeroDocumento { get; set; }

    // For transfers
    public int? TransferenciaId { get; set; }
    public int? TipoUbicacionOrigen { get; set; }
    public int? UbicacionIdOrigen { get; set; }
    public int? TipoUbicacionDestino { get; set; }
    public int? UbicacionIdDestino { get; set; }

    // Additional metadata
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Inventory movement summary DTO
/// </summary>
public class MovimientoInventarioResumenDto
{
    public int InventarioId { get; set; }
    public int ArticuloId { get; set; }
    public string ArticuloNombre { get; set; } = string.Empty;
    public int CantidadActual { get; set; }

    // Movement statistics
    public int TotalMovimientos { get; set; }
    public int TotalEntradas { get; set; }
    public int TotalSalidas { get; set; }
    public DateTime? UltimoMovimiento { get; set; }
    public string? TipoUltimoMovimiento { get; set; }

    // Recent movements (last 5)
    public List<MovimientoInventarioDto> MovimientosRecientes { get; set; } = new();
}

/// <summary>
/// Movement audit request DTO
/// </summary>
public class MovimientoAuditoriaRequestDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? TipoMovimiento { get; set; }
    public int? ArticuloId { get; set; }
    public int? TipoUbicacion { get; set; }
    public int? UbicacionId { get; set; }
    public string? UsuarioId { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamañoPagina { get; set; } = 50;
}

/// <summary>
/// Movement statistics DTO
/// </summary>
public class MovimientoEstadisticasDto
{
    public int TotalMovimientos { get; set; }
    public int TotalEntradas { get; set; }
    public int TotalSalidas { get; set; }
    public int TotalTransferencias { get; set; }
    public int TotalAjustes { get; set; }
    public DateTime? UltimoMovimiento { get; set; }
    public string? TipoUltimoMovimiento { get; set; }

    // Period statistics
    public int MovimientosEsteRango { get; set; }
    public int EntradasEsteRango { get; set; }
    public int SalidasEsteRango { get; set; }
    public decimal PromedioMovimientosPorDia { get; set; }

    // Value statistics
    public decimal ValorTotalMovimientos { get; set; }
    public decimal ValorTotalEntradas { get; set; }
    public decimal ValorTotalSalidas { get; set; }

    // By movement type
    public Dictionary<string, int> MovimientosPorTipo { get; set; } = new();
    public Dictionary<string, int> MovimientosPorDia { get; set; } = new();
    public Dictionary<string, decimal> ValorPorTipo { get; set; } = new();
    public Dictionary<string, int> UsuariosMasActivos { get; set; } = new();

    public PeriodoAnalisisDto PeriodoAnalisis { get; set; } = new();

    // By user
    public Dictionary<string, int> MovimientosPorUsuario { get; set; } = new();
}

/// <summary>
/// Period analysis DTO
/// </summary>
public class PeriodoAnalisisDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    /// <summary>
    /// Total days in the period
    /// </summary>
    public int DiasEnPeriodo => (FechaFin - FechaInicio).Days + 1;

    /// <summary>
    /// Period description
    /// </summary>
    public string Descripcion => $"{FechaInicio:yyyy-MM-dd} a {FechaFin:yyyy-MM-dd}";
}

/// <summary>
/// Movement audit response DTO
/// </summary>
public class MovimientoAuditoriaResponseDto
{
    public List<MovimientoInventarioDto> Movimientos { get; set; } = new();
    public int TotalRegistros { get; set; }
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public bool TienePaginaAnterior { get; set; }
    public bool TienePaginaSiguiente { get; set; }

    // Summary statistics
    public MovimientoEstadisticasDto Estadisticas { get; set; } = new();
}
