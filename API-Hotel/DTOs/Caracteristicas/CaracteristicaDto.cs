namespace hotel.DTOs.Caracteristicas
{
    /// <summary>
    /// DTO for Caracteristica entity
    /// </summary>
    public class CaracteristicaDto
    {
        /// <summary>
        /// Unique identifier for the caracteristica
        /// </summary>
        public int CaracteristicaId { get; set; }

        /// <summary>
        /// Name of the caracteristica (e.g., "King Size Bed")
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the caracteristica
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Optional icon path for the caracteristica
        /// </summary>
        public string? Icono { get; set; }

        /// <summary>
        /// Indicates if the caracteristica has an icon image
        /// </summary>
        public bool HasIcon => !string.IsNullOrEmpty(Icono);

        /// <summary>
        /// URL to access the icon image
        /// </summary>
        public string? IconUrl =>
            HasIcon ? $"/api/v1/caracteristicas/{CaracteristicaId}/image" : null;
    }
}

