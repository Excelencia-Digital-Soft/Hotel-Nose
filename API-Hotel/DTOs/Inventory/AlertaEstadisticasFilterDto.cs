namespace hotel.DTOs.Inventory;

/// <summary>
/// Filter DTO for alert statistics
/// </summary>
public class AlertaEstadisticasFilterDto
{
    /// <summary>
    /// Start date for statistics
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// End date for statistics
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Filter by alert type
    /// </summary>
    public string? TipoAlerta { get; set; }

    /// <summary>
    /// Filter by severity
    /// </summary>
    public string? Severidad { get; set; }

    /// <summary>
    /// Include resolved alerts
    /// </summary>
    public bool IncluirResueltas { get; set; } = true;
}