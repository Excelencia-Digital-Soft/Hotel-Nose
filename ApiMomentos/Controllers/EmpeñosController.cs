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
    public class EncargoController : ControllerBase
    {
        private readonly HotelDbContext _db;

        public EncargoController(HotelDbContext context)
        {
            _db = context;
        }

        // POST: api/Encargo/AddEncargo
        [HttpPost]
        [Route("AddEncargo")]
        public async Task<Respuesta> AddEncargo(int visitaID, string detalle, float monto)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Create a new Encargo entity
                var newEncargo = new Empeño
                {
                    VisitaID = visitaID,
                    Detalle = detalle,
                    Monto = monto,
                    FechaRegistro = DateTime.Now, // You can add a registration date if needed
                    Anulado = false // Default to not annulled
                };

                // Add the Encargo entity to the DbContext
                await _db.Empeños.AddAsync(newEncargo);
                await _db.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = "Encargo added successfully.";
                res.Data = newEncargo; // Optionally return the added encargo or any relevant info
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        // PUT: api/Encargo/UpdateEncargo
        [HttpPut]
        [Route("UpdateEncargo")]
        public async Task<Respuesta> UpdateEncargo(int encargoID, string detalle, float monto)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Find the Encargo by EncargoID
                var encargo = await _db.Empeños.FindAsync(encargoID);
                if (encargo == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el encargo con ID: {encargoID}.";
                    return res;
                }

                // Update the details and amount
                encargo.Detalle = detalle;
                encargo.Monto = monto;

                // Save changes to the database
                await _db.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = "Encargo updated successfully.";
                res.Data = encargo; // Optionally return the updated encargo
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        // GET: api/Encargo/GetEncargo
        [HttpGet]
        [Route("GetEncargo")]
        public async Task<Respuesta> GetEncargo(int encargoID)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Find the Encargo by EncargoID
                var encargo = await _db.Empeños.FindAsync(encargoID);
                if (encargo == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el encargo con ID: {encargoID}.";
                    return res;
                }

                // Return the encargo found
                res.Ok = true;
                res.Message = "Encargo found.";
                res.Data = encargo; // Return the encargo
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        // DELETE: api/Encargo/DeleteEncargo
        [HttpDelete]
        [Route("DeleteEncargo")]
        public async Task<Respuesta> DeleteEncargo(int encargoID)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Find the Encargo by EncargoID
                var encargo = await _db.Empeños.FindAsync(encargoID);
                if (encargo == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el encargo con ID: {encargoID}.";
                    return res;
                }

                // Mark the Encargo as annulled (soft delete)
                encargo.Anulado = true;

                // Save changes to the database
                await _db.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = "Encargo deleted (annulled) successfully.";
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        // GET: api/Encargo/GetAllEncargos
        [HttpGet]
        [Route("GetAllEncargos")]
        public async Task<Respuesta> GetAllEncargos()
        {
            Respuesta res = new Respuesta();

            try
            {
                // Get all Encargos (orders)
                var encargos = await _db.Empeños.Where(e => !e.Anulado).ToListAsync();

                // Check if any encargos were found
                if (encargos == null || encargos.Count == 0)
                {
                    res.Ok = false;
                    res.Message = "No encargos found.";
                }
                else
                {
                    res.Ok = true;
                    res.Message = "Encargos retrieved successfully.";
                    res.Data = encargos; // Return the list of Encargos
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
