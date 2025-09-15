namespace hotel.DTOs
{
    public class RoomRankingDto
    {
        public int HabitacionID { get; set; }
        public string NombreHabitacion { get; set; } = null!;
        public string NombreCategoria { get; set; } = null!;
        public int TotalReservas { get; set; }
    }
}