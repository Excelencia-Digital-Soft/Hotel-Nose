namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for cash register (Cierre) information
/// </summary>
public class CajaDto
{
    public int CierreId { get; set; }
    public int? UsuarioId { get; set; } // Legacy field
    public string? UserId { get; set; } // AspNetUsers ID
    public DateTime? FechaHoraCierre { get; set; }
    public decimal? TotalIngresosEfectivo { get; set; }
    public decimal? TotalIngresosBillVirt { get; set; }
    public decimal? TotalIngresosTarjeta { get; set; }
    public string? Observaciones { get; set; }
    public bool? EstadoCierre { get; set; }
    public decimal? MontoInicialCaja { get; set; }
    public int InstitucionID { get; set; }
}