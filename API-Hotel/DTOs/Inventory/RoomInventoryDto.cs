using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Inventory;

/// <summary>
/// Room inventory response DTO
/// </summary>
public class RoomInventoryDto
{
    public int InventoryId { get; set; }
    public int ArticuloId { get; set; }
    public string NombreArticulo { get; set; } = string.Empty;
    public string CodigoArticulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public int Cantidad { get; set; }
    public int RoomId { get; set; }
    public string NombreHabitacion { get; set; } = string.Empty;
    public DateTime? FechaActualizacion { get; set; }
}

/// <summary>
/// Room inventory creation DTO
/// </summary>
public class RoomInventoryCreateDto
{
    [Required]
    public int ArticuloId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int CantidadInicial { get; set; }
}

/// <summary>
/// Room inventory update DTO
/// </summary>
public class RoomInventoryUpdateDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public int NuevaCantidad { get; set; }

    [StringLength(500)]
    public string? Motivo { get; set; }
}

/// <summary>
/// Replenish stock DTO
/// </summary>
public class ReplenishStockDto
{
    [Required]
    public int InventoryId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }

    [StringLength(500)]
    public string? Motivo { get; set; }

    [StringLength(100)]
    public string? NumeroDocumento { get; set; }
}

