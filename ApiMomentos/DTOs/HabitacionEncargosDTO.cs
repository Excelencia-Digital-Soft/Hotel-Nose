namespace ApiObjetos.DTOs
{
    public class HabitacionEncargosDTO
    {
        public int HabitacionId { get; set; }
        public string NombreHabitacion { get; set; }
        public List<EncargoDTO> Encargos { get; set; }
    }
}
