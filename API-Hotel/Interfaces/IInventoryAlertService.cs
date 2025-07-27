using hotel.DTOs.Common;
using hotel.DTOs.Inventory;

namespace hotel.Interfaces;

/// <summary>
/// Service interface for inventory alert operations and configuration
/// </summary>
public interface IInventoryAlertService
{
    /// <summary>
    /// Gets active alerts for an institution with filtering and pagination
    /// </summary>
    Task<ApiResponse<PagedResult<AlertaInventarioDto>>> GetActiveAlertsAsync(
        int institucionId,
        AlertaInventarioFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures alert settings for an inventory item
    /// </summary>
    Task<ApiResponse<ConfiguracionAlertaDto>> ConfigureAlertsAsync(
        ConfiguracionAlertaCreateUpdateDto configDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges an alert and optionally marks it as resolved
    /// </summary>
    Task<ApiResponse<AlertaInventarioDto>> AcknowledgeAlertAsync(
        int alertId,
        AlertaReconocimientoDto acknowledgmentDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets alert configuration for a specific inventory item
    /// </summary>
    Task<ApiResponse<ConfiguracionAlertaDto>> GetAlertConfigurationAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks inventory levels and generates alerts based on configuration
    /// </summary>
    Task<ApiResponse<AlertaGenerationResultDto>> CheckAndGenerateAlertsAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets alert statistics for reporting
    /// </summary>
    Task<ApiResponse<AlertaEstadisticasDto>> GetAlertStatisticsAsync(
        int institucionId,
        AlertaEstadisticasFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates (resolves) multiple alerts by their IDs
    /// </summary>
    Task<ApiResponse<int>> ResolveMultipleAlertsAsync(
        List<int> alertIds,
        string resolutionNotes,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default);
}