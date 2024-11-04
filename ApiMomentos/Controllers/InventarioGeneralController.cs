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
        [Route("GetInventarioGeneral")]
        public async Task<Respuesta> GetInventarioGeneral()
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Retrieve all Inventarios for the specified habitacionID
                var inventario = await _db.InventarioGeneral
                    .Include(i => i.Articulo) // Include related Articulo data
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

    }

    public class UpdateStockRequest
    {
        public int InventarioId { get; set; }
        public int Cantidad { get; set; }
    }
}