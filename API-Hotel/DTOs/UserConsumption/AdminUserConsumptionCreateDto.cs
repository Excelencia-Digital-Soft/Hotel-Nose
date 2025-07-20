using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.UserConsumption;

public class AdminUserConsumptionCreateDto
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    [StringLength(450, ErrorMessage = "El ID del usuario no puede exceder 450 caracteres")]
    public string UserId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El artículo es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El artículo debe ser válido")]
    public int ArticuloId { get; set; }
    
    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }
    
    [Range(0.01, 9999999999999999.99, ErrorMessage = "El precio unitario debe estar entre 0.01 y 9,999,999,999,999,999.99")]
    public decimal? PrecioUnitario { get; set; }
    
    public int? HabitacionId { get; set; }
    
    public int? ReservaId { get; set; }
    
    [StringLength(50, ErrorMessage = "El tipo de consumo no puede exceder 50 caracteres")]
    public string? TipoConsumo { get; set; }
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}