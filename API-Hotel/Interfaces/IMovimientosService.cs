using hotel.DTOs.Common;
using hotel.DTOs.Movimientos;
using hotel.Models;

namespace hotel.Interfaces;

public interface IMovimientosService
{
    Task<ApiResponse<IEnumerable<MovimientoDto>>> GetAllAsync(
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<MovimientoDto>> GetByIdAsync(
        int id,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<Movimientos>> CreateMovimientoAsync(
        MovimientoCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<Movimientos>> CreateMovimientoHabitacionAsync(
        int visitaId,
        int institucionId,
        decimal totalFacturado,
        int habitacionId,
        string? descripcion = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<MovimientoDto>> UpdateAsync(
        int id,
        MovimientoUpdateDto updateDto,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse> AnularMovimientoAsync(
        int id,
        int institucionId,
        string? reason = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<IEnumerable<MovimientoDto>>> GetByVisitaAsync(
        int visitaId,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<decimal>> GetTotalByVisitaAsync(
        int visitaId,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse> UpdatePagoIdAsync(
        int movimientoId,
        int pagoId,
        int institucionId,
        CancellationToken cancellationToken = default);
}