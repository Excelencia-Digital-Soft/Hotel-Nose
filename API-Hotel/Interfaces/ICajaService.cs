using hotel.DTOs.Caja;
using hotel.DTOs.Common;

namespace hotel.Interfaces;

/// <summary>
/// Service interface for cash register (caja) operations
/// </summary>
public interface ICajaService
{
    /// <summary>
    /// Creates a new cash register entry
    /// </summary>
    Task<ApiResponse<CajaDto>> CrearCajaAsync(
        CrearCajaDto crearCajaDto,
        int institucionId,
        string? userId = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets all cash register closures for an institution
    /// </summary>
    Task<ApiResponse<IEnumerable<CajaDetalladaDto>>> GetCierresAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets cash register closures with detailed payment information
    /// </summary>
    Task<ApiResponse<CierresConPagosPaginadosDto>> GetCierresConPagosAsync(
        int institucionId,
        CierresConPagosFiltroDto filtro,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a specific cash register closure by ID
    /// </summary>
    Task<ApiResponse<CajaDetalladaDto>> GetCierreByIdAsync(
        int cierreId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets complete details of a cash register closure including payments, cancellations, and expenses
    /// </summary>
    Task<ApiResponse<CierreDetalleCompletoDto>> GetCierreDetalleAsync(
        int cierreId,
        int institucionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets all cash register closures and current pending transactions for an institution
    /// </summary>
    Task<ApiResponse<CierresyActualDto>> GetCierresyActualAsync(
        int institucionId,
        string? userId = null,
        CancellationToken cancellationToken = default
    );
}

