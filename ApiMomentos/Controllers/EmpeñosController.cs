using ApiObjetos.Data;
using ApiObjetos.Models;
using ApiObjetos.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApiObjetos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpeñoController : ControllerBase
    {
        private readonly HotelDbContext _db;

        public EmpeñoController(HotelDbContext context)
        {
            _db = context;
        }

        [HttpPost]
        [Route("AddEmpeno")]
        public async Task<Respuesta> AddEmpeno(int institucionID, int visitaID, string detalle, double monto)
        {
            Respuesta res = new Respuesta();

            try
            {
                var newEmpeño = new Empeño
                {
                    VisitaID = visitaID,
                    Detalle = detalle,
                    Monto = monto,
                    FechaRegistro = DateTime.Now, 
                    InstitucionID = institucionID,
                    Anulado = false 
                };

                // Add the Encargo entity to the DbContext
                await _db.Empeño.AddAsync(newEmpeño);
                await _db.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = "Empeño added successfully.";
                res.Data = newEmpeño; // Optionally return the added encargo or any relevant info
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpGet]
        [Route("GetEmpeno")]
        public async Task<Respuesta> GetEmpeno(int empeñoID)
        {
            Respuesta res = new Respuesta();

            try
            {
                var empeño = await _db.Empeño.FindAsync(empeñoID);
                if (empeño == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el empeño con ID: {empeñoID}.";
                    return res;
                }

                // Return the encargo found
                res.Ok = true;
                res.Message = "Empeño found.";
                res.Data = empeño; // Return the encargo
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message} {ex.InnerException}";
                res.Ok = false;
            }

            return res;
        }
        [HttpPost]
        [Route("PagarEmpeno")]
        public async Task<Respuesta> PagarEmpeno(int empeñoID, decimal montoEfectivo = 0, decimal montoTarjeta = 0, decimal montoBillVirt = 0)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Fetch the Empeño by ID
                var empeño = await _db.Empeño.FindAsync(empeñoID);
                if (empeño == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el empeño con ID: {empeñoID}.";
                    return res;
                }

                // Step 2: Fetch the Movimiento corresponding to the VisitaID
                var movimiento = await _db.Movimientos
                    .Where(m => m.VisitaId == empeño.VisitaID)
                    .FirstOrDefaultAsync();

                if (movimiento == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el movimiento para la visita con ID: {empeño.VisitaID}.";
                    return res;
                }

                // Step 3: Create a new Movimiento with the same HabitacionId as the original Movimiento
                var nuevoMovimiento = new Movimientos
                {
                    VisitaId = empeño.VisitaID,
                    TotalFacturado = montoEfectivo + montoTarjeta + montoBillVirt, // assuming the monto is the total facturado for the new movimiento
                    HabitacionId = movimiento.HabitacionId, // Use the same HabitacionId as the existing movimiento
                };

                // Add the new Movimiento to the context
                _db.Movimientos.Add(nuevoMovimiento);
                await _db.SaveChangesAsync();

                // Step 4: Create a new Pago for the new Movimiento
                var nuevoPago = new Pagos
                {
                    MontoDescuento = 0, // assuming no discount for now
                    MontoEfectivo = montoEfectivo,
                    MontoTarjeta = montoTarjeta,
                    MontoBillVirt = montoBillVirt,
                    MedioPagoId = 1, // assuming the MedioPagoId = 1 for now
                    fechaHora = DateTime.Now, // current time as payment time
                    Observacion = "Pago de empeño correspondiente a la visita " + empeño.VisitaID,
                };

                // Add the new Pago to the context
                _db.Pagos.Add(nuevoPago);
                await _db.SaveChangesAsync();

                // Step 5: Link the new Pago to the new Movimiento by setting PagoID in Movimiento
                nuevoMovimiento.PagoId = nuevoPago.PagoId;
                await _db.SaveChangesAsync();

                // Step 6: Link the Pago to the Empeño by setting the PagoID in Empeño
                empeño.PagoID = nuevoPago.PagoId;

                // Save the changes to the Empeño
                await _db.SaveChangesAsync();

                // Return success response
                res.Ok = true;
                res.Message = "Empeño pagado y movimiento creado correctamente.";

            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message} {ex.InnerException}";
                res.Ok = false;
            }

            return res;
        }

        [HttpGet]
        [Route("GetAllEmpenos")]
        public async Task<Respuesta> GetAllEmpeno(int institucionId)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Get all Encargos (orders)
                var empeños = await _db.Empeño.Where(e => (e.Anulado == false || e.Anulado == null) && e.PagoID == null && e.InstitucionID == institucionId).ToListAsync();

                // Check if any encargos were found
                if (empeños == null || empeños.Count == 0)
                {
                    res.Ok = false;
                    res.Message = "No empeños found.";
                }
                else
                {
                    res.Ok = true;
                    res.Message = "Empeños retrieved successfully.";
                    res.Data = empeños; // Return the list of Encargos
                }
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }
    }
}
