using System.Data.SqlTypes;
using hotel.Data;
using hotel.Interfaces;
using hotel.Models;
using hotel.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CajaController : ControllerBase
    {
        private readonly HotelDbContext _db;
        private readonly IRegistrosService _registrosService;

        public CajaController(HotelDbContext db, IRegistrosService registrosService)
        {
            _db = db;
            _registrosService = registrosService;
        }

        [Obsolete("Use ICajaService.CrearCajaAsync instead")]
        private async Task<bool> CrearCaja(int montoInicial, int institucionID, string? observacion)
        {
            Cierre nuevoCierre = new Cierre
            {
                MontoInicialCaja = montoInicial,
                Observaciones = observacion,
                TotalIngresosBillVirt = 0,
                TotalIngresosEfectivo = 0,
                TotalIngresosTarjeta = 0,
                EstadoCierre = false,
                InstitucionID = institucionID,
            };
            _db.Cierre.Add(nuevoCierre);
            await _db.SaveChangesAsync();
            return true;
        }

        #region Cerrar caja
        [HttpPost]
        [Route("CierreCaja")] // Paga todos los movimientos de una visita
        [Obsolete("This endpoint is deprecated. Use POST /api/v1/caja for creating new cash registers and other V1 endpoints for cash operations.")]
        public async Task<Respuesta> CierreCaja(
            int montoInicial,
            int institucionID,
            string? observacion
        )
        {
            Respuesta res = new Respuesta();
            try
            {
                var ultimoCierre = await _db
                    .Cierre.Where(c => c.InstitucionID == institucionID)
                    .OrderBy(c => c.CierreId)
                    .LastOrDefaultAsync();
                if (ultimoCierre == null)
                {
                    ultimoCierre = new Cierre()
                    {
                        TotalIngresosBillVirt = 0,
                        TotalIngresosEfectivo = 0,
                        TotalIngresosTarjeta = 0,
                        InstitucionID = institucionID,
                    };
                    _db.Cierre.Add(ultimoCierre);
                    await _db.SaveChangesAsync();
                }
                ultimoCierre.EstadoCierre = true;
                ultimoCierre.FechaHoraCierre = DateTime.Now;
                if (observacion != null)
                {
                    ultimoCierre.Observaciones = observacion;
                }
                var pagos = await _db
                    .Pagos.Where(p => p.CierreId == null && p.InstitucionID == institucionID)
                    .ToListAsync();
                if (pagos.Count == 0)
                {
                    res.Ok = false;
                    res.Message = "No hay pagos para cerrar la caja";
                    return res;
                }

                foreach (var p in pagos)
                {
                    p.CierreId = ultimoCierre.CierreId;
                    ultimoCierre.Pagos.Add(p);
                    ultimoCierre.TotalIngresosTarjeta =
                        ultimoCierre.TotalIngresosTarjeta + p.MontoTarjeta;
                    ultimoCierre.TotalIngresosEfectivo =
                        ultimoCierre.TotalIngresosEfectivo + p.MontoEfectivo;
                    ultimoCierre.TotalIngresosBillVirt =
                        ultimoCierre.TotalIngresosBillVirt + p.MontoBillVirt;
                }
                var egresos = await _db
                    .Egresos.Where(p => p.CierreID == null && p.InstitucionID == institucionID)
                    .ToListAsync();
                foreach (var e in egresos)
                {
                    e.CierreID = ultimoCierre.CierreId;
                    ultimoCierre.Egresos!.Add(e);
                    ultimoCierre.TotalIngresosEfectivo =
                        ultimoCierre.TotalIngresosEfectivo - (e.Precio * e.Cantidad);
                }
                await _db.SaveChangesAsync();
                await CrearCaja(montoInicial, institucionID, observacion);

                res.Ok = true;
                res.Message = "El cierre se registró correctamente.";
                res.Data = ultimoCierre;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message} {ex.InnerException?.Message}";
            }

            return res;
        }

        #endregion

        #region Get Cierres
        [HttpGet]
        [Route("GetCierres")]
        [Obsolete("This endpoint is deprecated. Use GET /api/v1/caja for retrieving cash register closures.")]
        public async Task<Respuesta> GetCierres()
        {
            Respuesta res = new Respuesta();
            try
            {
                var cierres = await _db
                    .Cierre.Include(p => p.Pagos)
                    .Include(p => p.Usuario)
                    .ToListAsync();

                res.Ok = true;
                res.Data = cierres;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }

        #endregion
        [HttpGet]
        [Route("GetCierresConPagos")]
        [Obsolete("This endpoint is deprecated. Use GET /api/v1/caja/con-pagos for retrieving cash closures with payment details.")]
        public async Task<Respuesta> GetCierresConPagos(int institucionID)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Fetch all necessary data from the database
                var empeños = await _db.Empeño.ToListAsync();

                var cierres = await _db
                    .Cierre.Where(c =>
                        c.FechaHoraCierre != null && c.InstitucionID == institucionID
                    )
                    .Include(c => c.Pagos)
                    .ToListAsync();

                List<Pagos>? pagosSinCierre =
                    _db.Pagos != null
                        ? await _db
                            .Pagos.Where(p =>
                                p.CierreId == null && p.InstitucionID == institucionID
                            )
                            .ToListAsync()
                        : new List<Pagos>();

                var movimientos = await _db
                    .Movimientos.Where(c => c.InstitucionID == institucionID)
                    .Include(m => m.Visita)
                    .ThenInclude(v => v!.Reservas)
                    .ToListAsync();

                var habitaciones = await _db.Habitaciones.Include(h => h.Categoria).ToListAsync();
                var consumos = await _db.Consumo.ToListAsync();
                var tarjetas = await _db.Tarjetas.ToListAsync();
                // List to store the mapped Cierres with Pagos
                var CierresReturn = new List<object>();

                // Map Cierres and their Pagos
                foreach (var cierre in cierres)
                {
                    var pagosConDetalle = new List<object>();

                    foreach (var pago in cierre.Pagos)
                    {
                        var tarjeta = tarjetas.FirstOrDefault(t => pago.TarjetaId == t.TarjetaID);
                        var empeño = empeños.FirstOrDefault(e => e.PagoID == pago.PagoId);
                        if (empeño == null)
                        {
                            var movimiento = movimientos.FirstOrDefault(m =>
                                m.PagoId == pago.PagoId
                            );
                            var visita = movimiento?.Visita;
                            var horaSalida = pago.fechaHora;
                            var ultimaReserva = visita?.Reservas?.FirstOrDefault();
                            var movimientosPago = movimientos
                                .Where(m => m.PagoId == pago.PagoId)
                                .ToList(); // realmente debería haberse hecho pensando en multiples movimientos desde un principio, si es posible arreglar a futuro.
                            decimal? totalConsumo = consumos
                                .Where(c =>
                                    movimientosPago.Any(m => m.MovimientosId == c.MovimientosId)
                                )
                                .Sum(c => c.PrecioUnitario * (c.Cantidad ?? 1));
                            var habitacionNombre =
                                ultimaReserva != null
                                    ? habitaciones
                                        .FirstOrDefault(h =>
                                            h.HabitacionId == ultimaReserva.HabitacionId
                                        )
                                        ?.NombreHabitacion
                                    : null;
                            var horaEntrada =
                                ultimaReserva != null ? ultimaReserva.FechaReserva : null;
                            var categoriaNombre =
                                ultimaReserva != null
                                    ? habitaciones
                                        .FirstOrDefault(h =>
                                            h.HabitacionId == ultimaReserva.HabitacionId
                                        )
                                        ?.Categoria!.NombreCategoria
                                    : null;
                            decimal? Periodo = 0;
                            if (movimiento != null)
                                Periodo = movimiento.TotalFacturado;
                            pagosConDetalle.Add(
                                new
                                {
                                    pago.PagoId,
                                    categoriaNombre,
                                    Periodo,
                                    Fecha = pago.fechaHora,
                                    TarjetaNombre = tarjeta?.Nombre ?? null,
                                    HoraIngreso = horaEntrada,
                                    HoraSalida = horaSalida,
                                    totalConsumo,
                                    MontoAdicional = pago.Adicional ?? 0,
                                    pago.MontoEfectivo,
                                    pago.MontoTarjeta,
                                    pago.MontoBillVirt,
                                    pago.MontoDescuento,
                                    pago.Observacion,
                                    TipoHabitacion = habitacionNombre,
                                }
                            );
                        }
                        else
                        {
                            pagosConDetalle.Add(
                                new
                                {
                                    pago.PagoId,
                                    Fecha = pago.fechaHora,
                                    Periodo = 0,
                                    HoraIngreso = (DateTime?)null,
                                    TarjetaNombre = tarjeta?.Nombre ?? null,
                                    HoraSalida = (DateTime?)null,
                                    totalConsumo = 0,
                                    MontoAdicional = 0,
                                    pago.Adicional,
                                    pago.MontoEfectivo,
                                    pago.MontoTarjeta,
                                    pago.MontoBillVirt,
                                    pago.MontoDescuento,
                                    pago.Observacion,
                                    TipoHabitacion = (string?)null,
                                }
                            );
                        }
                    }

                    CierresReturn.Add(
                        new
                        {
                            cierre.CierreId,
                            cierre.FechaHoraCierre,
                            cierre.TotalIngresosEfectivo,
                            cierre.TotalIngresosBillVirt,
                            cierre.TotalIngresosTarjeta,
                            cierre.Observaciones,
                            cierre.EstadoCierre,
                            cierre.MontoInicialCaja,
                            Pagos = pagosConDetalle,
                        }
                    );
                }

                // Handle Pagos without associated Cierres
                var PagosSinCierreReturn = new List<object>();

                // Handle Pagos without associated Cierres
                foreach (var pago in pagosSinCierre)
                {
                    var empeño = empeños.FirstOrDefault(e => e.PagoID == pago.PagoId);
                    var tarjeta = tarjetas.FirstOrDefault(t => pago.TarjetaId == t.TarjetaID);

                    if (empeño == null)
                    {
                        var movimiento = movimientos.FirstOrDefault(m => m.PagoId == pago.PagoId);
                        var movimientosPago = movimientos
                            .Where(m => m.PagoId == pago.PagoId)
                            .ToList(); // realmente debería haberse hecho pensando en multiples movimientos desde un principio, si es posible arreglar a futuro.
                        decimal? totalConsumo = consumos
                            .Where(c =>
                                movimientosPago.Any(m => m.MovimientosId == c.MovimientosId)
                            )
                            .Sum(c => c.PrecioUnitario * (c.Cantidad ?? 1));
                        var visita = movimiento?.Visita;
                        var ultimaReserva = visita?.Reservas?.FirstOrDefault();
                        var habitacionNombre =
                            ultimaReserva != null
                                ? habitaciones
                                    .FirstOrDefault(h =>
                                        h.HabitacionId == ultimaReserva.HabitacionId
                                    )
                                    ?.NombreHabitacion
                                : null;
                        var horaEntrada = ultimaReserva != null ? ultimaReserva.FechaReserva : null;
                        var categoriaNombre =
                            ultimaReserva != null
                                ? habitaciones
                                    .FirstOrDefault(h =>
                                        h.HabitacionId == ultimaReserva.HabitacionId
                                    )
                                    ?.Categoria!.NombreCategoria
                                : null;
                        decimal? Periodo = 0;
                        if (movimiento != null)
                            Periodo = movimiento.TotalFacturado;
                        PagosSinCierreReturn.Add(
                            new
                            {
                                pago.PagoId,
                                HabitacionID = ultimaReserva!.HabitacionId ?? null,
                                Periodo,
                                categoriaNombre,
                                TarjetaNombre = tarjeta?.Nombre ?? null,
                                Fecha = pago.fechaHora,
                                HoraIngreso = horaEntrada,
                                HoraSalida = pago.fechaHora,
                                MontoAdicional = pago.Adicional ?? 0,
                                totalConsumo,
                                pago.MontoEfectivo,
                                pago.MontoTarjeta,
                                pago.MontoBillVirt,
                                pago.MontoDescuento,
                                pago.Observacion,
                                TipoHabitacion = habitacionNombre,
                            }
                        );
                    }
                    else
                    {
                        PagosSinCierreReturn.Add(
                            new
                            {
                                pago.PagoId,
                                Fecha = pago.fechaHora,
                                Periodo = 0,
                                TarjetaNombre = tarjeta?.Nombre ?? null,
                                HoraIngreso = (DateTime?)null,
                                HoraSalida = (DateTime?)null,
                                totalConsumo = 0,
                                MontoAdicional = 0,
                                pago.MontoEfectivo,
                                pago.MontoTarjeta,
                                pago.MontoBillVirt,
                                pago.MontoDescuento,
                                pago.Observacion,
                                TipoHabitacion = (string?)null,
                            }
                        );
                    }
                }
                var reversedCierres = CierresReturn.AsEnumerable().Reverse().ToList();

                // Set the response
                res.Ok = true;
                res.Data = new { Cierres = reversedCierres, PagosSinCierre = PagosSinCierreReturn };
            }
            catch (Exception ex)
            {
                // Handle errors
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }

        #region Get Cierre
        [HttpGet]
        [Route("GetCierre")]
        [Obsolete("This endpoint is deprecated. Use GET /api/v1/caja/{id} for retrieving a specific cash register closure.")]
        public async Task<Respuesta> GetCierre(int idCierre)
        {
            Respuesta res = new Respuesta();
            try
            {
                var cierre = await _db
                    .Cierre.Where(c => c.CierreId == idCierre)
                    .Include(p => p.Pagos)
                    .Include(p => p.Usuario)
                    .ToListAsync();

                res.Ok = true;
                res.Data = cierre;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }
        #endregion

        [HttpGet]
        [Route("GetDetalleCierre")]
        [Obsolete("This endpoint is deprecated. Use GET /api/v1/caja/{id}/detalle for retrieving complete cash register closure details.")]
        public async Task<Respuesta> GetDetalleCierre(int idCierre)
        {
            Respuesta res = new Respuesta();
            try
            {
                var cierre = await _db
                    .Cierre.Where(c => c.CierreId == idCierre)
                    .Include(c => c.Pagos)
                    .FirstOrDefaultAsync();

                if (cierre == null)
                {
                    res.Ok = false;
                    res.Message = "No se encontró el cierre.";
                    return res;
                }

                // Obtener cierre anterior
                var cierreAnterior = await _db
                    .Cierre.Where(c => c.FechaHoraCierre < cierre.FechaHoraCierre)
                    .OrderByDescending(c => c.FechaHoraCierre)
                    .FirstOrDefaultAsync();

                var fechaCierreAnterior = cierreAnterior?.FechaHoraCierre ?? DateTime.MinValue;

                // Obtener datos auxiliares
                var habitaciones = await _db.Habitaciones.Include(h => h.Categoria).ToListAsync();
                var anulados =
                    await _db
                        .Reservas.Where(r =>
                            r.FechaAnula < cierre.FechaHoraCierre
                            && r.FechaAnula > fechaCierreAnterior
                        )
                        .ToListAsync() ?? null;
                var empeños = await _db
                    .Empeño.Where(e => e.InstitucionID == cierre.InstitucionID)
                    .ToListAsync();
                var movimientos = await _db
                    .Movimientos.Include(m => m.Visita)
                    .ThenInclude(v => v!.Reservas)
                    .ToListAsync();
                var consumos = await _db.Consumo.ToListAsync();
                var tarjetas = await _db.Tarjetas.ToListAsync();

                var pagosConDetalle = new List<object>();

                // Agregar pagos normales con detalles adicionales
                foreach (var pago in cierre.Pagos)
                {
                    var tarjeta = tarjetas.FirstOrDefault(t => pago.TarjetaId == t.TarjetaID);
                    var empeño = empeños.FirstOrDefault(e => e.PagoID == pago.PagoId);
                    if (empeño == null)
                    {
                        var movimiento = movimientos.FirstOrDefault(m => m.PagoId == pago.PagoId);
                        var visita = movimiento?.Visita;
                        var horaSalida = pago.fechaHora;
                        var ultimaReserva = visita?.Reservas?.FirstOrDefault();

                        var movimientosPago = movimientos
                            .Where(m => m.PagoId == pago.PagoId)
                            .ToList();
                        decimal? totalConsumo = consumos
                            .Where(c =>
                                movimientosPago.Any(m => m.MovimientosId == c.MovimientosId)
                            )
                            .Sum(c => c.PrecioUnitario * (c.Cantidad ?? 1));

                        var habitacion =
                            ultimaReserva != null
                                ? habitaciones.FirstOrDefault(h =>
                                    h.HabitacionId == ultimaReserva.HabitacionId
                                )
                                : null;

                        decimal? periodo = movimiento?.TotalFacturado ?? 0;

                        pagosConDetalle.Add(
                            new
                            {
                                pago.PagoId,
                                CategoriaNombre = habitacion?.Categoria!.NombreCategoria,
                                Periodo = periodo,
                                TarjetaNombre = tarjeta?.Nombre ?? null,
                                Fecha = pago.fechaHora,
                                HoraIngreso = ultimaReserva?.FechaReserva,
                                HoraSalida = horaSalida,
                                TotalConsumo = totalConsumo ?? 0,
                                MontoAdicional = pago.Adicional ?? 0,
                                pago.MontoEfectivo,
                                pago.MontoTarjeta,
                                pago.MontoBillVirt,
                                pago.MontoDescuento,
                                pago.Observacion,
                                TipoHabitacion = habitacion?.NombreHabitacion,
                            }
                        );
                    }
                    else
                    {
                        pagosConDetalle.Add(
                            new
                            {
                                pago.PagoId,
                                Fecha = pago.fechaHora,
                                Periodo = 0,
                                TarjetaNombre = tarjeta?.Nombre ?? null,
                                HoraIngreso = (DateTime?)null,
                                HoraSalida = (DateTime?)null,
                                totalConsumo = 0,
                                MontoAdicional = 0,
                                pago.MontoEfectivo,
                                pago.MontoTarjeta,
                                pago.MontoBillVirt,
                                pago.MontoDescuento,
                                pago.Observacion,
                                TipoHabitacion = (string?)null,
                            }
                        );
                    }
                }

                // Agregar reservas anuladas
                foreach (var reserva in anulados!)
                {
                    var nombreHabitacion = habitaciones
                        .Where(h => h.HabitacionId == reserva.HabitacionId)
                        .Select(h => h.NombreHabitacion)
                        .FirstOrDefault();

                    // Obtener el registro de anulación usando el servicio
                    var registrosResult = await _registrosService.GetRegistrosAsync(
                        habitaciones.FirstOrDefault()?.InstitucionID ?? 1,
                        null,
                        ModuloSistema.RESERVAS,
                        null,
                        null,
                        null,
                        1,
                        1
                    );

                    var reservaAnulada =
                        registrosResult.IsSuccess && registrosResult.Data?.Registros.Any() == true
                            ? registrosResult.Data.Registros.FirstOrDefault(r =>
                                r.ReservaId == reserva.ReservaId
                            )
                            : null;

                    pagosConDetalle.Add(
                        new
                        {
                            PagoId = 0,
                            Fecha = reserva.FechaAnula,
                            Periodo = 0,
                            HoraIngreso = reserva.FechaReserva,
                            HoraSalida = reserva.FechaAnula,
                            TotalConsumo = 0,
                            MontoAdicional = 0,
                            MontoEfectivo = 0,
                            MontoTarjeta = 0,
                            MontoBillVirt = 0,
                            MontoDescuento = 0,
                            Observacion = reservaAnulada?.Contenido,
                            TipoHabitacion = nombreHabitacion,
                        }
                    );
                }
                var egresosReturn = new List<object>();

                var egresos = await _db
                    .Egresos.Where(e => e.CierreID == idCierre)
                    .Include(e => e.TipoEgreso) // Ensure TipoEgreso is loaded
                    .ToListAsync();

                foreach (var egreso in egresos)
                {
                    egresosReturn.Add(
                        new
                        {
                            PagoId = 0,
                            Periodo = 0,
                            HoraIngreso = (DateTime?)null,
                            HoraSalida = (DateTime?)null,
                            TipoHabitacion = (string?)null,
                            totalConsumo = 0,
                            MontoAdicional = 0,
                            MontoEfectivo = egreso.Cantidad * egreso.Precio, // Assuming 'Cierre' was a mistake, using 'Precio'
                            MontoTarjeta = 0,
                            MontoBillVirt = 0,
                            MontoDescuento = 0,
                            Fecha = egreso.Fecha,
                            Observacion = "Pago de egreso por: " + egreso.TipoEgreso?.Nombre, // Ensure TipoEgreso is not null
                        }
                    );
                }
                // Retornar respuesta con los datos del cierre
                res.Ok = true;
                res.Data = new
                {
                    cierre.CierreId,
                    cierre.FechaHoraCierre,
                    cierre.EstadoCierre,
                    cierre.TotalIngresosEfectivo,
                    cierre.TotalIngresosBillVirt,
                    cierre.TotalIngresosTarjeta,
                    cierre.MontoInicialCaja,
                    cierre.Observaciones,
                    Pagos = pagosConDetalle,
                    egresos = egresosReturn,
                };
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }

        [HttpGet("GetCierresyActual")]
        [Obsolete("This endpoint is deprecated. Use GET /api/v1/caja/actual for retrieving cash closures and current transactions.")]
        public async Task<Respuesta> GetCierresyActual(int InstitucionID)
        {
            Respuesta res = new Respuesta();
            try
            {
                //las hab anuladas deberían aparecer dentro de los pagos sin cierre que sería el cierre actual
                //ademas para encasillarlas dentro de un cierre anterior deberían tener la fecha anula dentro del marco
                //de fecha fin y fecha inicio de ese cierre
                // Fetch all necessary data from the database
                var empeños = await _db
                    .Empeño.Where(e => e.InstitucionID == InstitucionID)
                    .ToListAsync();

                var cierres = await _db
                    .Cierre.Where(c =>
                        c.FechaHoraCierre != null && c.InstitucionID == InstitucionID
                    )
                    .Include(c => c.Pagos)
                    .ToListAsync();

                List<Pagos>? pagosSinCierre =
                    _db.Pagos != null
                        ? await _db
                            .Pagos.Where(p =>
                                p.CierreId == null && p.InstitucionID == InstitucionID
                            )
                            .ToListAsync()
                        : new List<Pagos>();

                var movimientos = await _db
                    .Movimientos.Include(m => m.Visita)
                    .ThenInclude(v => v!.Reservas)
                    .ToListAsync();
                var ultimocierre =
                    cierres.Where(c => c.InstitucionID == InstitucionID).LastOrDefault()
                    ?? new Cierre { FechaHoraCierre = SqlDateTime.MinValue.Value };

                var anulados = await _db
                    .Reservas.Where(r =>
                        r.FechaAnula > ultimocierre.FechaHoraCierre
                        && r.InstitucionID == InstitucionID
                    )
                    .ToListAsync();

                var habitaciones = await _db
                    .Habitaciones.Include(h => h.Categoria)
                    .Where(h => h.Categoria!.InstitucionID == InstitucionID)
                    .ToListAsync();
                var consumos = await _db.Consumo.ToListAsync();
                var tarjetas = await _db.Tarjetas.ToListAsync();
                // List to store the mapped Cierres with Pagos
                var CierresReturn = new List<object>();

                // Handle Pagos without associated Cierres
                var PagosSinCierreReturn = new List<object>();
                //ocupaciones Anuladas
                var OcupacionesAnuladas = new List<object>();

                // Handle Pagos without associated Cierres
                foreach (var pago in pagosSinCierre)
                {
                    var empeño = empeños.FirstOrDefault(e => e.PagoID == pago.PagoId);
                    var tarjeta = tarjetas.FirstOrDefault(t => pago.TarjetaId == t.TarjetaID);

                    if (empeño == null)
                    {
                        var movimiento = movimientos.FirstOrDefault(m => m.PagoId == pago.PagoId);
                        var movimientosPago = movimientos
                            .Where(m => m.PagoId == pago.PagoId)
                            .ToList(); // realmente debería haberse hecho pensando en multiples movimientos desde un principio, si es posible arreglar a futuro.
                        decimal? totalConsumo = consumos
                            .Where(c =>
                                movimientosPago.Any(m => m.MovimientosId == c.MovimientosId)
                            )
                            .Sum(c => c.PrecioUnitario * (c.Cantidad ?? 1));
                        var visita = movimiento?.Visita;
                        var ultimaReserva = visita?.Reservas?.FirstOrDefault();
                        var habitacionNombre =
                            ultimaReserva != null
                                ? habitaciones
                                    .FirstOrDefault(h =>
                                        h.HabitacionId == ultimaReserva.HabitacionId
                                    )
                                    ?.NombreHabitacion
                                : null;
                        var horaEntrada = ultimaReserva != null ? ultimaReserva.FechaReserva : null;
                        var categoriaNombre =
                            ultimaReserva != null
                                ? habitaciones
                                    .FirstOrDefault(h =>
                                        h.HabitacionId == ultimaReserva.HabitacionId
                                    )
                                    ?.Categoria!.NombreCategoria
                                : null;
                        decimal? Periodo = 0;
                        if (movimiento != null)
                            Periodo = movimiento.TotalFacturado;
                        PagosSinCierreReturn.Add(
                            new
                            {
                                pago.PagoId,
                                HabitacionID = ultimaReserva!.HabitacionId ?? null,
                                TarjetaNombre = tarjeta?.Nombre ?? null,
                                Periodo,
                                categoriaNombre,
                                Fecha = pago.fechaHora,
                                HoraIngreso = horaEntrada,
                                HoraSalida = pago.fechaHora,
                                MontoAdicional = pago.Adicional ?? 0,
                                totalConsumo,
                                pago.MontoEfectivo,
                                pago.MontoTarjeta,
                                pago.MontoBillVirt,
                                pago.MontoDescuento,
                                pago.Observacion,
                                TipoHabitacion = habitacionNombre,
                            }
                        );
                    }
                    else
                    {
                        PagosSinCierreReturn.Add(
                            new
                            {
                                pago.PagoId,
                                Fecha = pago.fechaHora,
                                Periodo = 0,
                                TarjetaNombre = tarjeta?.Nombre ?? null,
                                HoraIngreso = (DateTime?)null,
                                HoraSalida = (DateTime?)null,
                                totalConsumo = 0,
                                MontoAdicional = 0,
                                pago.MontoEfectivo,
                                pago.MontoTarjeta,
                                pago.MontoBillVirt,
                                pago.MontoDescuento,
                                pago.Observacion,
                                TipoHabitacion = (string?)null,
                            }
                        );
                    }
                }
                if (anulados.Count() != 0)
                {
                    foreach (var row in anulados)
                    {
                        var nombreHabitacion = habitaciones
                            .Where(r => r.HabitacionId == row.HabitacionId)
                            .Select(r => r.NombreHabitacion)
                            .FirstOrDefault();

                        // Obtener el registro de anulación usando el servicio
                        var registrosAnuladoResult = await _registrosService.GetRegistrosAsync(
                            habitaciones.FirstOrDefault()?.InstitucionID ?? 1,
                            null,
                            ModuloSistema.RESERVAS,
                            null,
                            null,
                            null,
                            1,
                            1
                        );

                        var reservaAnulada =
                            registrosAnuladoResult.IsSuccess
                            && registrosAnuladoResult.Data?.Registros.Any() == true
                                ? registrosAnuladoResult.Data.Registros.FirstOrDefault(r =>
                                    r.ReservaId == row.ReservaId
                                )
                                : null;
                        PagosSinCierreReturn.Add(
                            new
                            {
                                PagoId = 0,
                                Fecha = row.FechaAnula,
                                Periodo = 0,
                                HoraIngreso = row.FechaReserva,
                                HoraSalida = row.FechaAnula,
                                totalConsumo = 0,
                                MontoAdicional = 0,
                                MontoEfectivo = 0,
                                MontoTarjeta = 0,
                                MontoBillVirt = 0,
                                MontoDescuento = 0,
                                Observacion = reservaAnulada?.Contenido,
                                TipoHabitacion = nombreHabitacion,
                            }
                        );
                    }
                }
                var egresosSinCierreReturn = new List<object>();

                var egresosSinCierre = await _db
                    .Egresos.Where(e => e.CierreID == null && e.InstitucionID == InstitucionID)
                    .Include(e => e.TipoEgreso) // Ensure TipoEgreso is loaded
                    .ToListAsync();

                foreach (var egreso in egresosSinCierre)
                {
                    egresosSinCierreReturn.Add(
                        new
                        {
                            PagoId = 0,
                            Periodo = 0,
                            HoraIngreso = (DateTime?)null,
                            HoraSalida = (DateTime?)null,
                            TipoHabitacion = (string?)null,
                            totalConsumo = 0,
                            MontoAdicional = 0,
                            MontoEfectivo = egreso.Cantidad * egreso.Precio, // Assuming 'Cierre' was a mistake, using 'Precio'
                            MontoTarjeta = 0,
                            MontoBillVirt = 0,
                            MontoDescuento = 0,
                            Fecha = egreso.Fecha,
                            Observacion = "Pago de egreso por: " + egreso.TipoEgreso?.Nombre, // Ensure TipoEgreso is not null
                        }
                    );
                }
                var reversedCierres = cierres.AsEnumerable().Reverse().ToList();

                // Set the response
                res.Ok = true;
                res.Data = new
                {
                    Cierres = reversedCierres,
                    PagosSinCierre = PagosSinCierreReturn,
                    egresos = egresosSinCierreReturn,
                };
            }
            catch (Exception ex)
            {
                // Handle errors
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }
    }
}
