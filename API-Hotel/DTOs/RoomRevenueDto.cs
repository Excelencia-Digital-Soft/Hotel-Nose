namespace hotel.DTOs
{
    public class RoomRevenueDto
    {
        public int HabitacionID { get; set; }
        public string NombreHabitacion { get; set; } = null!;
        public string NombreCategoria { get; set; } = null!;
        public decimal TotalIngresos { get; set; }
        public decimal IngresosReservas { get; set; }
        public decimal IngresosConsumos { get; set; }
    }
}