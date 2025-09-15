namespace hotel.Models
{
    public class HabitacionImagenes
    {
        public int Id { get; set; }
        public int HabitacionId { get; set; }
        public int ImagenId { get; set; }
        public bool? Anulado { get; set; }

        public virtual Habitaciones Habitacion { get; set; } = null!;
        public virtual Imagenes Imagen { get; set; } = null!;
    }
}
