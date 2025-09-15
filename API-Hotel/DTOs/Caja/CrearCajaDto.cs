using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for creating a new cash register entry
/// </summary>
public class CrearCajaDto
{
    /// <summary>
    /// Initial amount in the cash register
    /// </summary>
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "El monto inicial debe ser mayor o igual a 0")]
    public decimal MontoInicial { get; set; }

    /// <summary>
    /// Optional observations about the cash register opening
    /// </summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observacion { get; set; }
}