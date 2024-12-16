using ApiObjetos.Models;
using ApiObjetos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiObjetos.Models.Sistema;

namespace ApiObjetos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CajaController : ControllerBase
    {
        private readonly HotelDbContext _db;

        public CajaController(HotelDbContext db)
        {
            _db = db;
        }

        private async Task<bool> CrearCaja(int montoInicial, string? observacion)
        {
            Cierre nuevoCierre = new Cierre
            {
                MontoInicialCaja = montoInicial,
                Observaciones = observacion,
                TotalIngresosBillVirt = 0,
                TotalIngresosEfectivo = 0,
                TotalIngresosTarjeta = 0,
                EstadoCierre = false,

            };
            _db.Cierre.Add(nuevoCierre);
            await _db.SaveChangesAsync();
            return true;
        }
        #region Cerrar caja
        [HttpPost]
        [Route("CierreCaja")] // Paga todos los movimientos de una visita
        public async Task<Respuesta> CierreCaja(int montoInicial, string? observacion)
        {
            Respuesta res = new Respuesta();
            try
            {
                decimal? totalUltimoCierre = 0;
                var ultimoCierre = await _db.Cierre.OrderBy(c => c.CierreId).LastOrDefaultAsync();
                if (ultimoCierre == null)
                {
                    ultimoCierre = new Cierre()
                    {
                        TotalIngresosBillVirt = 0,
                        TotalIngresosEfectivo = 0,
                        TotalIngresosTarjeta = 0,
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
                    var pagos = await _db.Pagos
                    .Where(p => p.CierreId == null)
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
                        ultimoCierre.TotalIngresosTarjeta = ultimoCierre.TotalIngresosTarjeta + p.MontoTarjeta;
                        ultimoCierre.TotalIngresosEfectivo = ultimoCierre.TotalIngresosEfectivo + p.MontoEfectivo;
                        ultimoCierre.TotalIngresosBillVirt = ultimoCierre.TotalIngresosBillVirt + p.MontoBillVirt;

                    }
                await _db.SaveChangesAsync();
                await CrearCaja(montoInicial, observacion);




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
        public async Task<Respuesta> GetCierres()
        {
            Respuesta res = new Respuesta();
            try
            {
                var cierres = await _db.Cierre
                    .Include(p => p.Pagos)
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
        public async Task<Respuesta> GetCierresConPagos()
        {
            Respuesta res = new Respuesta();

            try
            {
                // Fetch all necessary data from the database
                var empeños = await _db.Empeño.ToListAsync();

                var cierres = await _db.Cierre
                    .Include(c => c.Pagos)
                    .ToListAsync();

                var pagosSinCierre = await _db.Pagos
                    .Where(p => p.CierreId == null)
                    .ToListAsync();

                var movimientos = await _db.Movimientos
                    .Include(m => m.Visita)
                    .ThenInclude(v => v.Reservas)
                    .ToListAsync();

                var habitaciones = await _db.Habitaciones.ToListAsync();

                // List to store the mapped Cierres with Pagos
                var CierresReturn = new List<object>();

                // Map Cierres and their Pagos
                foreach (var cierre in cierres)
                {
                    var pagosConDetalle = new List<object>();

                    foreach (var pago in cierre.Pagos)
                    {
                        var empeño = empeños.FirstOrDefault(e => e.PagoID == pago.PagoId);
                        if (empeño == null)
                        {
                            var movimiento = movimientos.FirstOrDefault(m => m.PagoId == pago.PagoId);
                            var visita = movimiento?.Visita;
                            var horaIngreso = visita?.FechaPrimerIngreso;
                            var horaSalida = pago.fechaHora;
                            var reservaActiva = visita?.Reservas?.FirstOrDefault(r => r.FechaFin == null);
                            var habitacionNombre = reservaActiva != null
                                ? habitaciones.FirstOrDefault(h => h.HabitacionId == reservaActiva.HabitacionId)?.NombreHabitacion
                                : null;
                            var horaEntrada = reservaActiva != null
                            ? reservaActiva.FechaReserva
                            : null;
                            pagosConDetalle.Add(new
                            {
                                pago.PagoId,
                                Fecha = pago.fechaHora,
                                HoraIngreso = horaEntrada,
                                HoraSalida = horaSalida,
                                pago.MontoEfectivo,
                                pago.MontoTarjeta,
                                pago.MontoBillVirt,
                                pago.MontoDescuento,
                                pago.Observacion,
                                TipoHabitacion = habitacionNombre
                            });
                        }
                        else
                        {
                            pagosConDetalle.Add(new
                            {
                                pago.PagoId,
                                Fecha = pago.fechaHora,
                                HoraIngreso = (DateTime?)null,
                                HoraSalida = (DateTime?)null,
                                pago.MontoEfectivo,
                                pago.MontoTarjeta,
                                pago.MontoBillVirt,
                                pago.MontoDescuento,
                                pago.Observacion,
                                TipoHabitacion = (string?)null
                            });
                        }
                    }

                    CierresReturn.Add(new
                    {
                        cierre.CierreId,
                        cierre.FechaHoraCierre,
                        cierre.TotalIngresosEfectivo,
                        cierre.TotalIngresosBillVirt,
                        cierre.TotalIngresosTarjeta,
                        cierre.Observaciones,
                        cierre.EstadoCierre,
                        cierre.MontoInicialCaja,
                        Pagos = pagosConDetalle
                    });
                }

                // Handle Pagos without associated Cierres
                var PagosSinCierreReturn = new List<object>();


                // Handle Pagos without associated Cierres
                foreach (var pago in pagosSinCierre)
                {
                    var empeño = empeños.FirstOrDefault(e => e.PagoID == pago.PagoId);

                    if (empeño == null) {
                        var movimiento = movimientos.FirstOrDefault(m => m.PagoId == pago.PagoId);
                        var visita = movimiento?.Visita;
                    var hora = visita?.FechaPrimerIngreso;
                    var reservaActiva = visita?.Reservas?.FirstOrDefault(r => r.FechaFin == null);
                    var habitacionNombre = reservaActiva != null
                        ? habitaciones.FirstOrDefault(h => h.HabitacionId == reservaActiva.HabitacionId)?.NombreHabitacion
                        : null;
                        var horaEntrada = reservaActiva != null
                        ? reservaActiva.FechaReserva
                        : null;
                        PagosSinCierreReturn.Add(new
                        {
                            pago.PagoId,
                            Fecha = pago.fechaHora,
                            HoraIngreso = horaEntrada,
                            HoraSalida = pago.fechaHora,
                            pago.MontoEfectivo,
                            pago.MontoTarjeta,
                            pago.MontoBillVirt,
                            pago.MontoDescuento,
                            pago.Observacion,
                            TipoHabitacion = habitacionNombre
                        });
                    }
                    else
                    {
                        PagosSinCierreReturn.Add(new
                        {
                            pago.PagoId,
                            Fecha = pago.fechaHora,
                            HoraIngreso = (DateTime?)null,
                            HoraSalida = (DateTime?)null,
                            pago.MontoEfectivo,
                            pago.MontoTarjeta,
                            pago.MontoBillVirt,
                            pago.MontoDescuento,
                            pago.Observacion,
                            TipoHabitacion = (string?)null
                        });
                    }

                }

                // Set the response
                res.Ok = true;
                res.Data = new
                {
                    Cierres = CierresReturn,
                    PagosSinCierre = PagosSinCierreReturn
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




        #region Get Cierre
        [HttpGet]
        [Route("GetCierre")] 
        public async Task<Respuesta> GetCierre(int idCierre)
        {
            Respuesta res = new Respuesta();
            try
            {
                var cierre = await _db.Cierre
                    .Where(c => c.CierreId == idCierre)
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



    }
}
