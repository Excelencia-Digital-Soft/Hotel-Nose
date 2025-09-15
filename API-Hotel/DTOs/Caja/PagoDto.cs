namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for payment information
/// </summary>
public class PagoDto
{
    public int PagoId { get; set; }
    public decimal? MontoEfectivo { get; set; }
    public decimal? MontoBillVirt { get; set; }
    public decimal? MontoTarjeta { get; set; }
    public decimal? Adicional { get; set; }
    public decimal? MontoDescuento { get; set; }
    public int? MedioPagoId { get; set; }
    public int? CierreId { get; set; }
    public int? TarjetaId { get; set; }
    public DateTime? FechaHora { get; set; }
    public string? Observacion { get; set; }
    public int InstitucionID { get; set; }
}