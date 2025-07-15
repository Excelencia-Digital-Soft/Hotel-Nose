using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Promociones;

/// <summary>
/// DTO for promocion (promotion) information
/// </summary>
public class PromocionDto
{
    public int PromocionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Tarifa { get; set; }
    public int CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; }
    public int InstitucionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new promotion
/// </summary>
public class PromocionCreateDto
{
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Descripcion { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be greater than 0")]
    public decimal Tarifa { get; set; }
    
    [Required]
    public int CategoriaId { get; set; }
    
    [Required]
    [Range(1, 24, ErrorMessage = "Hours must be between 1 and 24")]
    public int CantidadHoras { get; set; } = 1;
    
    [Required]
    public DateTime FechaInicio { get; set; }
    
    [Required]
    public DateTime FechaFin { get; set; }
    
    public bool Activo { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing promotion
/// </summary>
public class PromocionUpdateDto
{
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string? Nombre { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Descripcion { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be greater than 0")]
    public decimal? Tarifa { get; set; }
    
    public int? CategoriaId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool? Activo { get; set; }
}

/// <summary>
/// DTO for promotion validation result
/// </summary>
public class PromocionValidationDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal? OriginalRate { get; set; }
    public decimal? PromotionRate { get; set; }
    public decimal? Discount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
}

/// <summary>
/// DTO for promotion summary
/// </summary>
public class PromocionSummaryDto
{
    public int PromocionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Tarifa { get; set; }
    public bool IsActive { get; set; }
    public int DaysRemaining { get; set; }
    public int UsageCount { get; set; }
}