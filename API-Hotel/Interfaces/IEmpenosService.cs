using hotel.DTOs.Common;
using hotel.DTOs.Empenos;

namespace hotel.Interfaces
{
    /// <summary>
    /// Service interface for managing Empeños (Pawn/Collateral) operations
    /// </summary>
    public interface IEmpenosService
    {
        /// <summary>
        /// Gets all active (unpaid) empeños for a specific institution
        /// </summary>
        /// <param name="institucionId">Institution ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of unpaid empeños</returns>
        Task<ApiResponse<IEnumerable<EmpenoDto>>> GetAllUnpaidByInstitutionAsync(
            int institucionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all empeños (paid and unpaid) for a specific institution
        /// </summary>
        /// <param name="institucionId">Institution ID</param>
        /// <param name="includeAnulados">Include cancelled empeños</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of empeños</returns>
        Task<ApiResponse<IEnumerable<EmpenoDto>>> GetAllByInstitutionAsync(
            int institucionId,
            bool includeAnulados = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific empeño by ID
        /// </summary>
        /// <param name="id">Empeño ID</param>
        /// <param name="institucionId">Institution ID for security</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Empeño details</returns>
        Task<ApiResponse<EmpenoDto>> GetByIdAsync(
            int id,
            int institucionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets empeños by visit ID
        /// </summary>
        /// <param name="visitaId">Visit ID</param>
        /// <param name="institucionId">Institution ID for security</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of empeños for the visit</returns>
        Task<ApiResponse<IEnumerable<EmpenoDto>>> GetByVisitaIdAsync(
            int visitaId,
            int institucionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new empeño
        /// </summary>
        /// <param name="createDto">Empeño creation data</param>
        /// <param name="institucionId">Institution ID</param>
        /// <param name="userId">User ID who creates the empeño</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created empeño</returns>
        Task<ApiResponse<EmpenoDto>> CreateAsync(
            EmpenoCreateDto createDto,
            int institucionId,
            string? userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing empeño (only if not paid)
        /// </summary>
        /// <param name="id">Empeño ID</param>
        /// <param name="updateDto">Update data</param>
        /// <param name="institucionId">Institution ID for security</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated empeño</returns>
        Task<ApiResponse<EmpenoDto>> UpdateAsync(
            int id,
            EmpenoUpdateDto updateDto,
            int institucionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes payment for an empeño (fixed logic - no duplicate payments)
        /// </summary>
        /// <param name="id">Empeño ID</param>
        /// <param name="pagoDto">Payment details</param>
        /// <param name="institucionId">Institution ID for security</param>
        /// <param name="userId">User ID who processes the payment</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment processing result</returns>
        Task<ApiResponse<EmpenoDto>> PayEmpenoAsync(
            int id,
            EmpernoPagoDto pagoDto,
            int institucionId,
            string? userId, // Added userId parameter
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft deletes (cancels) an empeño
        /// </summary>
        /// <param name="id">Empeño ID</param>
        /// <param name="institucionId">Institution ID for security</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation result</returns>
        Task<ApiResponse> DeleteAsync(
            int id,
            int institucionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates if a visit exists and belongs to the institution
        /// </summary>
        /// <param name="visitaId">Visit ID</param>
        /// <param name="institucionId">Institution ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        Task<ApiResponse<bool>> ValidateVisitaExistsAsync(
            int visitaId,
            int institucionId,
            CancellationToken cancellationToken = default);
    }
}