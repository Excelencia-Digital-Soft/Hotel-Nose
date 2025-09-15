using hotel.DTOs.Common;
using hotel.DTOs.Categorias;

namespace hotel.Interfaces;

public interface ICategoriasService
{
    Task<ApiResponse<IEnumerable<CategoriaDto>>> GetAllAsync(
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<CategoriaDto>> GetByIdAsync(
        int id,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<CategoriaDto>> CreateAsync(
        CategoriaCreateDto createDto,
        int institucionId,
        string? creadoPorId = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<CategoriaDto>> CreateWithImageAsync(
        CategoriaCreateWithImageDto createDto,
        int institucionId,
        string? creadoPorId = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<CategoriaDto>> UpdateAsync(
        int id,
        CategoriaUpdateDto updateDto,
        int institucionId,
        string? modificadoPorId = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<CategoriaDto>> UpdateImageAsync(
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
        int categoriaId,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<string>> GetImageContentTypeAsync(
        int categoriaId,
        int institucionId,
        CancellationToken cancellationToken = default);
}