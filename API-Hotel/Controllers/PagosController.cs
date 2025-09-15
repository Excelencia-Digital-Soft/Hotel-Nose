﻿using hotel.Models;
using hotel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hotel.Models.Sistema;
using hotel.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace hotel.Controllers
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
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Obsolete("This endpoint is deprecated. Consider using /api/v1/pagos instead.")]
        public async Task<Respuesta> PagarVisita(int visitaId, decimal montoDescuento, decimal montoEfectivo, decimal montoTarjeta, decimal montoBillVirt, decimal adicional, int medioPagoId, string? comentario, decimal? montoRecargo, string? descripcionRecargo, int? tarjetaID)
        {
            Respuesta res = new Respuesta();
            
            // Use transaction for data consistency
            using var transaction = await _db.Database.BeginTransactionAsync();
            
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

                // Step 4: Calculate the total amount to be paid and validate payment amounts
                decimal totalFacturado = movimientos.Sum(m => m.TotalFacturado ?? 0);
                decimal totalPagado = montoEfectivo + montoTarjeta + montoBillVirt + adicional - montoDescuento + (montoRecargo ?? 0);
                
                // Validate that payment amounts are not negative
                if (montoEfectivo < 0 || montoTarjeta < 0 || montoBillVirt < 0)
                {
                    res.Ok = false;
                    res.Message = "Los montos de pago no pueden ser negativos.";
                    return res;
                }
                
                string observacion = string.IsNullOrWhiteSpace(comentario) ? "-" : comentario;

                // Step 5: Create a new Pago for the Visita with the respective payment methods and UserId
                // Get authenticated user ID
                var userId = this.GetCurrentUserId();
                
                Pagos nuevoPago = new Pagos
                {
                    MontoDescuento = montoDescuento,
                    MontoEfectivo = montoEfectivo,
                    InstitucionID = visita.InstitucionID,
                    MontoTarjeta = montoTarjeta,
                    MontoBillVirt = montoBillVirt,
                    TarjetaId = tarjetaID,
                    Adicional = adicional,
                    MedioPagoId = medioPagoId,
                    fechaHora = DateTime.Now,
                    Observacion = observacion,
                    UserId = userId // Track which user processed this payment
                };
                
                _db.Pagos.Add(nuevoPago);
                await _db.SaveChangesAsync();

                // Add surcharge if provided
                if (montoRecargo.HasValue && montoRecargo.Value > 0)
                {
                    Recargos nuevoRecargo = new Recargos
                    {
                        Valor = montoRecargo,
                        Descripcion = descripcionRecargo ?? "Recargo aplicado",
                        PagoID = nuevoPago.PagoId,
                    };
                    _db.Recargos.Add(nuevoRecargo);
                    await _db.SaveChangesAsync();
                }

                // Step 6: Update all Movimientos related to the Visita to include the PagoId
                foreach (var movimiento in movimientos)
                {
                    movimiento.PagoId = nuevoPago.PagoId;
                }
                
                _db.UpdateRange(movimientos);
                await _db.SaveChangesAsync();
                
                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                res.Ok = true;
                res.Message = "El pago de la visita se realizó correctamente.";
                res.Data = nuevoPago;
            }
            catch (DbUpdateException dbEx)
            {
                // Rollback transaction on database error
                await transaction.RollbackAsync();
                res.Ok = false;
                res.Message = $"Error al actualizar la base de datos: {dbEx.Message}";
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();
                res.Ok = false;
                res.Message = $"Error al procesar el pago: {ex.Message}";
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
        public async Task<Respuesta> GetPagos(int institucionID)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Get all Pagos from the database, including related MedioPago and Movimientos
                var pagos = await _db.Pagos
                    .Where(p => p.InstitucionID == institucionID)
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
