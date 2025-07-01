using hotel.DTOs;
using hotel.DTOs.Common;

namespace hotel.Interfaces;

/// <summary>
/// Service interface for managing room (Habitaciones) operations
/// </summary>
public interface IHabitacionesService
{
    /// <summary>
    /// Get all rooms for a specific institution
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="includeInactive">Include inactive rooms</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of rooms</returns>
    Task<ApiResponse<IEnumerable<HabitacionDto>>> GetHabitacionesAsync(
        int institucionId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get a specific room by ID
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="institucionId">Institution ID for security validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Room details</returns>
    Task<ApiResponse<HabitacionDto>> GetHabitacionByIdAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get available rooms for a specific institution
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available rooms</returns>
    Task<ApiResponse<IEnumerable<HabitacionDto>>> GetAvailableHabitacionesAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get occupied rooms for a specific institution
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of occupied rooms</returns>
    Task<ApiResponse<IEnumerable<HabitacionDto>>> GetOccupiedHabitacionesAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create a new room
    /// </summary>
    /// <param name="createDto">Room creation data</param>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created room</returns>
    Task<ApiResponse<HabitacionDto>> CreateHabitacionAsync(
        HabitacionCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Update an existing room
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="updateDto">Room update data</param>
    /// <param name="institucionId">Institution ID for security validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated room</returns>
    Task<ApiResponse<HabitacionDto>> UpdateHabitacionAsync(
        int habitacionId,
        HabitacionUpdateDto updateDto,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Delete a room (soft delete)
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="institucionId">Institution ID for security validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Delete result</returns>
    Task<ApiResponse> DeleteHabitacionAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Change room availability status
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="available">New availability status</param>
    /// <param name="institucionId">Institution ID for security validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update result</returns>
    Task<ApiResponse> ChangeAvailabilityAsync(
        int habitacionId,
        bool available,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get room statistics for an institution
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Room statistics</returns>
    Task<ApiResponse<HabitacionStatsDto>> GetHabitacionStatsAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get complete room information including visits, reservations, orders, images and characteristics
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="includeInactive">Include inactive rooms</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete room information</returns>
    Task<ApiResponse<IEnumerable<HabitacionCompleteDto>>> GetHabitacionesCompleteAsync(
        int institucionId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get free rooms with minimal data for optimal performance
    /// Returns ~70% less data compared to complete endpoint
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of free rooms with minimal data</returns>
    Task<ApiResponse<IEnumerable<HabitacionLibreDto>>> GetFreeHabitacionesOptimizedAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get occupied rooms with optimized data structure
    /// Returns ~40% less data compared to complete endpoint
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of occupied rooms with optimized data</returns>
    Task<ApiResponse<IEnumerable<HabitacionOptimizedDto>>> GetOccupiedHabitacionesOptimizedAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get optimized rooms data with conditional loading
    /// Free rooms: minimal data, Occupied rooms: necessary data only
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="filter">Filter parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Optimized rooms data with statistics</returns>
    Task<ApiResponse<HabitacionBulkStatsDto>> GetHabitacionesOptimizedAsync(
        int institucionId,
        HabitacionFilterDto filter,
        CancellationToken cancellationToken = default
    );
}

