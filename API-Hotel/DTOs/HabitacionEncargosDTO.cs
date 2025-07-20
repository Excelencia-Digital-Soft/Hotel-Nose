namespace hotel.DTOs
{
    public class HabitacionEncargosDTO
    {
        public int HabitacionId { get; set; }
        public string NombreHabitacion { get; set; } = string.Empty;
        public List<EncargoDTO> Encargos { get; set; } = [];
    }
}
