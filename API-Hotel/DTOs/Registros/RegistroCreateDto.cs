using System.ComponentModel.DataAnnotations;
using hotel.Models;

namespace hotel.DTOs.Registros;

public class RegistroCreateDto
{
    [Required]
    [StringLength(2000, ErrorMessage = "El contenido no puede exceder los 2000 caracteres")]
    public string Contenido { get; set; } = string.Empty;

    [Required]
    public TipoRegistro TipoRegistro { get; set; } = TipoRegistro.INFO;

    [Required]
    public ModuloSistema Modulo { get; set; } = ModuloSistema.SISTEMA;

    public int? ReservaId { get; set; }

    public string? DetallesAdicionales { get; set; }
}