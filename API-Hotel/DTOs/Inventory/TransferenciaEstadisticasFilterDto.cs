namespace hotel.DTOs.Inventory;

/// <summary>
/// Filter DTO for transfer statistics
/// </summary>
public class TransferenciaEstadisticasFilterDto
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
    /// Filter by state
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// Filter by priority
    /// </summary>
    public string? Prioridad { get; set; }

    /// <summary>
    /// Include cancelled transfers
    /// </summary>
    public bool IncluirCanceladas { get; set; } = false;
}