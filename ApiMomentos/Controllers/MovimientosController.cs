using ApiObjetos.Data;
using ApiObjetos.Models.Sistema;
using ApiObjetos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiObjetos.Controllers
{
    public class MovimientosController
    {
        private readonly HotelDbContext _db;

        public MovimientosController(HotelDbContext db)
        {
            _db = db;
        }


        public async Task<int> CrearMovimientoHabitacion(int visitaId, int totalFacturado, int habitacionId, Habitaciones habitacion)
        {
            try
            {
                Movimientos nuevoMovimiento = new Movimientos
                {
                    VisitaId = visitaId,
                    TotalFacturado = totalFacturado,
                    Habitacion = habitacion,
                    HabitacionId = habitacionId,
                };

                _db.Add(nuevoMovimiento);

                await _db.SaveChangesAsync();

                return nuevoMovimiento.MovimientosId;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        [HttpPost]
        [Route("ConsumoHabitacion")]
        [AllowAnonymous]
        public async Task<Respuesta> ConsumirArticulo(int articuloId, int cantidad, int habitacionId, int visitaId)
        {
            Respuesta res = new Respuesta();

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Step 1: Retrieve the Articulo to get the price
                    var articulo = await _db.Articulos.FindAsync(articuloId);
                    if (articulo == null)
                    {
                        res.Ok = false;
                        res.Message = $"Articulo with ID {articuloId} not found.";
                        return res;
                    }

                    // Step 2: Retrieve the Inventario for the specific Articulo and Habitacion
                    var inventario = await _db.Inventarios
                        .FirstOrDefaultAsync(i => i.ArticuloId == articuloId && i.HabitacionId == habitacionId);

                    if (inventario == null)
                    {
                        res.Ok = false;
                        res.Message = $"No inventario found for Articulo ID {articuloId} in Habitacion ID {habitacionId}.";
                        return res;
                    }

                    // Step 3: Ensure enough stock exists
                    if (inventario.Cantidad < cantidad)
                    {
                        res.Ok = false;
                        res.Message = $"Not enough stock for Articulo ID {articuloId}. Available stock: {inventario.Cantidad}.";
                        return res;
                    }

                    // Step 4: Deduct the quantity from the Inventario
                    inventario.Cantidad -= cantidad;
                    _db.Inventarios.Update(inventario);

                    // Step 5: Create a new Movimiento, link it to Habitacion and Visita
                    Movimientos nuevoMovimiento = new Movimientos
                    {
                        HabitacionId = habitacionId,
                        VisitaId = visitaId,  // Associate with Visita
                        TotalFacturado = articulo.Precio * cantidad, // Calculate total cost based on the price and quantity
                        FechaRegistro = DateTime.Now,
                        Anulado = false
                    };
                    _db.Movimientos.Add(nuevoMovimiento);
                    await _db.SaveChangesAsync();  // Save Movimiento first to generate MovimientosId

                    // Step 6: Create a new Consumo and link it to the saved Movimiento
                    Consumo nuevoConsumo = new Consumo
                    {
                        ArticuloId = articulo.ArticuloId,
                        Cantidad = cantidad,
                        PrecioUnitario = articulo.Precio,
                        MovimientosId = nuevoMovimiento.MovimientosId, // Associate with the saved Movimiento
                        Anulado = false // By default, it's not annulled
                    };
                    _db.Consumo.Add(nuevoConsumo);

                    // Step 7: Save all changes to the database
                    await _db.SaveChangesAsync();

                    // Step 8: Load the related data (Habitacion and Visita) to return in the response
                    await _db.Entry(nuevoMovimiento).Reference(m => m.Habitacion).LoadAsync();
                    await _db.Entry(nuevoMovimiento).Reference(m => m.Visita).LoadAsync();

                    // Step 9: Commit the transaction
                    await transaction.CommitAsync();

                    // Set response on success
                    res.Ok = true;
                    res.Message = "Consumo created and stock updated successfully.";
                    res.Data = nuevoMovimiento; // Return the created movimiento with Habitacion and Visita info
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    await transaction.RollbackAsync();
                    res.Message = $"Error: {ex.Message}";
                    res.Ok = false;
                }
            }

            return res;
        }

        [HttpGet]
        [Route("GetMovimiento")] // Obtiene un paciente basado en su idPaciente. Se obtiene la lista de los idPaciente con el metodo GetPacientes
        [AllowAnonymous]

        public async Task<Respuesta> GetMovimiento(int id)
        {
            Respuesta res = new Respuesta();
            try
            {

                var Objeto = await _db.Movimientos.Where(
                t => t.MovimientosId == id
                ).ToListAsync();
                res.Ok = true;
                res.Data = Objeto[0];
                return res;



            }
            catch (Exception ex)
            {
                res.Message = ex.ToString();
                res.Ok = false;
            }
            return res;
        }

        [HttpGet]
        [Route("GetMovimientos")] // Obtiene un paciente basado en su idPaciente. Se obtiene la lista de los idPaciente con el metodo GetPacientes
        [AllowAnonymous]

        public async Task<Respuesta> GetMovimientos()
        {
            Respuesta res = new Respuesta();
            try
            {

                var Objeto = await _db.Movimientos.ToListAsync();
                res.Ok = true;
                res.Data = Objeto;
                return res;



            }
            catch (Exception ex)
            {
                res.Message = ex.ToString();
                res.Ok = false;
            }
            return res;
        }


        [HttpDelete]
        [Route("AnularMovimiento")] // Encuentra el ID del paciente para luego eliminarlo
        [AllowAnonymous]
        public async Task<Respuesta> AnularMovimiento(int id, bool Estado)
        {
            Respuesta res = new Respuesta();
            try
            {
                var movimiento = await _db.Movimientos.FindAsync(id);

                if (movimiento == null)
                {
                    res.Ok = false;
                    res.Message = $"El movimiento con el id {id} no se encontró.";
                }
                else
                {
                    movimiento.Anulado = Estado;
                    await _db.SaveChangesAsync();

                    res.Ok = true;
                    res.Message = $"Se cambió el estado correctamente";
                }
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Ocurrió un error: {ex.Message}";
            }

            return res;
        }

        [HttpGet]
        [Route("GetMovimientosVisita")] // Obtiene un paciente basado en su idPaciente. Se obtiene la l
        [AllowAnonymous]

        public async Task<Respuesta> GetMovimientosVisita(int id)
        {
            Respuesta res = new Respuesta();
            try
            {

                var Objeto = await _db.Movimientos.Where(
                t => t.VisitaId == id
                ).ToListAsync(); res.Ok = true;
                res.Data = Objeto;
                return res;



            }
            catch (Exception ex)
            {
                res.Message = ex.ToString();
                res.Ok = false;
            }
            return res;
        }
        [HttpGet]
        [Route("GetTotalVisita")] // Obtiene el total facturado para una visita
        [AllowAnonymous]
        public async Task<Respuesta> GetTotalVisita(int id)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Step 1: Retrieve the list of Movimientos for the given VisitaId
                var movimientos = await _db.Movimientos
                                           .Where(t => t.VisitaId == id)
                                           .ToListAsync();

                // Step 2: Calculate the total sum of TotalFacturado
                var totalFacturado = movimientos.Sum(m => m.TotalFacturado);

                // Step 3: Return the result
                res.Ok = true;
                res.Data = totalFacturado; // return the total sum
                res.Message = "Total facturado calculado correctamente.";
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
