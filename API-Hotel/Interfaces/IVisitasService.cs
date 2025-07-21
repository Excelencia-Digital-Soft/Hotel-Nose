using hotel.DTOs.Common;
using hotel.DTOs.Visitas;
using hotel.Models;

namespace hotel.Interfaces;

public interface IVisitasService
{
    Task<ApiResponse<IEnumerable<VisitaDto>>> GetAllAsync(
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<VisitaDto>> GetByIdAsync(
        int id,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<Visitas>> CreateVisitaAsync(
        VisitaCreateDto createDto,
        int institucionId,
        string? userId = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<VisitaDto>> UpdateAsync(
        int id,
        VisitaUpdateDto updateDto,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse> AnularVisitaAsync(
        int id,
        int institucionId,
        string? reason = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<IEnumerable<VisitaDto>>> GetActiveVisitasAsync(
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<VisitaDto>> GetByHabitacionAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default);
}