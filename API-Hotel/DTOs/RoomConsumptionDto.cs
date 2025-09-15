namespace hotel.DTOs
{
    public class RoomConsumptionDto
    {
        public int HabitacionID { get; set; }
        public string NombreHabitacion { get; set; } = null!;
        public string NombreCategoria { get; set; } = null!;
        public decimal TotalConsumos { get; set; }
        public List<ConsumptionDetailDto> Detalles { get; set; } = [];
    }

    public class ConsumptionDetailDto
    {
        public int ArticuloID { get; set; }
        public string NombreArticulo { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal PrecioTotal { get; set; }
    }
}