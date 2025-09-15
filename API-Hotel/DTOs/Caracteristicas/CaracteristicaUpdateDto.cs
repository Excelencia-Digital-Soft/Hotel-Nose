using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Caracteristicas
{
    /// <summary>
    /// DTO for updating an existing Caracteristica
    /// </summary>
    public class CaracteristicaUpdateDto
    {
        /// <summary>
        /// Name of the caracteristica (e.g., "King Size Bed")
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the caracteristica
        /// </summary>
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Optional icon file for the caracteristica (only if updating the icon)
        /// </summary>
        public IFormFile? Icono { get; set; }

        /// <summary>
        /// Indicates whether to remove the current icon (when no new icon is provided)
        /// </summary>
        public bool RemoveIcon { get; set; } = false;
    }
}