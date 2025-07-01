namespace hotel.DTOs
{
    public class RoomConsumptionDto
    {
        public int HabitacionID { get; set; }
        public string NombreHabitacion { get; set; }
        public string NombreCategoria { get; set; }
        public decimal TotalConsumos { get; set; }
        public List<ConsumptionDetailDto> Detalles { get; set; }
    }

    public class ConsumptionDetailDto
    {
        public int ArticuloID { get; set; }
        public string NombreArticulo { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioTotal { get; set; }
    }
}