namespace hotel.DTOs.Inventory;

/// <summary>
/// Statistics DTO for alerts
/// </summary>
public class AlertaEstadisticasDto
{
    /// <summary>
    /// Total number of alerts
    /// </summary>
    public int TotalAlertas { get; set; }

    /// <summary>
    /// Total active alerts
    /// </summary>
    public int TotalActivas { get; set; }

    /// <summary>
    /// Total acknowledged alerts
    /// </summary>
    public int TotalReconocidas { get; set; }

    /// <summary>
    /// Total critical alerts
    /// </summary>
    public int TotalCriticas { get; set; }

    /// <summary>
    /// Alerts grouped by type
    /// </summary>
    public Dictionary<string, int> AlertasPorTipo { get; set; } = new();

    /// <summary>
    /// Alerts grouped by severity
    /// </summary>
    public Dictionary<string, int> AlertasPorSeveridad { get; set; } = new();

    /// <summary>
    /// Alerts per day (last 30 days)
    /// </summary>
    public Dictionary<DateTime, int> AlertasPorDia { get; set; } = new();

    /// <summary>
    /// Percentage of acknowledged alerts
    /// </summary>
    public double PorcentajeReconocimiento { get; set; }

    /// <summary>
    /// Average resolution time in hours
    /// </summary>
    public double TiempoPromedioResolucion { get; set; }
}