using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for filtering cash closures with payments
/// </summary>
public class CierresConPagosFiltroDto
{
    /// <summary>
    /// Filter by closure date range - start date
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Filter by closure date range - end date
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Filter by closure status (true = closed, false = open, null = all)
    /// </summary>
    public bool? EstadoCierre { get; set; }

    /// <summary>
    /// Page number for pagination
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Include only closures with payments
    /// </summary>
    public bool SoloConPagos { get; set; } = false;

    /// <summary>
    /// Include payment details
    /// </summary>
    public bool IncluirDetallePagos { get; set; } = true;
}