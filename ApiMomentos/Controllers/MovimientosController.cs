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


        [HttpPost]
        [Route("MovimientoHabitacion")]
        [AllowAnonymous]
        public async Task<int> CrearMovimientoHabitacion(int visitaId, decimal totalFacturado, int habitacionId)
        {
            try
            {
                Movimientos nuevoMovimiento = new Movimientos
                {
                    VisitaId = visitaId,
                    TotalFacturado = totalFacturado,
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
                        if (articulo == null)
                        {
                            res.Ok = false;
                            res.Message = $"Articulo with ID {articuloDTO.ArticuloId} not found.";
                            return res;
                        }
                        if (articulo.Precio == null || articulo.Precio == 0 || articuloDTO.Cantidad == 0)
                        {
                            res.Ok = false;
                            res.Message = $"Error con el precio del articulo";
                            return res;
                        }
                        // Step 3: Retrieve the Inventario for the specific Articulo and Habitacion
                        var inventario = await _db.Inventarios
                            .FirstOrDefaultAsync(i => i.ArticuloId == articuloDTO.ArticuloId && i.HabitacionId == habitacionId);

                        if (inventario == null || inventario.Cantidad < articuloDTO.Cantidad)
                        {
                                res.Ok = false;
                                res.Message = $"No hay suficiente producto";
                                return res;

                        }
                        else
                        {
                            // Step 5: Deduct the quantity from the Inventario
                            inventario.Cantidad -= articuloDTO.Cantidad;
                            if (inventario.Cantidad < 0)
                            {
                                res.Ok = false;
                                res.Message = $"No hay suficiente producto en la habitación";
                                return res;
                            }
                            _db.Inventarios.Update(inventario);
                        }

                        // Step 6: Calculate total for this articulo (price * quantity)
                        decimal totalArticulo = articulo.Precio * articuloDTO.Cantidad;
                        totalFacturado += totalArticulo;

                        // Step 7: Create a new Consumo for each Articulo and add it to the list
                        Consumo nuevoConsumo = new Consumo
                        {
                            ArticuloId = articulo.ArticuloId,
                            Cantidad = articuloDTO.Cantidad,
                            PrecioUnitario = articulo.Precio,
                            MovimientosId = nuevoMovimiento.MovimientosId,
                            Anulado = false,
                            EsHabitacion = true
                        };

                        consumosToAdd.Add(nuevoConsumo);
                    }

                    // Step 8: Save all Consumos in one go
                    _db.Consumo.AddRange(consumosToAdd);

                    // Step 9: Update Movimiento with the total facturado for all items
                    nuevoMovimiento.TotalFacturado = totalFacturado;
                    _db.Movimientos.Update(nuevoMovimiento);

                    // Step 10: Save all changes to the database
                    await _db.SaveChangesAsync();

                    // Step 11: Load the related data (Habitacion and Visita) to return in the response
                    await _db.Entry(nuevoMovimiento).Reference(m => m.Habitacion).LoadAsync();
                    await _db.Entry(nuevoMovimiento).Reference(m => m.Visita).LoadAsync();

                    // Step 12: Commit the transaction
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
                    res.Message = $"Error: {ex.Message}";
                    res.Ok = false;
                }
            }

            return res;
        }

        [HttpPost]
        [Route("ConsumoGeneral")]
        [AllowAnonymous]
        public async Task<Respuesta> ConsumirArticulosGeneral([FromBody] List<ArticuloConsumoDTO> articulos, int habitacionId, int visitaId)
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
                        if (articulo == null)
                        {
                            res.Ok = false;
                            res.Message = $"Articulo with ID {articuloDTO.ArticuloId} not found.";
                            return res;
                        }
                        if (articulo.Precio == null || articulo.Precio == 0 || articuloDTO.Cantidad == 0)
                        {
                            res.Ok = false;
                            res.Message = $"Error con el precio del articulo";
                            return res;
                        }
                        // Step 3: Retrieve the Inventario for the specific Articulo and Habitacion
                        var inventario = await _db.InventarioGeneral
                            .FirstOrDefaultAsync(i => i.ArticuloId == articuloDTO.ArticuloId);

                        if (inventario == null || inventario.Cantidad < articuloDTO.Cantidad)
                        {
                            if (inventario.Cantidad < 0)
                            {
                                res.Ok = false;
                                res.Message = $"No hay suficiente producto";
                                return res;
                            }

                        }
                        else
                        {
                            // Step 5: Deduct the quantity from the Inventario
                            inventario.Cantidad -= articuloDTO.Cantidad;
                            if (inventario.Cantidad < 0)
                            {
                                res.Ok = false;
                                res.Message = $"No hay suficiente producto en el inventario general";
                                return res;
                            }
                            _db.InventarioGeneral.Update(inventario);
                        }

                        // Step 6: Calculate total for this articulo (price * quantity)
                        decimal totalArticulo = articulo.Precio * articuloDTO.Cantidad;
                        totalFacturado += totalArticulo;

                        // Step 7: Create a new Consumo for each Articulo and add it to the list
                        Consumo nuevoConsumo = new Consumo
                        {
                            ArticuloId = articulo.ArticuloId,
                            Cantidad = articuloDTO.Cantidad,
                            PrecioUnitario = articulo.Precio,
                            MovimientosId = nuevoMovimiento.MovimientosId,
                            Anulado = false,
                            EsHabitacion = false
                        };

                        consumosToAdd.Add(nuevoConsumo);
                    }

                    // Step 8: Save all Consumos in one go
                    _db.Consumo.AddRange(consumosToAdd);

                    // Step 9: Update Movimiento with the total facturado for all items
                    nuevoMovimiento.TotalFacturado = totalFacturado;
                    _db.Movimientos.Update(nuevoMovimiento);

                    // Step 10: Save all changes to the database
                    await _db.SaveChangesAsync();

                    // Step 11: Load the related data (Habitacion and Visita) to return in the response
                    await _db.Entry(nuevoMovimiento).Reference(m => m.Habitacion).LoadAsync();
                    await _db.Entry(nuevoMovimiento).Reference(m => m.Visita).LoadAsync();

                    // Step 12: Commit the transaction
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
                                  c.EsHabitacion,
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

        public async Task<Respuesta> GetMovimientos(int institucionID)
        {
            Respuesta res = new Respuesta();
            try
            {

                var Objeto = await _db.Movimientos.Where(m => m.InstitucionID == institucionID).ToListAsync();
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

        [HttpGet]
        [Route("GetEgresosSegunTipo")]
        [AllowAnonymous]
        public async Task<Respuesta> GetEgresosSegunTipo(int institucionID, int tipoEgresoID)
        {
            Respuesta res = new Respuesta();
            try
            {
                var egresos = await _db.Egresos
                    .Where(e => e.TipoEgresoId == tipoEgresoID && e.InstitucionID == institucionID)
                    .ToListAsync();

                res.Ok = true;
                res.Data = egresos;
            }
            catch (Exception ex)
            {
                res.Message = ex.ToString();
                res.Ok = false;
            }
            return res;
        }

        // Get a single Egreso by ID
        [HttpGet]
        [Route("GetEgreso")]
        [AllowAnonymous]
        public async Task<Respuesta> GetEgreso(int id)
        {
            Respuesta res = new Respuesta();
            try
            {
                var egreso = await _db.Egresos.FindAsync(id);
                if (egreso != null)
                {
                    res.Ok = true;
                    res.Data = egreso;
                }
                else
                {
                    res.Ok = false;
                    res.Message = "Egreso not found.";
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.ToString();
                res.Ok = false;
            }
            return res;
        }

        // Get a single Egreso by ID
        [HttpGet]
        [Route("GetEgresoMovimiento")]
        [AllowAnonymous]
        public async Task<Respuesta> GetEgresoMovimiento(int idMovimiento)
        {
            Respuesta res = new Respuesta();
            try
            {
                var egreso = await _db.Egresos.Where(e => e.Movimiento.MovimientosId == idMovimiento).FirstOrDefaultAsync();
                if (egreso != null)
                {
                    res.Ok = true;
                    res.Data = egreso;
                }
                else
                {
                    res.Ok = false;
                    res.Message = "Egreso not found.";
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.ToString();
                res.Ok = false;
            }
            return res;
        }


        // Get all TipoEgresos
        [HttpGet]
        [Route("GetTipoEgresos")]
        [AllowAnonymous]
        public async Task<Respuesta> GetTipoEgresos(int institucionID)
        {
            Respuesta res = new Respuesta();
            try
            {
                var tipoEgresos = await _db.TipoEgreso.Where(t => t.InstitucionID == institucionID).ToListAsync();
                res.Ok = true;
                res.Data = tipoEgresos;
            }
            catch (Exception ex)
            {
                res.Message = ex.ToString();
                res.Ok = false;
            }
            return res;
        }

        // Create a new Egreso
        [HttpPost]
        [Route("CreateEgreso")]
        [AllowAnonymous]
        public async Task<Respuesta> CreateEgreso([FromBody] Egresos newEgreso)
        {
            Respuesta res = new Respuesta();
            try
            {
                if (newEgreso == null || newEgreso.TipoEgresoId == null)
                {
                    res.Ok = false;
                    res.Message = "Invalid Egreso data.";
                    return res;
                }

                // Calculate the total facturado as a negative amount for an egreso
                decimal totalFacturado = -Math.Abs(newEgreso.Precio * newEgreso.Cantidad); // Ensure it's negative

                // Create the new Movimiento
                Movimientos nuevoMovimiento = new Movimientos
                {
                    TotalFacturado = totalFacturado,
                    FechaRegistro = DateTime.Now // Assuming you have a Fecha field
                };

                // Add the Movimiento to the database
                newEgreso.MovimientoId = nuevoMovimiento.MovimientosId;
                _db.Movimientos.Add(nuevoMovimiento);
                await _db.SaveChangesAsync();

                // Link the Movimiento to the Egreso via MovimientoId
                newEgreso.MovimientoId = nuevoMovimiento.MovimientosId;
                _db.Egresos.Add(newEgreso);
                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Data = newEgreso;
            }
            catch (Exception ex)
            {
                res.Message = ex.ToString();
                res.Ok = false;
            }
            return res;
        }

        [HttpPost]
        [Route("CreateTipoEgreso")]
        [AllowAnonymous]
        public async Task<Respuesta> CreateTipoEgreso([FromBody] TipoEgreso newTipoEgreso)
        {
            Respuesta res = new Respuesta();
            try
            {
                if (newTipoEgreso == null || newTipoEgreso.Nombre == null)
                {
                    res.Ok = false;
                    res.Message = "Invalid Egreso data.";
                    return res;
                }

                _db.TipoEgreso.Add(newTipoEgreso);
                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Data = newTipoEgreso;
            }
            catch (Exception ex)
            {
                res.Message = ex.ToString();
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
