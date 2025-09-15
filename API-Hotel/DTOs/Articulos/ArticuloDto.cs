namespace hotel.DTOs.Articulos;

public class ArticuloDto
{
    public int ArticuloId { get; set; }
    public string NombreArticulo { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int? CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public int? ImagenId { get; set; }
    public string? ImagenUrl { get; set; }
    public string? ImagenAPI { get; set; }
    public bool Anulado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? CreadoPorId { get; set; }
    public string? CreadoPorNombre { get; set; }
    public string? ModificadoPorId { get; set; }
    public string? ModificadoPorNombre { get; set; }
}

