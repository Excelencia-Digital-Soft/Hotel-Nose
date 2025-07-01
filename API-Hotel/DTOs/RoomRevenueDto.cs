namespace hotel.DTOs
{
    public class RoomRevenueDto
    {
        public int HabitacionID { get; set; }
        public string NombreHabitacion { get; set; }
        public string NombreCategoria { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal IngresosReservas { get; set; }
        public decimal IngresosConsumos { get; set; }
    }
}