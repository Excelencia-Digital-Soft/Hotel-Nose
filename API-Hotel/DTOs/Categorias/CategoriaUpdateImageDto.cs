using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Categorias;

/// <summary>
/// DTO para actualizar la imagen de una categoría existente
/// </summary>
public class CategoriaUpdateImageDto
{
    [Required(ErrorMessage = "La imagen es requerida")]
    public IFormFile Imagen { get; set; } = null!;
}