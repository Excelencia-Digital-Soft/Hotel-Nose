using ApiObjetos.Data;
using ApiObjetos.Models;
using ApiObjetos.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ApiObjetos.Controllers
{
    public class InventarioGeneralController
    {
        private readonly HotelDbContext _db;

        public InventarioGeneralController(HotelDbContext context)
        {
            _db = context;
        }

        [HttpPut]
        [Route("UpdateStockGeneral")]
        public async Task<Respuesta> UpdateStockGeneral([FromBody] List<UpdateStockRequest> updates)
        {
            Respuesta res = new Respuesta();
            List<object> updateResults = new List<object>();

            try
            {
                // Step 1: Iterate through each update in the request
                foreach (var update in updates)
                {
                    var inventario = await _db.InventarioGeneral.FindAsync(update.InventarioId);
                    if (inventario != null)
                    {
                        // Update the stock quantity for each entry
                        inventario.Cantidad = update.Cantidad;
                        updateResults.Add(new { update.InventarioId, Success = true });
                    }
                    else
                    {
                        // Add a failed update result if the item was not found
                        updateResults.Add(new { update.InventarioId, Success = false, Message = "Item not found" });
                    }
                }

                // Save all changes to the database once after processing all updates
                await _db.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = "Stock actualizado para los elementos procesados.";
                res.Data = updateResults; // Optionally return the status of each update
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpGet]
        [Route("GetInventarioConHabitacion")]
        public async Task<Respuesta> GetInventarioConHabitacion(int idHabitacion)
        {
            Respuesta res = new Respuesta();

            try
            {
                InventarioDTO inventario = new InventarioDTO();
                // Step 1: Retrieve all Inventarios for the specified habitacionID
                var inventarioGeneral = await _db.InventarioGeneral
                    .Where(i => i.Cantidad > 0)
                    .Include(i => i.Articulo) // Include related Articulo data
                    .ToListAsync();
                var inventarioHabitacion = await _db.Inventarios
                    .Where(i => i.HabitacionId == idHabitacion)
                    .Include(i => i.Articulo)
                    .ToListAsync();
                var combinedInventarioDict = inventarioGeneral
                      .ToDictionary(i => i.Articulo.ArticuloId, i => new InventarioDTO
                      {
                          InventarioId = i.InventarioId,
                          ArticuloId = i.Articulo.ArticuloId,
                          Cantidad = i.Cantidad
                      });

                // Update quantities from inventarioHabitacion into combinedInventarioDict
                foreach (var habitacionItem in inventarioHabitacion)
                {
                    if (habitacionItem.Articulo != null)
                    {
                        var articuloId = habitacionItem.Articulo.ArticuloId;

                        if (combinedInventarioDict.ContainsKey(articuloId))
                        {
                            // Add the quantity if the item exists in inventarioGeneral
                            combinedInventarioDict[articuloId].Cantidad += habitacionItem.Cantidad;
                        }
                        else
                        {
                            // Add new entry if not present in inventarioGeneral
                            combinedInventarioDict[articuloId] = new InventarioDTO
                            {
                                InventarioId = habitacionItem.InventarioId,
                                ArticuloId = articuloId,
                                Cantidad = habitacionItem.Cantidad
                            };
                        }
                    }
                }
                    // Step 2: Check if any inventarios were found
                    if (combinedInventarioDict == null)
                {
                    res.Ok = false;
                    res.Message = "No se encontraron productos en el inventario.";
                }
                else
                {
                    res.Ok = true;
                    res.Message = "Productos obtenidos correctamente.";
                    res.Data = combinedInventarioDict; // Return the list of Inventarios
                }
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }


        [HttpGet]
        [Route("GetInventarioGeneral")]
        public async Task<Respuesta> GetInventarioGeneral(int institucionID)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Retrieve all Inventarios for the specified habitacionID
                var inventario = await _db.InventarioGeneral
                    .Where(i => i.InstitucionID == institucionID)
                    .Include(i => i.Articulo) // Include related Articulo data
                    .Where(i => i.Articulo != null && i.Articulo.Anulado == false)
                    .ToListAsync();

                // Step 2: Check if any inventarios were found
                if (inventario == null)
                {
                    res.Ok = false;
                    res.Message = "No se encontraron productos en el inventario.";
                }
                else
                {
                    res.Ok = true;
                    res.Message = "Productos obtenidos correctamente.";
                    res.Data = inventario; // Return the list of Inventarios
                }
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpGet]
        [Route("CoordinarInventarioGeneral")]
        public async Task<Respuesta> CoordinarInventarioGeneral(int institucionID)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Remove entries in `InventarioGeneral` where the associated `Articulo` has `Anulado = 1`
                var anuladoInventarios = _db.InventarioGeneral
                    .Where(i => i.InstitucionID == institucionID)
                    .Include(i => i.Articulo)
                    .Where(i => i.Articulo != null && i.Articulo.Anulado == true);

                _db.InventarioGeneral.RemoveRange(anuladoInventarios);

                // Step 2: Identify `Articulos` that are not in `InventarioGeneral`
                var existingArticuloIds = await _db.InventarioGeneral.Where(i => i.InstitucionID == institucionID).Select(i => i.ArticuloId).ToListAsync();
                var missingArticulos = await _db.Articulos
                    .Where(a => !existingArticuloIds.Contains(a.ArticuloId) && a.Anulado != true && a.InstitucionID == institucionID)
                    .ToListAsync();

                // Step 3: Add missing `Articulos` to `InventarioGeneral` with default values
                var newInventarios = missingArticulos.Select(a => new InventarioGeneral
                {
                    ArticuloId = a.ArticuloId,
                    Cantidad = 0, // Default quantity
                    FechaRegistro = DateTime.Now,
                    Anulado = false
                }).ToList();

                _db.InventarioGeneral.AddRange(newInventarios);

                // Save changes
                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Inventario General coordinado exitosamente.";
                res.Data = new
                {
                    RemovedItems = anuladoInventarios.Count(),
                    AddedItems = newInventarios.Count
                };
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error al coordinar el inventario general: {ex.Message}";
            }

            return res;
        }

    }

    public class UpdateStockRequest
    {
        public int InventarioId { get; set; }
        public int Cantidad { get; set; }
    }

    public class InventarioDTO
    {
        public int InventarioId { get; set; }

        public int? ArticuloId { get; set; }

        public int? Cantidad { get; set; }

    }
}