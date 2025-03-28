namespace ApiObjetos.DTOs
{
    public class HabitacionDTO
    {
        public int HabitacionId { get; set; }
        public string NombreHabitacion { get; set; }
        public int InstitucionID { get; set; }
        public int? CategoriaId { get; set; }
        public List<string> Imagenes { get; set; } // Solo rutas de imágenes
    }

}
