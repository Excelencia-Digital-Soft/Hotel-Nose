namespace hotel.Models;

public partial class Acompanantes
{
    public int AcompananteId { get; set; }
    public string Nombres { get; set; } = null!;
    public string? DocumentoIdentidad { get; set; }
    public string? Telefono { get; set; }
    public string? Parentesco { get; set; }
    public DateTime FechaRegistro { get; set; }
    public int? InstitucionId { get; set; }

    public virtual Institucion? Institucion { get; set; }
}