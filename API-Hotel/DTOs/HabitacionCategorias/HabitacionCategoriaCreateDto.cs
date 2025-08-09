using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.HabitacionCategorias
{
    /// <summary>
    /// DTO for creating a new HabitacionCategoria
    /// </summary>
    public class HabitacionCategoriaCreateDto
    {
        /// <summary>
        /// Category name
        /// </summary>
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string NombreCategoria { get; set; } = string.Empty;

        /// <summary>
        /// Category description
        /// </summary>
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Normal price for this category
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio normal debe ser mayor que 0")]
        public decimal? PrecioNormal { get; set; }

        /// <summary>
        /// Special price for this category
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio especial debe ser mayor que 0")]
        public decimal? PrecioEspecial { get; set; }
    }
}