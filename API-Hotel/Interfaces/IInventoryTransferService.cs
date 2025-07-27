using hotel.DTOs.Common;
using hotel.DTOs.Inventory;

namespace hotel.Interfaces;

/// <summary>
/// Service interface for inventory transfer operations with approval workflow
/// </summary>
public interface IInventoryTransferService
{
    /// <summary>
    /// Creates a new transfer request
    /// </summary>
    Task<ApiResponse<TransferenciaInventarioDto>> CreateTransferAsync(
        TransferenciaCreateDto transferDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple transfers in batch
    /// </summary>
    Task<ApiResponse<List<TransferenciaInventarioDto>>> CreateBatchTransfersAsync(
        TransferenciaBatchCreateDto batchDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated list of transfers with filtering
    /// </summary>
    Task<ApiResponse<PagedResult<TransferenciaInventarioDto>>> GetTransfersAsync(
        int institucionId,
        TransferenciaInventarioFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves or rejects a transfer request
    /// </summary>
    Task<ApiResponse<TransferenciaInventarioDto>> ApproveTransferAsync(
        int transferId,
        TransferenciaApprovalDto approvalDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed information about a specific transfer
    /// </summary>
    Task<ApiResponse<TransferenciaInventarioDto>> GetTransferByIdAsync(
        int transferId,
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes (completes) an approved transfer
    /// </summary>
    Task<ApiResponse<TransferenciaInventarioDto>> ExecuteTransferAsync(
        int transferId,
        TransferenciaExecutionDto executionDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a pending transfer
    /// </summary>
    Task<ApiResponse<TransferenciaInventarioDto>> CancelTransferAsync(
        int transferId,
        string cancellationReason,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending transfers that require approval
    /// </summary>
    Task<ApiResponse<PagedResult<TransferenciaInventarioDto>>> GetPendingApprovalsAsync(
        int institucionId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transfer statistics for reporting
    /// </summary>
    Task<ApiResponse<TransferenciaEstadisticasDto>> GetTransferStatisticsAsync(
        int institucionId,
        TransferenciaEstadisticasFilterDto filter,
        CancellationToken cancellationToken = default);
}