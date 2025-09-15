namespace hotel.Models
{
    public class Caracteristica
    {
        public int CaracteristicaId { get; set; } // ID único de la característica
        public string Nombre { get; set; } = string.Empty; // Nombre de la característica (ej: "Cama King Size")
        public string? Descripcion { get; set; } // Descripción opcional de la característica
        public string? Icono { get; set; } // Make Icono nullable

        // Relación muchos a muchos con Habitaciones
        public virtual ICollection<HabitacionCaracteristica> HabitacionCaracteristicas { get; set; } =
            new List<HabitacionCaracteristica>();
    }
}

