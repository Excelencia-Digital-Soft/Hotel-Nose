namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for complete cash register closure details including payments, canceled reservations, and expenses
/// </summary>
public class CierreDetalleCompletoDto
{
    public int CierreId { get; set; }
    public DateTime? FechaHoraCierre { get; set; }
    public bool EstadoCierre { get; set; }
    public decimal? TotalIngresosEfectivo { get; set; }
    public decimal? TotalIngresosBillVirt { get; set; }
    public decimal? TotalIngresosTarjeta { get; set; }
    public decimal? MontoInicialCaja { get; set; }
    public string? Observaciones { get; set; }
    public int InstitucionID { get; set; }
    public int CantidadAnulaciones { get; set; }

    /// <summary>
    /// List of payment transactions
    /// </summary>
    public List<PagoDetalleCompletoDto> Pagos { get; set; } = new();

    /// <summary>
    /// List of canceled reservations
    /// </summary>
    public List<PagoDetalleCompletoDto> Anulaciones { get; set; } = new();

    /// <summary>
    /// List of expense details
    /// </summary>
    public List<EgresoDetalleDto> Egresos { get; set; } = new();
}