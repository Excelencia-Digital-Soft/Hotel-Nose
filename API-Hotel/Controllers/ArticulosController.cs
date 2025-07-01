using hotel.Data;
using hotel.Models;
using hotel.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticulosController : ControllerBase
    {
        private readonly HotelDbContext _db;
        private const string UPLOADS_PATH = "wwwroot/uploads";

        public ArticulosController(HotelDbContext context)
        {
            _db = context;
        }

        // Method to get all articulos
        [HttpGet]
        [Route("GetArticulos")]
        public async Task<Respuesta> GetArticulos(int institucionID, int? categoriaID)
        {
            Respuesta res = new Respuesta();

            try
            {
                List<Articulos> articulos;
                // Step 1: Retrieve all articulos from the database
                if (categoriaID == null)
                {
                    articulos = await _db
                        .Articulos.Where(a => a.Anulado != true && a.InstitucionID == institucionID)
                        .ToListAsync();
                }
                else
                {
                    articulos = await _db
                        .Articulos.Where(a =>
                            a.Anulado != true
                            && a.CategoriaID == categoriaID
                            && a.InstitucionID == institucionID
                        )
                        .ToListAsync();
                }
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
        public async Task<Respuesta> UpdateArticulo(
            int id,
            string? nombre,
            decimal? precio,
            int? categoriaID
        )
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

                if (categoriaID.HasValue)
                {
                    // Optionally validate the provided CategoriaID exists
                    if (
                        await _db.CategoriasArticulos.AnyAsync(c =>
                            c.CategoriaId == categoriaID.Value
                        )
                    )
                    {
                        articulo.CategoriaID = categoriaID.Value;
                    }
                    else
                    {
                        res.Ok = false;
                        res.Message = $"La categoría con ID: {categoriaID.Value} no existe.";
                        return res;
                    }
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
        public async Task<Respuesta> CreateArticulo(
            int institucionID,
            string nombre,
            decimal precio,
            int? categoriaID
        )
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

                // Step 2: Validate or set CategoriaID
                int categoriaToUse = categoriaID ?? 1; // Default to CategoriaID = 1
                if (!await _db.CategoriasArticulos.AnyAsync(c => c.CategoriaId == categoriaToUse))
                {
                    res.Ok = false;
                    res.Message = $"La categoría con ID: {categoriaToUse} no existe.";
                    return res;
                }

                // Step 3: Create a new Articulo object
                Articulos nuevoArticulo = new Articulos
                {
                    NombreArticulo = nombre,
                    Precio = precio,
                    CategoriaID = categoriaToUse,
                    Anulado = false, // By default, the article is not annulled
                    FechaRegistro = DateTime.Now,
                    InstitucionID = institucionID,
                };

                // Step 4: Add the new articulo to the database
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

        [HttpPut]
        [Route("UpdateArticuloImage")]
        public async Task<Respuesta> UpdateArticuloImage(
            [FromForm] int articuloID,
            [FromForm] IFormFile nuevaImagen
        )
        {
            Respuesta res = new Respuesta();

            try
            {
                // Find the article by ID
                var articulo = await _db.Articulos.FindAsync(articuloID);

                if (articulo == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el artículo con ID: {articuloID}.";
                    return res;
                }

                if (nuevaImagen == null || nuevaImagen.Length == 0)
                {
                    res.Ok = false;
                    res.Message = "La imagen proporcionada no es válida.";
                    return res;
                }

                // Handle the new image
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(nuevaImagen.FileName);
                var filePath = Path.Combine(UPLOADS_PATH, fileName);

                Directory.CreateDirectory(UPLOADS_PATH);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await nuevaImagen.CopyToAsync(stream);
                }

                // Create a new image record
                Imagenes nuevaImagenDB = new Imagenes
                {
                    NombreArchivo = fileName,
                    FechaSubida = DateTime.Now,
                    InstitucionID = articulo.InstitucionID
                };

                _db.Imagenes.Add(nuevaImagenDB);
                await _db.SaveChangesAsync();

                // Optionally, clean up the old image file (if needed)
                var oldImage = await _db.Imagenes.FindAsync(articulo.imagenID);
                if (oldImage != null)
                {
                    var oldFilePath = Path.Combine(UPLOADS_PATH, oldImage.NombreArchivo);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    _db.Imagenes.Remove(oldImage);
                }

                // Link the new image to the article
                articulo.imagenID = nuevaImagenDB.ImagenId;
                _db.Articulos.Update(articulo);
                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Imagen del artículo actualizada correctamente.";
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
        [Route("CreateArticuloWithImage")]
        public async Task<Respuesta> CreateArticuloWithImage(
            int InstitucionID,
            [FromForm] string nombre,
            [FromForm] decimal precio,
            [FromForm] IFormFile? imagen,
            [FromForm] int categoriaID
        )
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

                // Validar que la categoría existe
                var categoria = await _db.CategoriasArticulos.FindAsync(categoriaID);
                if (categoria == null)
                {
                    res.Ok = false;
                    res.Message = "La categoría especificada no existe.";
                    return res;
                }

                // Crear un nuevo objeto Articulo
                Articulos nuevoArticulo = new Articulos
                {
                    NombreArticulo = nombre,
                    Precio = precio,
                    Anulado = false, // Por defecto, el artículo no está anulado
                    FechaRegistro = DateTime.Now,
                    CategoriaID = categoriaID, // Asignar la categoría al artículo
                    InstitucionID = InstitucionID,
                };

                // Agregar el artículo a la base de datos
                _db.Articulos.Add(nuevoArticulo);
                await _db.SaveChangesAsync();

                // Manejar la imagen si se proporciona
                if (imagen != null && imagen.Length > 0)
                {
                    // Generar un nombre único para la imagen
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                    var filePath = Path.Combine(UPLOADS_PATH, fileName);

                    // Guardar la imagen en la carpeta de uploads
                    Directory.CreateDirectory(UPLOADS_PATH);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }

                    // Crear un nuevo objeto Imagen
                    Imagenes nuevaImagen = new Imagenes
                    {
                        NombreArchivo = fileName,
                        FechaSubida = DateTime.Now,
                        InstitucionID = InstitucionID
                    };

                    // Agregar la imagen a la base de datos
                    _db.Imagenes.Add(nuevaImagen);
                    await _db.SaveChangesAsync();

                    // Asociar la imagen con el artículo
                    nuevoArticulo.imagenID = nuevaImagen.ImagenId;
                    _db.Articulos.Update(nuevoArticulo);
                    await _db.SaveChangesAsync();
                }

                // Responder con éxito
                res.Ok = true;
                res.Message = "Artículo creado con imagen y categoría correctamente.";
                res.Data = nuevoArticulo;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();

            // Add other content types as needed
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream", // Default content type for unknown extensions
            };
        }

        [HttpGet("GetImage/{idArticulo}")]
        public IActionResult GetImage(int idArticulo)
        {
            // Access the database to find the DTOAttachment by fileName
            var articulo = _db.Articulos.FirstOrDefault(a => a.ArticuloId == idArticulo);
            var imagen = _db.Imagenes.FirstOrDefault(i => i.ImagenId == articulo!.imagenID);

            // Check if the attachment was found
            if (imagen != null)
            {
                // Combine the attachmentPath with the fileName to get the full path
                var imagePath = Path.Combine(UPLOADS_PATH, imagen.NombreArchivo);

                // Check if the file exists at the specified path
                if (System.IO.File.Exists(imagePath))
                {
                    // Determine the content type based on the file extension
                    var contentType = GetContentType(imagePath);

                    // Read the file data
                    var imageData = System.IO.File.ReadAllBytes(imagePath);

                    // Return the file data with the appropriate content type
                    return File(imageData, contentType);
                }
                else
                {
                    // Return a 404 Not Found response if the image file does not exist
                    return NotFound(new { message = "Image file not found" });
                }
            }
            else
            {
                // Return a 404 Not Found response if the DTOAttachment record does not exist
                return NotFound(new { message = "Attachment not found in database" });
            }
        }

        [HttpGet("GetImagenGlobal/{imagenId}")]
        public IActionResult GetImagenGlobal(int imagenId)
        {
            // Access the database to find the image by imagenId
            var imagen = _db.Imagenes.FirstOrDefault(i => i.ImagenId == imagenId);

            // Check if the image was found
            if (imagen != null)
            {
                // Combine the attachmentPath with the fileName to get the full path
                var imagePath = Path.Combine(UPLOADS_PATH, imagen.NombreArchivo);

                // Check if the file exists at the specified path
                if (System.IO.File.Exists(imagePath))
                {
                    // Determine the content type based on the file extension
                    var contentType = GetContentType(imagePath);

                    // Read the file data
                    var imageData = System.IO.File.ReadAllBytes(imagePath);

                    // Return the file data with the appropriate content type
                    return File(imageData, contentType);
                }
                else
                {
                    // Return a 404 Not Found response if the image file does not exist
                    return NotFound(new { message = "Image file not found" });
                }
            }
            else
            {
                // Return a 404 Not Found response if the image record does not exist
                return NotFound(new { message = "Image not found in database" });
            }
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
                res.Message = estado
                    ? "Artículo anulado correctamente."
                    : "Artículo desanulado correctamente.";
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

