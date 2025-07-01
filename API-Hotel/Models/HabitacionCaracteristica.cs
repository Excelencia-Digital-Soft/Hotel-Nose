namespace hotel.Models
{
    public class HabitacionCaracteristica
    {
        public int HabitacionId { get; set; } // ID de la habitación
        public int CaracteristicaId { get; set; } // ID de la característica

        // Relaciones
        public virtual Habitaciones Habitacion { get; set; }
        public virtual Caracteristica Caracteristica { get; set; }
    }
}