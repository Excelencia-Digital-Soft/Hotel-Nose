using hotel.DTOs.Common;
using hotel.DTOs.Inventory;

namespace hotel.Interfaces;

/// <summary>
/// Interface for inventory reporting and statistics operations
/// Follows Single Responsibility Principle
/// </summary>
public interface IInventoryReportingService
{
    /// <summary>
    /// Gets inventory summary grouped by location
    /// </summary>
    Task<ApiResponse<IEnumerable<InventorySummaryDto>>> GetInventorySummaryAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets comprehensive inventory statistics
    /// </summary>
    Task<ApiResponse<InventoryStatisticsDto>> GetInventoryStatisticsAsync(
        int institucionId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets items with low stock based on threshold
    /// </summary>
    Task<ApiResponse<IEnumerable<InventoryDto>>> GetLowStockItemsAsync(
        int institucionId,
        int threshold = 5,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets combined inventory for a room (room + general inventory)
    /// </summary>
    Task<ApiResponse<IEnumerable<InventoryDto>>> GetCombinedInventoryAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default
    );
}