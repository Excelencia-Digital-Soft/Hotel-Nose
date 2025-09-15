using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Categorias;

public class CategoriaCreateDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string NombreCategoria { get; set; } = string.Empty;
}