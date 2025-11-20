using hotel.DTOs.Common;
using hotel.DTOs.Inventory;

namespace hotel.Interfaces;

/// <summary>
/// Service interface for unified inventory management
/// </summary>
public interface IInventoryUnifiedService
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
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Synchronize general inventory with articles catalog
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Synchronization result</returns>
    Task<ApiResponse<string>> SynchronizeGeneralInventoryAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

    #endregion

    #region Inventory Movements

    /// <summary>
    /// Register an inventory movement
    /// </summary>
    /// <param name="movementDto">Movement data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="userId">User ID making the movement</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created movement</returns>
    Task<ApiResponse<MovimientoInventarioDto>> RegisterMovementAsync(
        MovimientoInventarioCreateDto movementDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get movement history for a specific inventory item
    /// </summary>
    /// <param name="inventoryId">Inventory ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Movement summary with history</returns>
    Task<ApiResponse<MovimientoInventarioResumenDto>> GetInventoryMovementsAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get movement audit trail with advanced filtering
    /// </summary>
    /// <param name="request">Audit request parameters</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated audit results</returns>
    Task<ApiResponse<MovimientoAuditoriaResponseDto>> GetMovementAuditAsync(
        MovimientoAuditoriaRequestDto request,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get a specific movement by ID
    /// </summary>
    /// <param name="movementId">Movement ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Movement details</returns>
    Task<ApiResponse<MovimientoInventarioDto>> GetMovementByIdAsync(
        int movementId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    #endregion

    #region Inventory Alerts

    /// <summary>
    /// Get active inventory alerts
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active alerts summary</returns>
    Task<ApiResponse<AlertasActivasResumenDto>> GetActiveAlertsAsync(
        AlertaFiltroRequestDto request,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Configure alert thresholds for inventory items
    /// </summary>
    /// <param name="configDto">Alert configuration</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="userId">User ID making the configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created configuration</returns>
    Task<ApiResponse<ConfiguracionAlertaDto>> ConfigureAlertsAsync(
        ConfiguracionAlertaCreateUpdateDto configDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Acknowledge an inventory alert
    /// </summary>
    /// <param name="alertId">Alert ID</param>
    /// <param name="acknowledgmentDto">Acknowledgment data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="userId">User ID acknowledging the alert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated alert</returns>
    Task<ApiResponse<AlertaInventarioDto>> AcknowledgeAlertAsync(
        int alertId,
        AlertaReconocimientoDto acknowledgmentDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get alert configuration for an inventory item
    /// </summary>
    /// <param name="inventoryId">Inventory ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Alert configuration</returns>
    Task<ApiResponse<ConfiguracionAlertaDto>> GetAlertConfigurationAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    #endregion

    #region Enhanced Transfer Operations

    /// <summary>
    /// Create a single transfer request
    /// </summary>
    /// <param name="transferDto">Transfer data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="userId">User ID creating the transfer</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transfer</returns>
    Task<ApiResponse<TransferenciaInventarioDto>> CreateTransferAsync(
        TransferenciaCreateDto transferDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create multiple transfer requests in batch
    /// </summary>
    /// <param name="batchDto">Batch transfer data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="userId">User ID creating the transfers</param>
    /// <param name="ipAddress">IP address of the client</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transfers</returns>
    Task<ApiResponse<IEnumerable<TransferenciaInventarioDto>>> CreateBatchTransfersAsync(
        TransferenciaBatchCreateDto batchDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get transfers with filtering
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of transfers</returns>
    Task<ApiResponse<IEnumerable<TransferenciaInventarioDto>>> GetTransfersAsync(
        TransferenciaInventarioFilterDto request,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Approve a transfer request
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <param name="approvalDto">Approval data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="userId">User ID approving the transfer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated transfer</returns>
    Task<ApiResponse<TransferenciaInventarioDto>> ApproveTransferAsync(
        int transferId,
        TransferenciaAprobacionDto approvalDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get a specific transfer by ID
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer details</returns>
    Task<ApiResponse<TransferenciaInventarioDto>> GetTransferByIdAsync(
        int transferId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Transfer inventory between locations (legacy method)
    /// </summary>
    /// <param name="transferDto">Transfer data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer result</returns>
    Task<ApiResponse<string>> TransferInventoryAsync(
        InventoryTransferDto transferDto,
        int institucionId,
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

    #endregion
}

