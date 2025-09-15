namespace hotel.DTOs.Inventory;

/// <summary>
/// Filter DTO for inventory movements
/// </summary>
public class MovimientoInventarioFilterDto
{
    /// <summary>
    /// Filter by movement type
    /// </summary>
    public string? TipoMovimiento { get; set; }

    /// <summary>
    /// Filter movements from this date
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Filter movements until this date
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Filter by user ID
    /// </summary>
    public string? UsuarioId { get; set; }

    /// <summary>
    /// Sort by field (fecha, tipo, cantidad)
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