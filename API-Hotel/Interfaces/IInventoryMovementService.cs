using hotel.DTOs.Common;
using hotel.DTOs.Inventory;

namespace hotel.Interfaces;

/// <summary>
/// Service interface for inventory movement operations and audit trail
/// </summary>
public interface IInventoryMovementService
{
    /// <summary>
    /// Records a new movement in the inventory audit trail
    /// </summary>
    Task<ApiResponse<MovimientoInventarioDto>> CreateMovementAsync(
        MovimientoInventarioCreateDto movementDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated list of movements for an inventory item
    /// </summary>
    Task<ApiResponse<PagedResult<MovimientoInventarioDto>>> GetMovementsAsync(
        int inventoryId,
        int institucionId,
        MovimientoInventarioFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed information about a specific movement
    /// </summary>
    Task<ApiResponse<MovimientoInventarioDto>> GetMovementByIdAsync(
        int movementId,
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets movement statistics for reporting
    /// </summary>
    Task<ApiResponse<MovimientoEstadisticasDto>> GetMovementStatisticsAsync(
        int institucionId,
        MovimientoEstadisticasFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records inventory adjustment with proper audit trail
    /// </summary>
    Task<ApiResponse<MovimientoInventarioDto>> RecordAdjustmentAsync(
        int inventoryId,
        int newQuantity,
        string reason,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records consumption movement with consumption details
    /// </summary>
    Task<ApiResponse<MovimientoInventarioDto>> RecordConsumptionAsync(
        int inventoryId,
        int quantity,
        int? consumoId,
        string? details,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}