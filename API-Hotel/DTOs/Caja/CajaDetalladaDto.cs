namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for detailed cash register closure information including payments and user details
/// </summary>
public class CajaDetalladaDto
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
    
    /// <summary>
    /// User information (legacy Usuarios table)
    /// </summary>
    public UsuarioDto? Usuario { get; set; }
    
    /// <summary>
    /// Associated payments for this closure
    /// </summary>
    public IEnumerable<PagoDto> Pagos { get; set; } = new List<PagoDto>();
}