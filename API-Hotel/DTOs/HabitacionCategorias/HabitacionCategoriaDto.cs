using System.ComponentModel.DataAnnotations;

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
        /// Category description
        /// </summary>
        public string? Descripcion { get; set; }

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

        /// <summary>
        /// Number of rooms in this category
        /// </summary>
        public int NumeroHabitaciones { get; set; }

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