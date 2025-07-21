using hotel.DTOs.Common;
using hotel.DTOs.Promociones;

namespace hotel.Interfaces;

/// <summary>
/// Service interface for managing promociones (promotions)
/// </summary>
public interface IPromocionesService
{
    /// <summary>
    /// Get all promotions for a specific category
    /// </summary>
    /// <param name="categoriaId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of promotions</returns>
    Task<ApiResponse<IEnumerable<PromocionDto>>> GetPromotionsByCategoryAsync(
        int categoriaId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active promotions for an institution
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active promotions</returns>
    Task<ApiResponse<IEnumerable<PromocionDto>>> GetActivePromotionsAsync(
        int institucionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get promotion by ID
    /// </summary>
    /// <param name="promocionId">Promotion ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Promotion details</returns>
    Task<ApiResponse<PromocionDto>> GetPromotionByIdAsync(
        int promocionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new promotion
    /// </summary>
    /// <param name="createDto">Promotion creation data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created promotion</returns>
    Task<ApiResponse<PromocionDto>> CreatePromotionAsync(
        PromocionCreateDto createDto, 
        int institucionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing promotion
    /// </summary>
    /// <param name="promocionId">Promotion ID</param>
    /// <param name="updateDto">Promotion update data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated promotion</returns>
    Task<ApiResponse<PromocionDto>> UpdatePromotionAsync(
        int promocionId, 
        PromocionUpdateDto updateDto, 
        int institucionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a promotion (soft delete)
    /// </summary>
    /// <param name="promocionId">Promotion ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    Task<ApiResponse> DeletePromotionAsync(
        int promocionId, 
        int institucionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if a promotion is applicable to a reservation
    /// </summary>
    /// <param name="promocionId">Promotion ID</param>
    /// <param name="reservaId">Reservation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<ApiResponse<PromocionValidationDto>> ValidatePromotionAsync(
        int promocionId, 
        int reservaId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate and get promotion for reservation creation
    /// </summary>
    /// <param name="promocionId">Promotion ID</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="categoriaId">Room category ID (optional, for compatibility check)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with promotion data</returns>
    Task<ApiResponse<PromocionValidationResult>> ValidateAndGetPromocionAsync(
        int promocionId, 
        int institucionId, 
        int? categoriaId = null,
        CancellationToken cancellationToken = default);
}