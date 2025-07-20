using hotel.Models;

namespace hotel.DTOs.Registros;

public class RegistroFiltroDto
{
    public TipoRegistro? TipoRegistro { get; set; }
    public ModuloSistema? Modulo { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? UsuarioId { get; set; }
    public int? ReservaId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}