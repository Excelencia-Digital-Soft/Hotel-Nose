using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Articulos;

public class ArticuloUpdateDto
{
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string? NombreArticulo { get; set; }
    
    [Range(0.01, 9999999999999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 9,999,999,999,999,999.99")]
    public decimal? Precio { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "La categoría debe ser válida")]
    public int? CategoriaId { get; set; }
}