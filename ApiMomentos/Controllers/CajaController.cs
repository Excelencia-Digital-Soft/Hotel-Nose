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
                // Get all Cierres and related Pagos using a left join concept
                var cierres = await _db.Cierre
                    .Select(c => new
                    {
                        c.CierreId,
                        c.FechaHoraCierre,
                        c.TotalIngresosEfectivo,
                        c.TotalIngresosBillVirt,
                        c.TotalIngresosTarjeta,
                        c.Observaciones,
                        c.EstadoCierre,
                        c.MontoInicialCaja,
                        Pagos = _db.Pagos
                            .Where(p => p.CierreId == c.CierreId)
                            .Select(p => new
                            {
                                p.PagoId,
                                p.MontoEfectivo,
                                p.MontoTarjeta,
                                p.MontoBillVirt,
                                p.MontoDescuento,
                                p.fechaHora,
                                p.MedioPagoId,
                                p.CierreId
                            })
                            .ToList()
                    })
                    .ToListAsync();

                // Get all Pagos with no associated Cierre (cierreId is null)
                var pagosSinCierre = await _db.Pagos
                    .Where(p => p.CierreId == null)
                    .Select(p => new
                    {
                        p.PagoId,
                        p.MontoEfectivo,
                        p.MontoTarjeta,
                        p.MontoBillVirt,
                        p.MontoDescuento,
                        p.MedioPagoId,
                        CierreId = (int?)null
                    })
                    .ToListAsync();

                // Returning the data as part of the response
                res.Ok = true;
                res.Data = new
                {
                    Cierres = cierres,
                    PagosSinCierre = pagosSinCierre
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
