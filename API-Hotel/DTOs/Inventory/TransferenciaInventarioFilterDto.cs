namespace hotel.DTOs.Inventory;

/// <summary>
/// Filter DTO for inventory transfers
/// </summary>
public class TransferenciaInventarioFilterDto
{
    /// <summary>
    /// Filter by state
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// Filter by priority
    /// </summary>
    public string? Prioridad { get; set; }

    /// <summary>
    /// Filter by source location type
    /// </summary>
    public int? TipoUbicacionOrigen { get; set; }

    /// <summary>
    /// Filter by destination location type
    /// </summary>
    public int? TipoUbicacionDestino { get; set; }

    /// <summary>
    /// Filter by source location ID
    /// </summary>
    public int? UbicacionOrigenId { get; set; }

    /// <summary>
    /// Filter by destination location ID
    /// </summary>
    public int? UbicacionDestinoId { get; set; }

    /// <summary>
    /// Filter transfers from this date
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Filter transfers until this date
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Filter by creator user ID
    /// </summary>
    public string? UsuarioCreacion { get; set; }

    /// <summary>
    /// Sort by field (fecha, estado, prioridad, numero)
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