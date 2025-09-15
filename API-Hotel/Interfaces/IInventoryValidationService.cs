using hotel.DTOs.Common;
using hotel.DTOs.Inventory;

namespace hotel.Interfaces;

/// <summary>
/// Interface for inventory validation operations
/// Follows Single Responsibility Principle
/// </summary>
public interface IInventoryValidationService
{
    /// <summary>
    /// Validates if requested stock is available
    /// </summary>
    Task<ApiResponse<StockValidationDto>> ValidateStockAsync(
        int articuloId,
        int requestedQuantity,
        InventoryLocationType locationType,
        int? locationId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Validates multiple stock items at once
    /// </summary>
    Task<ApiResponse<IEnumerable<StockValidationDto>>> ValidateMultipleStockAsync(
        IEnumerable<StockValidationRequestDto> requests,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Simple availability check returning true/false
    /// </summary>
    Task<ApiResponse<bool>> ValidateAvailabilityAsync(
        int articuloId,
        int requiredQuantity,
        InventoryLocationType locationType,
        int? locationId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the available quantity for an article in a specific location
    /// </summary>
    Task<ApiResponse<int>> GetAvailableQuantityAsync(
        int articuloId,
        InventoryLocationType locationType,
        int? locationId,
        int institucionId,
        CancellationToken cancellationToken = default
    );
}