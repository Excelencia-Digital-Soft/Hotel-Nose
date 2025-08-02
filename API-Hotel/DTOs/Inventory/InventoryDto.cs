using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Inventory;

/// <summary>
/// DTO for inventory information
/// </summary>
public class InventoryDto
{
    public int InventoryId { get; set; }
    public int ArticuloId { get; set; }
    public string ArticuloNombre { get; set; } = string.Empty;
    public string? ArticuloDescripcion { get; set; }
    public decimal ArticuloPrecio { get; set; }
    public string? ArticuloImagenUrl { get; set; }
    public int Cantidad { get; set; }
    public InventoryLocationType LocationType { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int InstitucionId { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for creating inventory entries
/// </summary>
public class InventoryCreateDto
{
    [Required]
    public int ArticuloId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public int Cantidad { get; set; }

    [Required]
    public InventoryLocationType LocationType { get; set; }

    public int? LocationId { get; set; }

    [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating inventory quantities
/// </summary>
public class InventoryUpdateDto
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public int Cantidad { get; set; }

    [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for batch inventory updates
/// </summary>
public class InventoryBatchUpdateDto
{
    [Required]
    public List<InventoryBatchItem> Items { get; set; } = new();
}

/// <summary>
/// Individual item in batch update
/// </summary>
public class InventoryBatchItem
{
    [Required]
    public int InventoryId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public int Cantidad { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// DTO for inventory transfers between locations
/// </summary>
public class InventoryTransferDto
{
    [Required]
    public int ArticuloId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Cantidad { get; set; }

    [Required]
    public InventoryLocationType FromLocationType { get; set; }

    public int? FromLocationId { get; set; }

    [Required]
    public InventoryLocationType ToLocationType { get; set; }

    public int? ToLocationId { get; set; }

    [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for inventory movement history
/// </summary>
public class InventoryMovementDto
{
    public int MovementId { get; set; }
    public int ArticuloId { get; set; }
    public string ArticuloNombre { get; set; } = string.Empty;
    public InventoryMovementType MovementType { get; set; }
    public int Cantidad { get; set; }
    public int? FromLocationId { get; set; }
    public string? FromLocationName { get; set; }
    public int? ToLocationId { get; set; }
    public string? ToLocationName { get; set; }
    public string? Notes { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public int InstitucionId { get; set; }
}

/// <summary>
/// DTO for inventory summary by location
/// </summary>
public class InventorySummaryDto
{
    public InventoryLocationType LocationType { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int TotalItems { get; set; }
    public int UniqueArticles { get; set; }
    public decimal TotalValue { get; set; }
    public List<InventoryItemSummaryDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for individual inventory item summary
/// </summary>
public class InventoryItemSummaryDto
{
    public int ArticuloId { get; set; }
    public string ArticuloNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

/// <summary>
/// Types of inventory locations
/// </summary>
public enum InventoryLocationType
{
    General = 0, // Institution-wide general inventory
    Room = 1, // Room-specific inventory
    Warehouse = 2, // Warehouse inventory (future use)
}

/// <summary>
/// Types of inventory movements
/// </summary>
public enum InventoryMovementType
{
    Addition = 0, // Adding inventory
    Removal = 1, // Removing inventory
    Transfer = 2, // Transfer between locations
    Consumption = 3, // Consumption/usage
    Adjustment = 4, // Manual adjustment
}

/// <summary>
/// DTO for stock validation
/// </summary>
public class StockValidationDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public int ArticuloId { get; set; }
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public InventoryLocationType LocationType { get; set; }
    public int? LocationId { get; set; }
}
