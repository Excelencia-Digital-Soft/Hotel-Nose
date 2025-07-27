namespace hotel.DTOs.Inventory;

/// <summary>
/// Filter DTO for movement statistics
/// </summary>
public class MovimientoEstadisticasFilterDto
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
    /// Filter by movement type
    /// </summary>
    public string? TipoMovimiento { get; set; }

    /// <summary>
    /// Filter by inventory ID
    /// </summary>
    public int? InventarioId { get; set; }

    /// <summary>
    /// Group by period (day, week, month)
    /// </summary>
    public string? AgrupacionPeriodo { get; set; } = "day";
}