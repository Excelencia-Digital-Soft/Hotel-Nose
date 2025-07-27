namespace hotel.DTOs.Inventory;

/// <summary>
/// Filter DTO for inventory alerts
/// </summary>
public class AlertaInventarioFilterDto
{
    /// <summary>
    /// Show only active alerts
    /// </summary>
    public bool SoloActivas { get; set; } = true;

    /// <summary>
    /// Show only unacknowledged alerts
    /// </summary>
    public bool SoloNoReconocidas { get; set; } = false;

    /// <summary>
    /// Filter by alert type
    /// </summary>
    public string? TipoAlerta { get; set; }

    /// <summary>
    /// Filter by severity
    /// </summary>
    public string? Severidad { get; set; }

    /// <summary>
    /// Filter by inventory ID
    /// </summary>
    public int? InventarioId { get; set; }

    /// <summary>
    /// Filter alerts from this date
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Filter alerts until this date
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Sort by field (fecha, severidad, tipo)
    /// </summary>
    public string? OrdenarPor { get; set; } = "fecha";

    /// <summary>
    /// Sort descending
    /// </summary>
    public bool Descendente { get; set; } = true;

    /// <summary>
    /// Page number
    /// </summary>
    public int Pagina { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int TamanoPagina { get; set; } = 20;
}