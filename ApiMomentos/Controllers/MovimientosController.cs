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
        public async Task<Respuesta> ConsumirArticulos([FromBody] List<ArticuloConsumoDTO> articulos, int habitacionId, int visitaId)
        {
            Respuesta res = new Respuesta();

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    decimal? totalFacturado = 0;
                    List<Consumo> consumosToAdd = new List<Consumo>();
                    Movimientos nuevoMovimiento = new Movimientos
                    {
                        HabitacionId = habitacionId,
                        VisitaId = visitaId,
                        FechaRegistro = DateTime.Now,
                        Anulado = false
                    };

                    _db.Movimientos.Add(nuevoMovimiento);
                    await _db.SaveChangesAsync(); // Save Movimiento first to generate MovimientosId

                    // Step 1: Process each Articulo in the list
                    foreach (var articuloDTO in articulos)
                    {
                        // Step 2: Retrieve the Articulo to get the price
                        var articulo = await _db.Articulos.FindAsync(articuloDTO.ArticuloId);
                        if (articulo == null || articulo.Precio == null || articulo.Precio == 0 || articuloDTO.Cantidad == 0)
                        {
                            res.Ok = false;
                            res.Message = $"Error with the price or existence of the article ID: {articuloDTO.ArticuloId}";
                            return res;
                        }

                        int remainingQuantity = articuloDTO.Cantidad;

                        // Step 3: Retrieve and attempt to consume from the Habitacion inventory
                        var inventarioHabitacion = await _db.Inventarios
                            .FirstOrDefaultAsync(i => i.ArticuloId == articuloDTO.ArticuloId && i.HabitacionId == habitacionId);

                        if (inventarioHabitacion != null && inventarioHabitacion.Cantidad > 0)
                        {
                            int quantityToDeduct = Math.Min(inventarioHabitacion.Cantidad ?? 0, remainingQuantity);
                            inventarioHabitacion.Cantidad -= quantityToDeduct;
                            remainingQuantity -= quantityToDeduct;
                            _db.Inventarios.Update(inventarioHabitacion);
                        }

                        // Step 4: If still more is needed, try deducting from InventarioGeneral
                        if (remainingQuantity > 0)
                        {
                            var inventarioGeneral = await _db.InventarioGeneral
                                .FirstOrDefaultAsync(i => i.ArticuloId == articuloDTO.ArticuloId);

                            if (inventarioGeneral == null || inventarioGeneral.Cantidad < remainingQuantity)
                            {
                                res.Ok = false;
                                res.Message = $"Insufficient product stock in general inventory.";
                                return res;
                            }

                            // Deduct remaining quantity from InventarioGeneral
                            inventarioGeneral.Cantidad -= remainingQuantity;
                            _db.InventarioGeneral.Update(inventarioGeneral);
                        }

                        // Step 5: Calculate total for this articulo (price * quantity)
                        decimal totalArticulo = articulo.Precio * articuloDTO.Cantidad;
                        totalFacturado += totalArticulo;

                        // Step 6: Create a new Consumo for each Articulo and add it to the list
                        Consumo nuevoConsumo = new Consumo
                        {
                            ArticuloId = articulo.ArticuloId,
                            Cantidad = articuloDTO.Cantidad,
                            PrecioUnitario = articulo.Precio,
                            MovimientosId = nuevoMovimiento.MovimientosId,
                            Anulado = false
                        };

                        consumosToAdd.Add(nuevoConsumo);
                    }

                    // Step 7: Save all Consumos in one go
                    _db.Consumo.AddRange(consumosToAdd);

                    // Step 8: Update Movimiento with the total facturado for all items
                    nuevoMovimiento.TotalFacturado = totalFacturado;
                    _db.Movimientos.Update(nuevoMovimiento);

                    // Step 9: Save all changes to the database
                    await _db.SaveChangesAsync();

                    // Step 10: Load related data (Habitacion and Visita) to return in the response
                    await _db.Entry(nuevoMovimiento).Reference(m => m.Habitacion).LoadAsync();
                    await _db.Entry(nuevoMovimiento).Reference(m => m.Visita).LoadAsync();

                    // Step 11: Commit the transaction
                    await transaction.CommitAsync();

                    // Set response on success
                    res.Ok = true;
                    res.Message = "Consumos created and stock updated successfully.";
                    res.Data = nuevoMovimiento; // Return the created movimiento with Habitacion and Visita info
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    await transaction.RollbackAsync();
                    res.Message = $"Error: {ex.Message} {ex.InnerException}";
                    res.Ok = false;
                }
            }

            return res;
        }


        [HttpGet]
        [Route("GetConsumosVisita")]
        [AllowAnonymous]
        public async Task<Respuesta> GetConsumosVisita(int VisitaID)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Get the Movimientos associated with the given VisitaID
                var movimientos = await _db.Movimientos
                    .Where(t => t.VisitaId == VisitaID && t.Anulado == false)
                    .ToListAsync();

                // Check if there are any Movimientos found
                if (movimientos.Count > 0)
                {
                    // Extract the MovimientosIDs from the Movimientos
                    // Extract the MovimientosIDs from the Movimientos
                    var movimientoIds = movimientos
                        .Select(m => m.MovimientosId) // Select MovimientosID
                        .ToList(); // Create a list of IDs

                    // Get the Consumos associated with the found MovimientosIDs
                    var consumos = await _db.Consumo
                        .Where(c => movimientoIds.Contains(c.MovimientosId.GetValueOrDefault()) && c.Anulado == false)
                        .Join(_db.Articulos,
                              c => c.ArticuloId,
                              a => a.ArticuloId,
                              (c, a) => new
                              {
                                  c.ConsumoId,
                                  c.ArticuloId,
                                  ArticleName = a.NombreArticulo, // Assuming 'NombreArticulo' is the name column in Articulos table
                                  c.Cantidad,
                                  c.PrecioUnitario,
                                  Total = c.Cantidad * c.PrecioUnitario
                              })
                        .ToListAsync();

                    res.Ok = true;
                    res.Data = consumos; // Return the list of consumos
                }
                else
                {
                    res.Ok = false;
                    res.Message= "No se encontraron movimientos para esta visita.";
                }
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = "Error al obtener los consumos: " + ex.Message;
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
    public class ArticuloConsumoDTO
    {
        public int ArticuloId { get; set; }
        public int Cantidad { get; set; }
    }
}
