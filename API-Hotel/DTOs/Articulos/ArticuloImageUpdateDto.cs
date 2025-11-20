using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Articulos;

/// <summary>
/// DTO para actualizar la imagen de un artículo existente
/// </summary>
public class ArticuloImageUpdateDto
{
    [Required(ErrorMessage = "La imagen es requerida")]
    public IFormFile Imagen { get; set; } = null!;
}