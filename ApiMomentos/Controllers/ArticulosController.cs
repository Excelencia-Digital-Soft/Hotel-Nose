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
    public class ArticulosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ArticulosController(ApplicationDbContext context)
        {
            _db = context;
        }

        // Method to get all articulos
        [HttpGet]
        [Route("GetArticulos")]
        public async Task<Respuesta> GetArticulos()
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Retrieve all articulos from the database
                var articulos = await _db.Articulos.ToListAsync();

                // Step 2: Check if any articulos were found
                if (articulos == null || articulos.Count == 0)
                {
                    res.Ok = false;
                    res.Message = "No se encontraron artículos.";
                }
                else
                {
                    res.Ok = true;
                    res.Message = "Artículos obtenidos correctamente.";
                    res.Data = articulos; // Return the list of articulos
                }
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        // Method to get a specific articulo by ID
        [HttpGet]
        [Route("GetArticulo/{id}")]
        public async Task<Respuesta> GetArticulo(int id)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Retrieve the articulo by ID
                var articulo = await _db.Articulos.FindAsync(id);
                if (articulo == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el artículo con ID: {id}.";
                }
                else
                {
                    res.Ok = true;
                    res.Message = "Artículo obtenido correctamente.";
                    res.Data = articulo; // Return the found articulo
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