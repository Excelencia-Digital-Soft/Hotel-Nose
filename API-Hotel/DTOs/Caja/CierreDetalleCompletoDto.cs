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
    
    /// <summary>
    /// List of all transaction details including payments, cancellations, and expenses
    /// </summary>
    public List<PagoDetalleCompletoDto> Transacciones { get; set; } = new();
    
    /// <summary>
    /// List of expense details
    /// </summary>
    public List<EgresoDetalleDto> Egresos { get; set; } = new();
}