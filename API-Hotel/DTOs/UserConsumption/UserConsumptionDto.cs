namespace hotel.DTOs.UserConsumption;

public class UserConsumptionDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public int ArticuloId { get; set; }
    public string ArticuloNombre { get; set; } = string.Empty;
    public string ArticuloCodigo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaConsumo { get; set; }
    public int? HabitacionId { get; set; }
    public string? HabitacionNumero { get; set; }
    public int? ReservaId { get; set; }
    public string? TipoConsumo { get; set; } // "Habitacion", "Servicio", "Bar", etc.
    public string? Observaciones { get; set; }
    public bool Anulado { get; set; }
}