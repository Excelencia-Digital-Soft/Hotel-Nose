using ApiObjetos.Models;
using ApiObjetos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiObjetos.Models.Sistema;

namespace ApiObjetos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagoController : ControllerBase
    {
        private readonly HotelDbContext _db;

        public PagoController(HotelDbContext db)
        {
            _db = db;
        }

        #region Create Pago
        [HttpPost]
        [Route("PagarVisita")] // Paga todos los movimientos de una visita
        public async Task<Respuesta> PagarVisita(int visitaId, decimal montoDescuento, decimal montoEfectivo, decimal montoTarjeta, decimal montoBillVirt, int medioPagoId, string? comentario, decimal? montoRecargo, string? descripcionRecargo)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Step 1: Find the Visita by visitaId
                var visita = await _db.Visitas.FindAsync(visitaId);
                if (visita == null)
                {
                    res.Ok = false;
                    res.Message = "La visita no existe.";
                    return res;
                }

                // Step 2: Find all Movimientos related to the Visita
                var movimientos = await _db.Movimientos
                    .Where(m => m.VisitaId == visitaId && m.PagoId == null) // Only unpaid Movimientos
                    .ToListAsync();

                if (movimientos.Count == 0)
                {
                    res.Ok = false;
                    res.Message = "No hay movimientos sin pagar para esta visita.";
                    return res;
                }

                // Step 3: Verify that the MedioPago exists
                var medioPago = await _db.MediosPago.FindAsync(medioPagoId);
                if (medioPago == null)
                {
                    res.Ok = false;
                    res.Message = "El medio de pago no existe.";
                    return res;
                }

                // Step 4: Calculate the total amount to be paid (sum of all movimientos' totalFacturado)
                decimal totalFacturado = movimientos.Sum(m => m.TotalFacturado ?? 0);
                string observacion = "-";
                if (comentario != null) observacion = comentario;

                // Step 5: Create a new Pago for the Visita with the respective payment methods
                Pagos nuevoPago = new Pagos
                {
                    MontoDescuento = montoDescuento,
                    MontoEfectivo = montoEfectivo,
                    MontoTarjeta = montoTarjeta,
                    MontoBillVirt = montoBillVirt,
                    MedioPagoId = medioPagoId,
                    fechaHora = DateTime.Now,
                    Observacion = observacion,
                };
                _db.Pagos.Add(nuevoPago);
                await _db.SaveChangesAsync();

                if (montoRecargo != null)
                {
                    Recargos nuevoRecargo = new Recargos
                    {
                        Valor = montoRecargo,
                        Descripcion = descripcionRecargo,
                        PagoID = nuevoPago.PagoId,
                    };
                    _db.Recargos.Add(nuevoRecargo);

                }
                await _db.SaveChangesAsync();

                // Step 6: Update all Movimientos related to the Visita to include the PagoId
                foreach (var movimiento in movimientos)
                {
                    movimiento.PagoId = nuevoPago.PagoId;
                }

                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "El pago de la visita se realizó correctamente.";
                res.Data = nuevoPago;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message} {ex.InnerException?.Message}";
            }

            return res;
        }

        #endregion

        #region Get Pago
        [HttpGet]
        [Route("GetPago/{pagoId}")] // Obtiene un pago basado en su id
        public async Task<Respuesta> GetPago(int pagoId)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Find the Pago with the given id and include related Movimiento and MedioPago
                var pago = await _db.Pagos
                    .Include(p => p.MedioPago)      // Include related MedioPago
                    .Include(p => p.Movimientos)    // Include related Movimientos
                    .FirstOrDefaultAsync(p => p.PagoId == pagoId);

                if (pago == null)
                {
                    res.Ok = false;
                    res.Message = "El pago no se encontró.";
                    return res;
                }

                res.Ok = true;
                res.Data = pago; // Return the Pago data
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }
        #endregion


        #region Get All Pagos
        [HttpGet]
        [Route("GetPagos")] // Obtiene todos los pagos
        public async Task<Respuesta> GetPagos()
        {
            Respuesta res = new Respuesta();
            try
            {
                // Get all Pagos from the database, including related MedioPago and Movimientos
                var pagos = await _db.Pagos
                    .Include(p => p.MedioPago)
                    .Include(p => p.Movimientos)
                    .ToListAsync();

                res.Ok = true;
                res.Data = pagos;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }
        #endregion

        #region Get Medios de Pago
        [HttpGet]
        [Route("GetMediosPago")] // Obtiene todos los medios de pago disponibles
        public async Task<Respuesta> GetMediosPago()
        {
            Respuesta res = new Respuesta();
            try
            {
                // Get all MediosPago from the database
                var mediosPago = await _db.MediosPago.ToListAsync();

                res.Ok = true;
                res.Data = mediosPago;
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
