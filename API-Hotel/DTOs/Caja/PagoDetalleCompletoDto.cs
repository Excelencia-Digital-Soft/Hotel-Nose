namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for payment with complete details including room and consumption information
/// </summary>
public class PagoDetalleCompletoDto
{
    public int PagoId { get; set; }
    public DateTime? Fecha { get; set; }
    public string? CategoriaNombre { get; set; }
    public decimal Periodo { get; set; }
    public string? TarjetaNombre { get; set; }
    public DateTime? HoraIngreso { get; set; }
    public DateTime? HoraSalida { get; set; }
    public decimal TotalConsumo { get; set; }
    public decimal MontoAdicional { get; set; }
    public decimal? MontoEfectivo { get; set; }
    public decimal? MontoTarjeta { get; set; }
    public decimal? MontoBillVirt { get; set; }
    public decimal? MontoDescuento { get; set; }
    public decimal? InteresTarjeta { get; set; }
    public string? Observacion { get; set; }
    public string? TipoHabitacion { get; set; }
    public string? TipoTransaccion { get; set; } // "Habitación", "Empeño", "Anulación", "Egreso"
}