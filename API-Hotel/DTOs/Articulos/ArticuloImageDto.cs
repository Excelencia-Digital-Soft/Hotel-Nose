namespace hotel.DTOs.Articulos;

public class ArticuloImageDto
{
    public int ArticuloId { get; set; }
    public int? ImagenId { get; set; }
    public string? ImagenUrl { get; set; }
    public string? NombreArchivo { get; set; }
    public DateTime? FechaSubida { get; set; }
    public string ContentType { get; set; } = string.Empty;
}