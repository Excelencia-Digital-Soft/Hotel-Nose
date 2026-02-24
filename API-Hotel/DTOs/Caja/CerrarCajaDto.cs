using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for closing the current cash register shift and setting up the next one
/// </summary>
public class CerrarCajaDto
{
    /// <summary>
    /// Initial amount for the NEXT cash register shift
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "El monto inicial debe ser mayor o igual a 0")]
    public decimal MontoInicial { get; set; }

    /// <summary>
    /// Optional observations about the cash register closure
    /// </summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observacion { get; set; }
}
