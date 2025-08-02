using hotel.DTOs.Common;
using hotel.DTOs.Inventory;

namespace hotel.Interfaces;

/// <summary>
/// Interface for core inventory operations (CRUD)
/// Follows Single Responsibility Principle
/// </summary>
public interface IInventoryCoreService
{
    /// <summary>
    /// Retrieves inventory items based on filters
    /// </summary>
    Task<ApiResponse<IEnumerable<InventoryDto>>> GetInventoryAsync(
        int institucionId,
        InventoryLocationType? locationType = null,
        int? locationId = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a specific inventory item by ID
    /// </summary>
    Task<ApiResponse<InventoryDto>> GetInventoryByIdAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates a new inventory item
    /// </summary>
    Task<ApiResponse<InventoryDto>> CreateInventoryAsync(
        InventoryCreateDto createDto,
        int institucionId,
        string? userId = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates the quantity of an inventory item
    /// </summary>
    Task<ApiResponse<InventoryDto>> UpdateInventoryQuantityAsync(
        int inventoryId,
        int newQuantity,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes an inventory item
    /// </summary>
    Task<ApiResponse> DeleteInventoryAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Synchronizes general inventory with articles
    /// </summary>
    Task<ApiResponse<string>> SynchronizeGeneralInventoryAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Batch update multiple inventory items
    /// </summary>
    /// <param name="batchUpdateDto">Batch update data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update result</returns>
    Task<ApiResponse<string>> BatchUpdateInventoryAsync(
        InventoryBatchUpdateDto batchUpdateDto,
        int institucionId,
        CancellationToken cancellationToken = default
    );
}

