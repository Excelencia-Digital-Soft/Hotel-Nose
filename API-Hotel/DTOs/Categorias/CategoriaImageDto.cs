namespace hotel.DTOs.Categorias;

public class CategoriaImageDto
{
    public int CategoriaId { get; set; }
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}