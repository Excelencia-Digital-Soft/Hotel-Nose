using hotel.DTOs.Caja;
using hotel.Models;
using hotel.Models.Identity;

namespace hotel.Interfaces;

/// <summary>
/// Interface for Caja-related DTO mapping operations
/// </summary>
public interface ICajaDtoMapper
{
    /// <summary>
    /// Maps a Cierre entity to CajaDetalladaDto with all related information
    /// </summary>
    CajaDetalladaDto MapToCajaDetalladaDto(Cierre cierre);

    /// <summary>
    /// Maps a Cierre entity to CajaDto for creation responses
    /// </summary>
    CajaDto MapToCajaDto(Cierre cierre);

    /// <summary>
    /// Maps a Pagos entity to PagoDto
    /// </summary>
    PagoDto MapToPagoDto(Pagos pago);

    /// <summary>
    /// Maps AspNetUsers (ApplicationUser) to UsuarioDto
    /// </summary>
    UsuarioDto MapToUsuarioDto(ApplicationUser user);

    /// <summary>
    /// Maps a collection of Cierre entities to CierreBasicoDto for summary views
    /// </summary>
    List<CierreBasicoDto> MapToCierreBasicoDtos(IEnumerable<Cierre> cierres);

    /// <summary>
    /// Maps Egresos entity to EgresoDetalleDto
    /// </summary>
    EgresoDetalleDto MapToEgresoDetalleDto(Egresos egreso);

    /// <summary>
    /// Maps a collection of Egresos to EgresoDetalleDto list
    /// </summary>
    List<EgresoDetalleDto> MapToEgresoDetalleDtos(IEnumerable<Egresos> egresos);

    /// <summary>
    /// Creates a TransaccionPendienteDto for canceled reservations
    /// </summary>
    TransaccionPendienteDto MapReservaAnuladaToTransaccionPendiente(Reservas reserva);

    /// <summary>
    /// Creates a PagoDetalleCompletoDto for canceled reservations in closure details
    /// </summary>
    PagoDetalleCompletoDto MapReservaAnuladaToPagoDetalleCompleto(Reservas reserva);
}