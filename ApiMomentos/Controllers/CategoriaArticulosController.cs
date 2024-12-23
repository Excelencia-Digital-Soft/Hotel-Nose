using ApiObjetos.Data;
using ApiObjetos.Models;
using ApiObjetos.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiObjetos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaArticulosController : ControllerBase
    {
        private readonly HotelDbContext _db;

        public CategoriaArticulosController(HotelDbContext context)
        {
            _db = context;
        }

        [HttpPost]
        [Route("CrearCategoria")]
        public async Task<Respuesta> CrearCategoria([FromBody] CategoriasArticulos categoria)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Check if the category name is already taken
                if (await _db.CategoriasArticulos.AnyAsync(c => c.NombreCategoria == categoria.NombreCategoria))
                {
                    res.Ok = false;
                    res.Message = "Ya existe una categoría con este nombre.";
                    return res;
                }

                // Step 2: Add the new category to the database
                _db.CategoriasArticulos.Add(categoria);
                await _db.SaveChangesAsync();

                // Step 3: Return success response with the created category
                res.Ok = true;
                res.Message = "Categoría creada correctamente.";
                res.Data = categoria;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpPut]
        [Route("ActualizarCategoria")]
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


        [HttpDelete]
        [Route("AnularCategoria")]
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
    }
}