using hotel.DTOs.Common;
using hotel.DTOs.Consumos;

namespace hotel.Interfaces;

/// <summary>
/// Service interface for managing consumos (consumptions)
/// </summary>
public interface IConsumosService
{
    /// <summary>
    /// Get all consumos for a specific visit
    /// </summary>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of consumos</returns>
    Task<ApiResponse<IEnumerable<ConsumoDto>>> GetConsumosByVisitaAsync(
        int visitaId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add general consumos to a visit
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="items">Items to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    Task<ApiResponse> AddGeneralConsumosAsync(
        int habitacionId, 
        int visitaId, 
        IEnumerable<ConsumoCreateDto> items, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add room-specific consumos to a visit
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="items">Items to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    Task<ApiResponse> AddRoomConsumosAsync(
        int habitacionId, 
        int visitaId, 
        IEnumerable<ConsumoCreateDto> items, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel/void a consumo
    /// </summary>
    /// <param name="consumoId">Consumo ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    Task<ApiResponse> CancelConsumoAsync(
        int consumoId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update consumo quantity
    /// </summary>
    /// <param name="consumoId">Consumo ID</param>
    /// <param name="quantity">New quantity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated consumo</returns>
    Task<ApiResponse<ConsumoDto>> UpdateConsumoQuantityAsync(
        int consumoId, 
        int quantity, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get consumos summary for a visit
    /// </summary>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Consumos summary</returns>
    Task<ApiResponse<ConsumoSummaryDto>> GetConsumosSummaryAsync(
        int visitaId, 
        CancellationToken cancellationToken = default);
}