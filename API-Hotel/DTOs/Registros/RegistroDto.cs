using hotel.Models;

namespace hotel.DTOs.Registros;

public class RegistroDto
{
    public int RegistroID { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public TipoRegistro TipoRegistro { get; set; }
    public string TipoRegistroTexto { get; set; } = string.Empty;
    public ModuloSistema Modulo { get; set; }
    public string ModuloTexto { get; set; } = string.Empty;
    public int? ReservaId { get; set; }
    public string? UsuarioId { get; set; }
    public string? NombreUsuario { get; set; }
    public int InstitucionID { get; set; }
    public string? NombreInstitucion { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string? DetallesAdicionales { get; set; }
    public string? DireccionIP { get; set; }
    public bool? Anulado { get; set; }
}