namespace hotel.DTOs
{
    public class HabitacionDTO
    {
        public int HabitacionId { get; set; }
        public string NombreHabitacion { get; set; } = string.Empty;
        public int InstitucionID { get; set; }
        public int? CategoriaId { get; set; }
        public List<string> Imagenes { get; set; } = [];
        public List<int> Caracteristicas { get; set; } = [];
    }
}
