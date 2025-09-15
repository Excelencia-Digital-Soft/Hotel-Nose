using hotel.DTOs.Common;
using hotel.DTOs.HabitacionCategorias;

namespace hotel.Interfaces
{
    /// <summary>
    /// Service interface for managing room categories (CategoriasHabitaciones)
    /// </summary>
    public interface IHabitacionCategoriasService
    {
        /// <summary>
        /// Gets all active room categories for a specific institution
        /// </summary>
        /// <param name="institucionId">Institution ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of room categories</returns>
        Task<ApiResponse<IEnumerable<HabitacionCategoriaDto>>> GetAllByInstitutionAsync(
            int institucionId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific room category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="institucionId">Institution ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Room category details</returns>
        Task<ApiResponse<HabitacionCategoriaDto>> GetByIdAsync(
            int id,
            int institucionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new room category
        /// </summary>
        /// <param name="createDto">Category creation data</param>
        /// <param name="institucionId">Institution ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created room category</returns>
        Task<ApiResponse<HabitacionCategoriaDto>> CreateAsync(
            HabitacionCategoriaCreateDto createDto,
            int institucionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing room category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="updateDto">Category update data</param>
        /// <param name="institucionId">Institution ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated room category</returns>
        Task<ApiResponse<HabitacionCategoriaDto>> UpdateAsync(
            int id,
            HabitacionCategoriaUpdateDto updateDto,
            int institucionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft deletes a room category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="institucionId">Institution ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation result</returns>
        Task<ApiResponse> DeleteAsync(
            int id,
            int institucionId,
            CancellationToken cancellationToken = default);
    }
}