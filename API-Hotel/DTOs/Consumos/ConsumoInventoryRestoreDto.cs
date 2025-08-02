namespace hotel.DTOs.Consumos;

public class ConsumoInventoryRestoreDto
{
    public int ConsumoId { get; set; }
    public int? ArticuloId { get; set; }
    public int? Cantidad { get; set; }
    public bool? EsHabitacion { get; set; }
}