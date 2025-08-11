namespace hotel.DTOs.HabitacionCategorias
{
    /// <summary>
    /// DTO for HabitacionCategoria entity
    /// </summary>
    public class HabitacionCategoriaDto
    {
        /// <summary>
        /// Category unique identifier
        /// </summary>
        public int CategoriaId { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        public string NombreCategoria { get; set; } = string.Empty;

        /// <summary>
        /// Capacidad Máxima
        /// </summary>
        public int? CapacidadMaxima { get; set; }

        /// <summary>
        /// Normal price for this category
        /// </summary>
        public decimal? PrecioNormal { get; set; }

        /// <summary>
        /// Special price for this category
        /// </summary>
        public decimal? PrecioEspecial { get; set; }

        /// <summary>
        /// Institution ID this category belongs to
        /// </summary>
        public int InstitucionId { get; set; }

        /// <summary>
        /// Indicates if the category is active (not deleted)
        /// </summary>
        public bool Activo { get; set; } = true;

        public int PorcentajeXPersona { get; set; }

        /// <summary>
        /// Category creation date
        /// </summary>
        public DateTime? FechaCreacion { get; set; }

        /// <summary>
        /// Category last modification date
        /// </summary>
        public DateTime? FechaModificacion { get; set; }
    }
}

