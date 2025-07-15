using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Consumos;

/// <summary>
/// DTO for consumo (consumption) information
/// </summary>
public class ConsumoDto
{
    public int ConsumoId { get; set; }
    public int ArticuloId { get; set; }
    public string ArticleName { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public bool EsHabitacion { get; set; }
    public DateTime FechaConsumo { get; set; }
    public int VisitaId { get; set; }
    public int HabitacionId { get; set; }
    public bool Activo { get; set; }
}

/// <summary>
/// DTO for creating a new consumo
/// </summary>
public class ConsumoCreateDto
{
    [Required]
    public int ArticuloId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Cantidad { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
    public decimal PrecioUnitario { get; set; }
    
    public bool EsHabitacion { get; set; }
}

/// <summary>
/// DTO for updating consumo quantity
/// </summary>
public class ConsumoUpdateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Cantidad { get; set; }
}

/// <summary>
/// DTO for consumos summary
/// </summary>
public class ConsumoSummaryDto
{
    public int VisitaId { get; set; }
    public int TotalConsumos { get; set; }
    public decimal TotalGeneral { get; set; }
    public decimal TotalHabitacion { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ConsumoDto> Consumos { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// DTO for consumo cancellation
/// </summary>
public class ConsumoCancelDto
{
    public string? Reason { get; set; }
    public DateTime CancelledAt { get; set; }
}