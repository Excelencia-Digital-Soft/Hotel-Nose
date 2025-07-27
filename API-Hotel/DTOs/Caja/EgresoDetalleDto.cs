namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for expense details in cash register closure
/// </summary>
public class EgresoDetalleDto
{
    public int EgresoId { get; set; }
    public DateTime? Fecha { get; set; }
    public decimal MontoEfectivo { get; set; }
    public string? Observacion { get; set; }
    public string? TipoEgresoNombre { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
}