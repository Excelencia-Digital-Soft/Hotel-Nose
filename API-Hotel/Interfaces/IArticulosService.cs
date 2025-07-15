using hotel.DTOs.Common;
using hotel.DTOs.Articulos;

namespace hotel.Interfaces;

public interface IArticulosService
{
    Task<ApiResponse<IEnumerable<ArticuloDto>>> GetAllAsync(
        int institucionId,
        int? categoriaId = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<ArticuloDto>> GetByIdAsync(
        int id,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<ArticuloDto>> CreateAsync(
        ArticuloCreateDto createDto,
        int institucionId,
        string? creadoPorId = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<ArticuloDto>> CreateWithImageAsync(
        ArticuloCreateWithImageDto createDto,
        int institucionId,
        string? creadoPorId = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<ArticuloDto>> UpdateAsync(
        int id,
        ArticuloUpdateDto updateDto,
        int institucionId,
        string? modificadoPorId = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<ArticuloDto>> UpdateImageAsync(
        int id,
        IFormFile imagen,
        int institucionId,
        string? modificadoPorId = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse> DeleteAsync(
        int id,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse> ToggleStatusAsync(
        int id,
        bool anulado,
        int institucionId,
        string? modificadoPorId = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<byte[]>> GetImageAsync(
        int articuloId,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<string>> GetImageContentTypeAsync(
        int articuloId,
        int institucionId,
        CancellationToken cancellationToken = default);
}