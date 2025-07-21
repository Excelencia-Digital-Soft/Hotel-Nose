using hotel.DTOs.Common;
using hotel.DTOs.Inventory;

namespace hotel.Interfaces;

/// <summary>
/// Service interface for unified inventory management
/// </summary>
public interface IInventoryService
{
    #region General Inventory Management
    
    /// <summary>
    /// Get all inventory items for an institution
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="locationType">Filter by location type (optional)</param>
    /// <param name="locationId">Filter by specific location ID (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of inventory items</returns>
    Task<ApiResponse<IEnumerable<InventoryDto>>> GetInventoryAsync(
        int institucionId,
        InventoryLocationType? locationType = null,
        int? locationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory item by ID
    /// </summary>
    /// <param name="inventoryId">Inventory ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Inventory item details</returns>
    Task<ApiResponse<InventoryDto>> GetInventoryByIdAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory items by article
    /// </summary>
    /// <param name="articuloId">Article ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of inventory items for the article</returns>
    Task<ApiResponse<IEnumerable<InventoryDto>>> GetInventoryByArticleAsync(
        int articuloId,
        int institucionId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Room Inventory Management

    /// <summary>
    /// Get all inventory items for a specific room
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of room inventory items</returns>
    Task<ApiResponse<IEnumerable<InventoryDto>>> GetRoomInventoryAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add inventory item to a room
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="createDto">Inventory creation data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created inventory item</returns>
    Task<ApiResponse<InventoryDto>> AddRoomInventoryAsync(
        int habitacionId,
        InventoryCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default);

    #endregion

    #region General Institution Inventory

    /// <summary>
    /// Get general inventory for institution (non-room specific)
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of general inventory items</returns>
    Task<ApiResponse<IEnumerable<InventoryDto>>> GetGeneralInventoryAsync(
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add item to general inventory
    /// </summary>
    /// <param name="createDto">Inventory creation data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created inventory item</returns>
    Task<ApiResponse<InventoryDto>> AddGeneralInventoryAsync(
        InventoryCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronize general inventory with articles catalog
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Synchronization result</returns>
    Task<ApiResponse<string>> SynchronizeGeneralInventoryAsync(
        int institucionId,
        CancellationToken cancellationToken = default);

    #endregion

    #region CRUD Operations

    /// <summary>
    /// Create a new inventory item
    /// </summary>
    /// <param name="createDto">Inventory creation data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created inventory item</returns>
    Task<ApiResponse<InventoryDto>> CreateInventoryAsync(
        InventoryCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update inventory quantity
    /// </summary>
    /// <param name="inventoryId">Inventory ID</param>
    /// <param name="updateDto">Inventory update data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated inventory item</returns>
    Task<ApiResponse<InventoryDto>> UpdateInventoryAsync(
        int inventoryId,
        InventoryUpdateDto updateDto,
        int institucionId,
        CancellationToken cancellationToken = default);

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
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete inventory item (soft delete)
    /// </summary>
    /// <param name="inventoryId">Inventory ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    Task<ApiResponse> DeleteInventoryAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Transfer Operations

    /// <summary>
    /// Transfer inventory between locations
    /// </summary>
    /// <param name="transferDto">Transfer data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer result</returns>
    Task<ApiResponse<string>> TransferInventoryAsync(
        InventoryTransferDto transferDto,
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory movement history
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="articuloId">Filter by article ID (optional)</param>
    /// <param name="locationId">Filter by location ID (optional)</param>
    /// <param name="fromDate">Filter from date (optional)</param>
    /// <param name="toDate">Filter to date (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of inventory movements</returns>
    Task<ApiResponse<IEnumerable<InventoryMovementDto>>> GetInventoryMovementsAsync(
        int institucionId,
        int? articuloId = null,
        int? locationId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Reporting and Analysis

    /// <summary>
    /// Get inventory summary by location
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Inventory summary by location</returns>
    Task<ApiResponse<IEnumerable<InventorySummaryDto>>> GetInventorySummaryAsync(
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get combined inventory view (general + room inventories)
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Combined inventory view</returns>
    Task<ApiResponse<IEnumerable<InventoryDto>>> GetCombinedInventoryAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Stock Validation

    /// <summary>
    /// Validate stock availability
    /// </summary>
    /// <param name="articuloId">Article ID</param>
    /// <param name="requestedQuantity">Requested quantity</param>
    /// <param name="locationType">Location type</param>
    /// <param name="locationId">Location ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stock validation result</returns>
    Task<ApiResponse<StockValidationDto>> ValidateStockAsync(
        int articuloId,
        int requestedQuantity,
        InventoryLocationType locationType,
        int? locationId,
        int institucionId,
        CancellationToken cancellationToken = default);

    #endregion
}