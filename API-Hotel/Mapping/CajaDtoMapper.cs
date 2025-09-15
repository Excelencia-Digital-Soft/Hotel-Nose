using hotel.DTOs.Caja;
using hotel.DTOs.Common;
using hotel.Interfaces;
using hotel.Models;
using hotel.Models.Identity;

namespace hotel.Mapping;

/// <summary>
/// Dedicated mapper for Caja-related DTOs to improve maintainability and separation of concerns
/// </summary>
public class CajaDtoMapper : ICajaDtoMapper
{
    /// <summary>
    /// Maps a Cierre entity to CajaDetalladaDto with all related information
    /// </summary>
    public CajaDetalladaDto MapToCajaDetalladaDto(Cierre cierre)
    {
        return new CajaDetalladaDto
        {
            CierreId = cierre.CierreId,
            UsuarioId = cierre.UsuarioId, // Legacy field
            UserId = cierre.UserId, // New AspNetUsers field
            FechaHoraCierre = cierre.FechaHoraCierre,
            TotalIngresosEfectivo = cierre.TotalIngresosEfectivo,
            TotalIngresosBillVirt = cierre.TotalIngresosBillVirt,
            TotalIngresosTarjeta = cierre.TotalIngresosTarjeta,
            Observaciones = cierre.Observaciones,
            EstadoCierre = cierre.EstadoCierre,
            MontoInicialCaja = cierre.MontoInicialCaja,
            InstitucionID = cierre.InstitucionID,
            Usuario = cierre.User != null ? MapToUsuarioDto(cierre.User) : null,
            Pagos = cierre.Pagos.Select(MapToPagoDto).ToList()
        };
    }

    /// <summary>
    /// Maps a Cierre entity to CajaDto for creation responses
    /// </summary>
    public CajaDto MapToCajaDto(Cierre cierre)
    {
        return new CajaDto
        {
            CierreId = cierre.CierreId,
            UsuarioId = cierre.UsuarioId, // Legacy field
            UserId = cierre.UserId, // AspNetUsers field
            FechaHoraCierre = cierre.FechaHoraCierre,
            TotalIngresosEfectivo = cierre.TotalIngresosEfectivo,
            TotalIngresosBillVirt = cierre.TotalIngresosBillVirt,
            TotalIngresosTarjeta = cierre.TotalIngresosTarjeta,
            Observaciones = cierre.Observaciones,
            EstadoCierre = cierre.EstadoCierre,
            MontoInicialCaja = cierre.MontoInicialCaja,
            InstitucionID = cierre.InstitucionID
        };
    }

    /// <summary>
    /// Maps a Pagos entity to PagoDto
    /// </summary>
    public PagoDto MapToPagoDto(Pagos pago)
    {
        return new PagoDto
        {
            PagoId = pago.PagoId,
            MontoEfectivo = pago.MontoEfectivo,
            MontoBillVirt = pago.MontoBillVirt,
            MontoTarjeta = pago.MontoTarjeta,
            Adicional = pago.Adicional,
            MontoDescuento = pago.MontoDescuento,
            MedioPagoId = pago.MedioPagoId,
            CierreId = pago.CierreId,
            TarjetaId = pago.TarjetaId,
            FechaHora = pago.fechaHora,
            Observacion = pago.Observacion,
            InstitucionID = pago.InstitucionID
        };
    }

    /// <summary>
    /// Maps AspNetUsers (ApplicationUser) to UsuarioDto
    /// </summary>
    public UsuarioDto MapToUsuarioDto(ApplicationUser user)
    {
        return new UsuarioDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email,
            NombreCompleto = $"{user.FirstName} {user.LastName}".Trim()
        };
    }

    /// <summary>
    /// Maps a collection of Cierre entities to CierreBasicoDto for summary views
    /// </summary>
    public List<CierreBasicoDto> MapToCierreBasicoDtos(IEnumerable<Cierre> cierres)
    {
        return cierres.Select(cierre => new CierreBasicoDto
        {
            CierreId = cierre.CierreId,
            FechaHoraCierre = cierre.FechaHoraCierre,
            EstadoCierre = cierre.EstadoCierre ?? false,
            TotalIngresosEfectivo = cierre.TotalIngresosEfectivo,
            TotalIngresosBillVirt = cierre.TotalIngresosBillVirt,
            TotalIngresosTarjeta = cierre.TotalIngresosTarjeta,
            MontoInicialCaja = cierre.MontoInicialCaja,
            Observaciones = cierre.Observaciones,
            InstitucionID = cierre.InstitucionID,
            Pagos = cierre.Pagos.Select(MapToPagoDto).ToList()
        }).ToList();
    }

    /// <summary>
    /// Maps Egresos entity to EgresoDetalleDto
    /// </summary>
    public EgresoDetalleDto MapToEgresoDetalleDto(Egresos egreso)
    {
        return new EgresoDetalleDto
        {
            EgresoId = egreso.EgresoId,
            Fecha = egreso.Fecha,
            MontoEfectivo = egreso.Cantidad * egreso.Precio,
            Observacion = $"Pago de egreso por: {egreso.TipoEgreso?.Nombre}",
            TipoEgresoNombre = egreso.TipoEgreso?.Nombre,
            Cantidad = egreso.Cantidad,
            Precio = egreso.Precio
        };
    }

    /// <summary>
    /// Maps a collection of Egresos to EgresoDetalleDto list
    /// </summary>
    public List<EgresoDetalleDto> MapToEgresoDetalleDtos(IEnumerable<Egresos> egresos)
    {
        return egresos.Select(MapToEgresoDetalleDto).ToList();
    }

    /// <summary>
    /// Creates a TransaccionPendienteDto for canceled reservations
    /// </summary>
    public TransaccionPendienteDto MapReservaAnuladaToTransaccionPendiente(Reservas reserva)
    {
        return new TransaccionPendienteDto
        {
            PagoId = 0, // Indicates this is not a payment
            HabitacionId = reserva.HabitacionId,
            TarjetaNombre = null,
            Periodo = 0,
            CategoriaNombre = reserva.Habitacion?.Categoria?.NombreCategoria,
            Fecha = reserva.FechaAnula,
            HoraIngreso = reserva.FechaReserva,
            HoraSalida = reserva.FechaAnula,
            MontoAdicional = 0,
            TotalConsumo = 0,
            MontoEfectivo = 0,
            MontoTarjeta = 0,
            MontoBillVirt = 0,
            MontoDescuento = 0,
            Observacion = $"Reserva anulada - {reserva.Habitacion?.NombreHabitacion}",
            TipoHabitacion = reserva.Habitacion?.NombreHabitacion,
            TipoTransaccion = "Anulación"
        };
    }

    /// <summary>
    /// Creates a PagoDetalleCompletoDto for canceled reservations in closure details
    /// </summary>
    public PagoDetalleCompletoDto MapReservaAnuladaToPagoDetalleCompleto(Reservas reserva)
    {
        return new PagoDetalleCompletoDto
        {
            PagoId = 0, // Indicates this is not a payment
            Fecha = reserva.FechaAnula,
            CategoriaNombre = reserva.Habitacion?.Categoria?.NombreCategoria,
            Periodo = 0,
            TarjetaNombre = null,
            HoraIngreso = reserva.FechaReserva,
            HoraSalida = reserva.FechaAnula,
            TotalConsumo = 0,
            MontoAdicional = 0,
            MontoEfectivo = 0,
            MontoTarjeta = 0,
            MontoBillVirt = 0,
            MontoDescuento = 0,
            Observacion = $"Reserva anulada - {reserva.Habitacion?.NombreHabitacion}",
            TipoHabitacion = reserva.Habitacion?.NombreHabitacion,
            TipoTransaccion = "Anulación"
        };
    }
}