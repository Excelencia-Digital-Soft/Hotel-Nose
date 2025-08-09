using hotel.DTOs.Common;
using hotel.DTOs.Caracteristicas;

namespace hotel.Interfaces
{
    public interface ICaracteristicasService
    {
        /// <summary>
        /// Gets all caracteristicas
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of caracteristicas</returns>
        Task<ApiResponse<IEnumerable<CaracteristicaDto>>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a caracteristica by ID
        /// </summary>
        /// <param name="id">Caracteristica ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Caracteristica details</returns>
        Task<ApiResponse<CaracteristicaDto>> GetByIdAsync(
            int id, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new caracteristica
        /// </summary>
        /// <param name="createDto">Caracteristica creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created caracteristica</returns>
        Task<ApiResponse<CaracteristicaDto>> CreateAsync(
            CaracteristicaCreateDto createDto, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing caracteristica
        /// </summary>
        /// <param name="id">Caracteristica ID</param>
        /// <param name="updateDto">Caracteristica update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated caracteristica</returns>
        Task<ApiResponse<CaracteristicaDto>> UpdateAsync(
            int id, 
            CaracteristicaUpdateDto updateDto, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a caracteristica
        /// </summary>
        /// <param name="id">Caracteristica ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation result</returns>
        Task<ApiResponse> DeleteAsync(
            int id, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Assigns caracteristicas to a room
        /// </summary>
        /// <param name="habitacionId">Room ID</param>
        /// <param name="caracteristicaIds">List of caracteristica IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation result</returns>
        Task<ApiResponse> AssignToRoomAsync(
            int habitacionId,
            IEnumerable<int> caracteristicaIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the image for a caracteristica
        /// </summary>
        /// <param name="id">Caracteristica ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Image file information</returns>
        Task<ApiResponse<FileResult>> GetImageAsync(
            int id, 
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// File result for image downloads
    /// </summary>
    public class FileResult
    {
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}