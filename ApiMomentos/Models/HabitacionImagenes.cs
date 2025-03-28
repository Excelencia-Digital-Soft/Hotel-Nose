namespace ApiObjetos.Models
{
    public class HabitacionImagenes
    {
        public int Id { get; set; }
        public int HabitacionId { get; set; }
        public int ImagenId { get; set; }
        public bool Anulado { get; set; } = false; // Add this property for soft delete

        public virtual Habitaciones Habitacion { get; set; }
        public virtual Imagenes Imagen { get; set; }
    }
}