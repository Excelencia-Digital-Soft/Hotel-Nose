namespace hotel.DTOs.Inventory;

/// <summary>
/// Statistics DTO for transfers
/// </summary>
public class TransferenciaEstadisticasDto
{
    /// <summary>
    /// Total number of transfers
    /// </summary>
    public int TotalTransferencias { get; set; }

    /// <summary>
    /// Transfers grouped by state
    /// </summary>
    public Dictionary<string, int> TransferenciasPorEstado { get; set; } = new();

    /// <summary>
    /// Transfers grouped by priority
    /// </summary>
    public Dictionary<string, int> TransferenciasPorPrioridad { get; set; } = new();

    /// <summary>
    /// Transfers per day (last 30 days)
    /// </summary>
    public Dictionary<DateTime, int> TransferenciasPorDia { get; set; } = new();

    /// <summary>
    /// Average approval time in hours
    /// </summary>
    public double TiempoPromedioAprobacionHoras { get; set; }

    /// <summary>
    /// Approval rate percentage
    /// </summary>
    public double TasaAprobacionPorcentaje { get; set; }

    /// <summary>
    /// Number of pending transfers
    /// </summary>
    public int TransferenciasPendientes { get; set; }

    /// <summary>
    /// Number of completed transfers
    /// </summary>
    public int TransferenciasCompletadas { get; set; }
}