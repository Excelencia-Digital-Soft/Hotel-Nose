namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for closures and current pending transactions response
/// </summary>
public class CierresyActualDto
{
    public List<CierreBasicoDto> Cierres { get; set; } = new();
    public List<TransaccionPendienteDto> TransaccionesPendientes { get; set; } = new();
    public List<EgresoDetalleDto> EgresosPendientes { get; set; } = new();
}

/// <summary>
/// DTO for basic closure information in the list
/// </summary>
public class CierreBasicoDto
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
    public List<PagoDto> Pagos { get; set; } = new();
}

/// <summary>
/// DTO for pending transactions (payments, cancellations)
/// </summary>
public class TransaccionPendienteDto
{
    public int PagoId { get; set; }
    public int? HabitacionId { get; set; }
    public string? TarjetaNombre { get; set; }
    public decimal Periodo { get; set; }
    public string? CategoriaNombre { get; set; }
    public DateTime? Fecha { get; set; }
    public DateTime? HoraIngreso { get; set; }
    public DateTime? HoraSalida { get; set; }
    public decimal MontoAdicional { get; set; }
    public decimal TotalConsumo { get; set; }
    public decimal? MontoEfectivo { get; set; }
    public decimal? MontoTarjeta { get; set; }
    public decimal? MontoBillVirt { get; set; }
    public decimal? MontoDescuento { get; set; }
    public string? Observacion { get; set; }
    public string? TipoHabitacion { get; set; }
    public string? TipoTransaccion { get; set; } // "Habitación", "Empeño", "Anulación"
}