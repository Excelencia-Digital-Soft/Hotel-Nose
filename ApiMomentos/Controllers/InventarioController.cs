using ApiObjetos.Data;
using ApiObjetos.Models;
using ApiObjetos.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ApiObjetos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public InventarioController(ApplicationDbContext context)
        {
            _db = context;
        }

        // POST: api/Inventario
        [HttpPost]
        [Route("AddInventario")]
        public async Task<Respuesta> AddInventario(int Cantidad, int ArticuloID, int HabitacionID)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Check if the Articulo exists
                var articulo = await _db.Articulos.FindAsync(ArticuloID);
                if (articulo == null)
                {
                    res.Message = $"Articulo with ID {ArticuloID} not found.";
                    res.Ok = false;
                    return res;
                }

                // Validate HabitacionId
                var habitacion = await _db.Habitaciones.FindAsync(HabitacionID);
                if (habitacion == null)
                {
                    res.Message = $"Habitacion with ID {HabitacionID} not found.";
                    res.Ok = false;
                    return res;
                }

                // Check if an Inventario entry already exists for the given ArticuloID and HabitacionID
                var existingInventario = await _db.Inventarios
                    .FirstOrDefaultAsync(i => i.ArticuloId == ArticuloID && i.HabitacionId == HabitacionID);

                if (existingInventario != null)
                {
                    res.Message = "An existing Inventario entry found. Please use UpdateStock method to update the stock.";
                    res.Ok = false;
                    return res;
                }

                // Create new Inventario entity
                var newInventario = new Inventario
                {
                    ArticuloId = ArticuloID,
                    HabitacionId = HabitacionID,
                    Cantidad = Cantidad,
                    FechaRegistro = DateTime.Now,
                    Anulado = false // Default to not annulled
                };

                // Add the Inventario entity to the DbContext
                await _db.Inventarios.AddAsync(newInventario);
                await _db.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = "Inventario added successfully.";
                res.Data = newInventario; // You can return the added inventario or any relevant info
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpPut]
        [Route("UpdateStock")]
        public async Task<Respuesta> UpdateStock(int cantidad, int inventarioId)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Find the Inventario entry by InventarioID
                var inventario = await _db.Inventarios.FindAsync(inventarioId);
                if (inventario == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el inventario con ID: {inventarioId}.";
                    return res;
                }

                // Step 2: Update the stock quantity
                inventario.Cantidad = cantidad;

                // Step 3: Save changes to the database
                await _db.SaveChangesAsync();

                // Step 4: Set response on success
                res.Ok = true;
                res.Message = "Stock actualizado correctamente.";
                res.Data = inventario; // Optionally return the updated inventario object
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }
        [HttpGet]
        [Route("GetInventario")]
        public async Task<Respuesta> GetInventario(int habitacionID)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Retrieve all Inventarios for the specified habitacionID
                var inventarios = await _db.Inventarios
                    .Where(i => i.HabitacionId == habitacionID)
                    .Include(i => i.Articulo) // Include related Articulo data
                    .ToListAsync();

                // Step 2: Check if any inventarios were found
                if (inventarios == null || inventarios.Count == 0)
                {
                    res.Ok = false;
                    res.Message = "No se encontraron productos en la habitación.";
                }
                else
                {
                    res.Ok = true;
                    res.Message = "Productos obtenidos correctamente.";
                    res.Data = inventarios; // Return the list of Inventarios
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