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
        private readonly HotelDbContext _db;

        public ArticulosController(HotelDbContext context)
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
                var articulos = await _db.Articulos.
                    Where(a => a.Anulado != true)
                    .
                    ToListAsync();

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


        [HttpPut]   
        [Route("UpdateArticulo")]
        public async Task<Respuesta> UpdateArticulo(int id, string? nombre, decimal? precio)
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
                    return res;
                }

                // Step 2: Update the articulo fields if the new values are provided
                if (!string.IsNullOrEmpty(nombre))
                {
                    articulo.NombreArticulo = nombre;
                }

                if (precio.HasValue && precio.Value > 0)
                {
                    articulo.Precio = precio.Value;
                }
                articulo.FechaRegistro = DateTime.Now;
                // Step 3: Save changes to the database
                _db.Articulos.Update(articulo);
                await _db.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = "Artículo actualizado correctamente.";
                res.Data = articulo;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpPost]
        [Route("CreateArticulo")]
        public async Task<Respuesta> CreateArticulo(string nombre, decimal precio)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Validate the input values
                if (string.IsNullOrEmpty(nombre))
                {
                    res.Ok = false;
                    res.Message = "El nombre del artículo es obligatorio.";
                    return res;
                }

                if (precio <= 0)
                {
                    res.Ok = false;
                    res.Message = "El precio del artículo debe ser mayor a cero.";
                    return res;
                }

                // Step 2: Create a new Articulo object
                Articulos nuevoArticulo = new Articulos
                {
                    NombreArticulo = nombre,
                    Precio = precio,
                    Anulado = false, // By default, the article is not annulled
                    FechaRegistro = DateTime.Now,
                };

                // Step 3: Add the new articulo to the database
                _db.Articulos.Add(nuevoArticulo);
                await _db.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = "Artículo creado correctamente.";
                res.Data = nuevoArticulo;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpPost]
        [Route("CreateArticuloWithImage")]
        public async Task<Respuesta> CreateArticuloWithImage([FromForm] string nombre, [FromForm] decimal precio, [FromForm] IFormFile? imagen)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Validar los valores de entrada
                if (string.IsNullOrEmpty(nombre))
                {
                    res.Ok = false;
                    res.Message = "El nombre del artículo es obligatorio.";
                    return res;
                }

                if (precio <= 0)
                {
                    res.Ok = false;
                    res.Message = "El precio del artículo debe ser mayor a cero.";
                    return res;
                }

                // Crear un nuevo objeto Articulo
                Articulos nuevoArticulo = new Articulos
                {
                    NombreArticulo = nombre,
                    Precio = precio,
                    Anulado = false, // Por defecto, el artículo no está anulado
                    FechaRegistro = DateTime.Now
                };

                // Agregar el artículo a la base de datos
                _db.Articulos.Add(nuevoArticulo);
                await _db.SaveChangesAsync();

                // Manejar la imagen si se proporciona
                if (imagen != null && imagen.Length > 0)
                {
                    // Generar un nombre único para la imagen
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                    var filePath = Path.Combine("wwwroot/uploads", fileName);

                    // Guardar la imagen en la carpeta de uploads
                    Directory.CreateDirectory("wwwroot/uploads");
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }

                    // Crear un nuevo objeto Imagen
                    Imagenes nuevaImagen = new Imagenes
                    {
                        Origen = filePath, // O cualquier otro identificador
                        NombreArchivo = fileName
                    };

                    // Agregar la imagen a la base de datos
                    // Agregar la imagen a la base de datos
                    _db.Imagenes.Add(nuevaImagen);
                    await _db.SaveChangesAsync();

                    nuevoArticulo.imagenID = nuevaImagen.ImagenId;
                    _db.Articulos.Update(nuevoArticulo);
                    await _db.SaveChangesAsync();
                    
                }

                // Responder con éxito
                res.Ok = true;
                res.Message = "Artículo creado con imagen correctamente.";
                res.Data = nuevoArticulo;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpDelete]
        [Route("AnularArticulo")]
        public async Task<Respuesta> AnularArticulo(int id, bool estado)
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
                    return res;
                }

                // Step 2: Set the Anulado property based on the estado parameter
                articulo.Anulado = estado;

                // Step 3: Save changes to the database
                _db.Articulos.Update(articulo);
                await _db.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = estado ? "Artículo anulado correctamente." : "Artículo desanulado correctamente.";
                res.Data = articulo;
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