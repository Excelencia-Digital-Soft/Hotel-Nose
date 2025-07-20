namespace hotel.DTOs.Registros;

public class RegistrosPaginadosDto
{
    public IEnumerable<RegistroDto> Registros { get; set; } = new List<RegistroDto>();
    public int TotalRegistros { get; set; }
    public int PaginaActual { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalPaginas { get; set; }
    public bool TienePaginaAnterior { get; set; }
    public bool TienePaginaSiguiente { get; set; }
}