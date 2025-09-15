using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Inventory;

public class TotalAlertas
{
    public int TotalActivas { get; set; }
    public int TotalCriticas { get; set; }
    public int TotalReconocidas { get; set; }
}

/// <summary>
/// Inventory alert response DTO
/// </summary>
public class AlertaInventarioDto
{
    public int AlertaId { get; set; }
    public int InventarioId { get; set; }
    public int InstitucionId { get; set; }

    // Alert details
    public string TipoAlerta { get; set; } = string.Empty;
    public string Severidad { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public int CantidadActual { get; set; }
    public int? UmbralConfiguracion { get; set; }
    public bool EsActiva { get; set; }
    public bool FueReconocida { get; set; }

    // Dates
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaReconocimiento { get; set; }
    public DateTime? FechaResolucion { get; set; }

    // Users
    public string? UsuarioReconocimiento { get; set; }
    public string? NombreUsuarioReconocimiento { get; set; }
    public string? UsuarioResolucion { get; set; }
    public string? NombreUsuarioResolucion { get; set; }
    public string? NotasReconocimiento { get; set; }
    public string? NotasResolucion { get; set; }

    // Related inventory information
    public int ArticuloId { get; set; }
    public string NombreArticulo { get; set; } = string.Empty;
    public string CodigoArticulo { get; set; } = string.Empty;
    public string? ArticuloDescripcion { get; set; }
    public int TipoUbicacion { get; set; }
    public int? UbicacionId { get; set; }
    public string? UbicacionNombre { get; set; }
}

/// <summary>
/// Alert acknowledgment DTO
/// </summary>
public class AlertaReconocimientoDto
{
    [StringLength(500)]
    public string? Notas { get; set; }

    public bool MarcarComoResuelto { get; set; } = false;

    [StringLength(500)]
    public string? NotasResolucion { get; set; }
}

/// <summary>
/// Alert configuration response DTO
/// </summary>
public class ConfiguracionAlertaDto
{
    public int ConfiguracionId { get; set; }
    public int InventarioId { get; set; }
    public int InstitucionId { get; set; }

    // Thresholds
    public int? StockMinimo { get; set; }
    public int? StockMaximo { get; set; }
    public int? StockCritico { get; set; }

    // Alert toggles
    public bool AlertasStockBajoActivas { get; set; }
    public bool AlertasStockAltoActivas { get; set; }
    public bool AlertasStockCriticoActivas { get; set; }

    // Notification settings
    public bool NotificacionEmailActiva { get; set; }
    public bool NotificacionSmsActiva { get; set; }
    public string? EmailsNotificacion { get; set; }
    public string? TelefonosNotificacion { get; set; }
    public int FrecuenciaRevisionMinutos { get; set; }
    public bool EsActiva { get; set; }

    // Audit information
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string UsuarioCreacion { get; set; } = string.Empty;
    public string? NombreUsuarioCreacion { get; set; }
    public string? UsuarioActualizacion { get; set; }
    public string? NombreUsuarioActualizacion { get; set; }

    // Related inventory information
    public int ArticuloId { get; set; }
    public string ArticuloNombre { get; set; } = string.Empty;
    public int TipoUbicacion { get; set; }
    public int? UbicacionId { get; set; }
    public string? UbicacionNombre { get; set; }
    public int CantidadActual { get; set; }
}

/// <summary>
/// Alert configuration create/update DTO
/// </summary>
public class ConfiguracionAlertaCreateUpdateDto
{
    [Required]
    public int InventarioId { get; set; }

    [Range(0, int.MaxValue)]
    public int? StockMinimo { get; set; }

    [Range(0, int.MaxValue)]
    public int? StockMaximo { get; set; }

    [Range(0, int.MaxValue)]
    public int? StockCritico { get; set; }

    public bool AlertasStockBajoActivas { get; set; } = true;
    public bool AlertasStockAltoActivas { get; set; } = false;
    public bool AlertasStockCriticoActivas { get; set; } = true;

    public bool NotificacionEmailActiva { get; set; } = true;
    public bool NotificacionSmsActiva { get; set; } = false;

    public string? EmailsNotificacion { get; set; }
    public string? TelefonosNotificacion { get; set; }
    public int? FrecuenciaRevisionMinutos { get; set; }
    public bool EsActiva { get; set; } = true;
}

/// <summary>
/// Batch alert configuration DTO
/// </summary>
public class ConfiguracionAlertaBatchDto
{
    [Required]
    [MinLength(1)]
    public List<ConfiguracionAlertaCreateUpdateDto> Configuraciones { get; set; } = new();

    /// <summary>
    /// Apply to all inventory items of the same article type
    /// </summary>
    public bool AplicarATodosLosArticulos { get; set; } = false;

    /// <summary>
    /// Apply to all items in the same location
    /// </summary>
    public bool AplicarATodaLaUbicacion { get; set; } = false;
}

/// <summary>
/// Active alerts summary DTO
/// </summary>
public class AlertasActivasResumenDto
{
    public int TotalAlertas { get; set; }
    public int AlertasCriticas { get; set; }
    public int AlertasAltas { get; set; }
    public int AlertasMedias { get; set; }
    public int AlertasBajas { get; set; }
    public int AlertasNoReconocidas { get; set; }

    // Alerts by type
    public Dictionary<string, int> AlertasPorTipo { get; set; } = new();

    // Alerts by location
    public Dictionary<string, int> AlertasPorUbicacion { get; set; } = new();

    // Recent alerts (last 10)
    public List<AlertaInventarioDto> AlertasRecientes { get; set; } = new();
}

/// <summary>
/// Alert filter request DTO
/// </summary>
public class AlertaFiltroRequestDto
{
    public string? TipoAlerta { get; set; }
    public string? Severidad { get; set; }
    public bool? SoloActivas { get; set; } = true;
    public bool? SoloNoReconocidas { get; set; }
    public int? TipoUbicacion { get; set; }
    public int? UbicacionId { get; set; }
    public int? ArticuloId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamañoPagina { get; set; } = 20;
    public string? OrdenarPor { get; set; } = "FechaCreacion";
    public bool OrdenDescendente { get; set; } = true;
}

