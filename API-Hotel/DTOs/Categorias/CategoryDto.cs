namespace hotel.DTOs.Categorias;

public class CategoriaDto
{
    public int CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public bool Anulado { get; set; }
    public int? ImagenId { get; set; }
    public string? ImagenUrl { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? CreadoPorId { get; set; }
    public string? CreadoPorNombre { get; set; }
    public string? ModificadoPorId { get; set; }
    public string? ModificadoPorNombre { get; set; }
}