namespace hotel.DTOs.Movimientos;

public class MovimientoDto
{
    public int MovimientosId { get; set; }
    public int? VisitaId { get; set; }
    public string? VisitaIdentificador { get; set; }
    public int InstitucionID { get; set; }
    public int? PagoId { get; set; }
    public DateTime? FechaPago { get; set; }
    public decimal? TotalFacturado { get; set; }
    public int? HabitacionId { get; set; }
    public string? HabitacionNombre { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public bool? Anulado { get; set; }
    public string? Descripcion { get; set; }
    public int TotalConsumos { get; set; }
    public decimal TotalConsumosAmount { get; set; }
    public string? UserId { get; set; }
}

public class MovimientoCreateDto
{
    public int? VisitaId { get; set; }
    public decimal TotalFacturado { get; set; }
    public int? HabitacionId { get; set; }
    public string? Descripcion { get; set; }
}

public class MovimientoUpdateDto
{
    public decimal? TotalFacturado { get; set; }
    public string? Descripcion { get; set; }
}

