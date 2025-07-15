using hotel.Data;
using hotel.Models;
using hotel.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Obsolete("This controller is deprecated. Use /api/v1/categorias instead.")]
    public class CategoriaArticulosController : ControllerBase
    {
        private readonly HotelDbContext _db;

        public CategoriaArticulosController(HotelDbContext context)
        {
            _db = context;
        }

        [HttpPost]
        [Route("CrearCategoria")]
        [Obsolete("This endpoint is deprecated. Use POST /api/v1/categorias instead.")]
        public async Task<Respuesta> CrearCategoria([FromBody] CrearCategoriaDTO categoriaCreada)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Validate input
                if (string.IsNullOrWhiteSpace(categoriaCreada.NombreCategoria))
                {
                    res.Ok = false;
                    res.Message = "El nombre de la categoría no puede estar vacío.";
                    return res;
                }

                // Step 2: Check if the category name is already taken (case-insensitive)
                if (await _db.CategoriasArticulos
                    .AnyAsync(c => c.NombreCategoria.ToLower() == categoriaCreada.NombreCategoria.ToLower() && c.InstitucionID == categoriaCreada.InstitucionID))
                {
                    res.Ok = false;
                    res.Message = "Ya existe una categoría con este nombre.";
                    return res;
                }

                // Step 3: Add the new category to the database
                CategoriasArticulos categoria = new CategoriasArticulos
                {
                    NombreCategoria = categoriaCreada.NombreCategoria.Trim(),
                    InstitucionID = categoriaCreada.InstitucionID,
                    Anulado = false
                };
                _db.CategoriasArticulos.Add(categoria);
                await _db.SaveChangesAsync();

                // Step 4: Return success response with the created category
                res.Ok = true;
                res.Message = "Categoría creada correctamente.";
                res.Data = new
                {
                    categoria.CategoriaId,
                    categoria.InstitucionID,
                    categoria.NombreCategoria
                };
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                // _logger.LogError(ex, "Error creating category");

                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpPut]
        [Route("ActualizarCategoria")]
        [Obsolete("This endpoint is deprecated. Use PUT /api/v1/categorias/{id} instead.")]
        public async Task<Respuesta> ActualizarCategoria(int id, [FromBody] CategoriasArticulos categoria)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Retrieve the existing category by ID
                var existingCategoria = await _db.CategoriasArticulos.FindAsync(id);
                if (existingCategoria == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró la categoría con ID: {id}.";
                    return res;
                }

                // Step 2: Update the category properties
                existingCategoria.NombreCategoria = categoria.NombreCategoria ?? existingCategoria.NombreCategoria;
                existingCategoria.Anulado = categoria.Anulado ?? existingCategoria.Anulado;

                // Step 3: Save changes to the database
                _db.CategoriasArticulos.Update(existingCategoria);
                await _db.SaveChangesAsync();

                // Step 4: Return success response
                res.Ok = true;
                res.Message = "Categoría actualizada correctamente.";
                res.Data = existingCategoria;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpGet]
        [Route("GetCategorias")]
        [Obsolete("This endpoint is deprecated. Use GET /api/v1/categorias instead.")]
        public async Task<Respuesta> GetCategorias(int InstitucionID)
        {
            Respuesta res = new Respuesta();

            try
            {

                    var categorias = await _db.CategoriasArticulos.
                        Where(a => a.Anulado != true && a.InstitucionID == InstitucionID)
                        .
                        ToListAsync();
   
                // Step 2: Check if any articulos were found
                if (categorias == null || categorias.Count == 0)
                {
                    res.Ok = false;
                    res.Message = "No se encontraron categorias.";
                }
                else
                {
                    res.Ok = true;
                    res.Message = "Categorías obtenidos correctamente.";
                    res.Data = categorias; // Return the list of articulos
                }
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpDelete]
        [Route("AnularCategoria")]
        [Obsolete("This endpoint is deprecated. Use PATCH /api/v1/categorias/{id}/status instead.")]
        public async Task<Respuesta> AnularCategoria(int id, bool estado)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Retrieve the category by ID
                var categoria = await _db.CategoriasArticulos.FindAsync(id);
                if (categoria == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró la categoría con ID: {id}.";
                    return res;
                }

                // Step 2: Set the Anulado property based on the estado parameter
                categoria.Anulado = estado;

                // Step 3: Save changes to the database
                _db.CategoriasArticulos.Update(categoria);
                await _db.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = estado ? "Categoría anulada correctamente." : "Categoría desanulada correctamente.";
                res.Data = categoria;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        public class CrearCategoriaDTO
        {
            public string NombreCategoria { get; set; }
            public int InstitucionID { get; set; }
        }
    }
}