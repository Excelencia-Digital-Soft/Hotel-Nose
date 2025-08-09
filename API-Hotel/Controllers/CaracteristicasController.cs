using hotel.Data;
using hotel.DTOs;
using hotel.Models;
using hotel.Models.Sistema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaracteristicasController : ControllerBase
    {
        private readonly HotelDbContext _db;

        public CaracteristicasController(HotelDbContext db, IConfiguration configuration)
        {
            _db = db;
        }

        [HttpGet("GetCaracteristicas")]
        [Obsolete("This endpoint is deprecated. Use GET /api/v1/caracteristicas instead.")]
        public async Task<Respuesta> GetCaracteristicas()
        {
            Respuesta res = new Respuesta();
            try
            {
                var caracteristicas = await _db
                    .Caracteristicas.Select(c => new
                    {
                        c.CaracteristicaId,
                        c.Nombre,
                        Descripcion = c.Descripcion ?? string.Empty,
                        Icono = c.Icono ?? string.Empty,
                    })
                    .ToListAsync();

                res.Ok = true;
                res.Data = caracteristicas;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = "Error al obtener las características: " + ex.Message;
            }
            return res;
        }

        [HttpPost]
        [Route("CrearCaracteristica")]
        [AllowAnonymous]
        [Obsolete("This endpoint is deprecated. Use POST /api/v1/caracteristicas instead.")]
        public async Task<Respuesta> CrearCaracteristica([FromForm] CaracteristicaDTO request)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Validar que el nombre sea obligatorio
                if (string.IsNullOrEmpty(request.Nombre))
                {
                    res.Ok = false;
                    res.Message = "El nombre es obligatorio";
                    return res;
                }

                string iconoPath = string.Empty;
                if (request.Icono != null && request.Icono.Length > 0)
                {
                    var fileName =
                        Guid.NewGuid().ToString() + Path.GetExtension(request.Icono.FileName);
                    var filePath = Path.Combine("wwwroot/uploads", fileName);
                    Directory.CreateDirectory("wwwroot/uploads");
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Icono.CopyToAsync(stream);
                    }
                    iconoPath = filePath;
                }

                Caracteristica nuevaCaracteristica = new Caracteristica
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion ?? string.Empty,
                    Icono = iconoPath,
                };
                _db.Caracteristicas.Add(nuevaCaracteristica);
                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Característica creada correctamente";
                res.Data = nuevaCaracteristica;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = "Error al crear la característica: " + ex.Message;
            }
            return res;
        }

        [HttpPut("ActualizarCaracteristica/{id}")]
        [AllowAnonymous]
        [Obsolete("This endpoint is deprecated. Use PUT /api/v1/caracteristicas/{id} instead.")]
        public async Task<Respuesta> ActualizarCaracteristica(
            int id,
            [FromForm] CaracteristicaDTO request
        )
        {
            Respuesta res = new Respuesta();
            try
            {
                // Validar que el nombre sea obligatorio
                if (string.IsNullOrEmpty(request.Nombre))
                {
                    res.Ok = false;
                    res.Message = "El nombre es obligatorio";
                    return res;
                }

                var caracteristica = await _db.Caracteristicas.FindAsync(id);
                if (caracteristica == null)
                {
                    res.Ok = false;
                    res.Message = "Característica no encontrada";
                    return res;
                }

                // Actualizar propiedades básicas
                caracteristica.Nombre = request.Nombre;
                caracteristica.Descripcion = request.Descripcion ?? string.Empty;

                // Manejo de la imagen (solo si se proporciona una nueva)
                if (request.Icono != null && request.Icono.Length > 0)
                {
                    // Eliminar imagen anterior si existe
                    if (
                        !string.IsNullOrEmpty(caracteristica.Icono)
                        && System.IO.File.Exists(caracteristica.Icono)
                    )
                    {
                        System.IO.File.Delete(caracteristica.Icono);
                    }

                    // Guardar nueva imagen
                    var fileName =
                        Guid.NewGuid().ToString() + Path.GetExtension(request.Icono.FileName);
                    var filePath = Path.Combine("wwwroot/uploads", fileName);
                    Directory.CreateDirectory("wwwroot/uploads");

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.Icono.CopyToAsync(stream);
                    }

                    caracteristica.Icono = filePath;
                }

                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Característica actualizada correctamente";
                res.Data = caracteristica;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = "Error al actualizar la característica: " + ex.Message;
            }
            return res;
        }

        [HttpDelete("EliminarCaracteristica/{id}")]
        [Obsolete("This endpoint is deprecated. Use DELETE /api/v1/caracteristicas/{id} instead.")]
        public async Task<Respuesta> EliminarCaracteristica(int id)
        {
            Respuesta res = new Respuesta();
            try
            {
                var caracteristica = await _db.Caracteristicas.FindAsync(id);
                if (caracteristica == null)
                {
                    res.Ok = false;
                    res.Message = "Característica no encontrada";
                    return res;
                }

                _db.Caracteristicas.Remove(caracteristica);
                await _db.SaveChangesAsync();
                res.Ok = true;
                res.Message = "Característica eliminada correctamente";
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = "Error al eliminar la característica: " + ex.Message;
            }
            return res;
        }

        [HttpPost("AsignarCaracteristicasAHabitacion")]
        [Obsolete("This endpoint is deprecated. Use POST /api/v1/caracteristicas/rooms/{roomId}/assign instead.")]
        public async Task<Respuesta> AsignarCaracteristicasAHabitacion(
            int habitacionId,
            [FromBody] List<int>? caracteristicaIds
        )
        {
            var res = new Respuesta();

            try
            {
                // Validar habitación primero (evita operaciones innecesarias)
                var habitacionExiste = await _db.Habitaciones.AnyAsync(h =>
                    h.HabitacionId == habitacionId
                );

                if (!habitacionExiste)
                {
                    res.Ok = false;
                    res.Message = "Habitación no encontrada";
                    return res;
                }

                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Paso 1: Eliminar relaciones existentes (si las hay)
                        await _db
                            .HabitacionCaracteristicas.Where(hc => hc.HabitacionId == habitacionId)
                            .ExecuteDeleteAsync();

                        // Paso 2: Si se enviaron características, crear nuevas relaciones
                        if (caracteristicaIds != null && caracteristicaIds.Any())
                        {
                            // Validar existencia de características (en una sola consulta)
                            var idsUnicos = caracteristicaIds.Distinct().ToList();
                            var countCaracteristicas = await _db
                                .Caracteristicas.Where(c => idsUnicos.Contains(c.CaracteristicaId))
                                .CountAsync();

                            if (countCaracteristicas != idsUnicos.Count)
                            {
                                res.Ok = false;
                                res.Message = "Algunas características no existen";
                                return res;
                            }

                            // Crear relaciones
                            var nuevasRelaciones = idsUnicos
                                .Select(cid => new HabitacionCaracteristica
                                {
                                    HabitacionId = habitacionId,
                                    CaracteristicaId = cid,
                                })
                                .ToList();

                            await _db.HabitacionCaracteristicas.AddRangeAsync(nuevasRelaciones);
                        }

                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();

                        res.Ok = true;
                        res.Message =
                            caracteristicaIds?.Any() == true
                                ? "Características actualizadas correctamente"
                                : "Todas las características fueron removidas";
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        // Log.Error(ex, "Error en transacción");
                        res.Ok = false;
                        res.Message = "Error interno al asignar características," + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log.Error(ex, "Error general");
                res.Ok = false;
                res.Message = "Error procesando la solicitud, " + ex.Message;
            }

            return res;
        }

        [HttpGet("GetImage/{idCaracteristica}")]
        [Obsolete("This endpoint is deprecated. Use GET /api/v1/caracteristicas/{id}/image instead.")]
        public IActionResult GetImage(int idCaracteristica)
        {
            try
            {
                // Buscar la característica en la base de datos
                var caracteristica = _db.Caracteristicas.FirstOrDefault(c =>
                    c.CaracteristicaId == idCaracteristica
                );

                if (caracteristica == null || string.IsNullOrEmpty(caracteristica.Icono))
                {
                    return NotFound(new { message = "Característica o imagen no encontrada" });
                }

                // Obtener la ruta de la imagen
                var imagePath = Path.Combine(caracteristica.Icono);

                // Verificar si el archivo existe
                if (!System.IO.File.Exists(imagePath))
                {
                    return NotFound(new { message = "Archivo de imagen no encontrado" });
                }

                // Leer la imagen y devolverla
                var imageData = System.IO.File.ReadAllBytes(imagePath);
                var contentType = GetContentType(imagePath); // Método para determinar el tipo de contenido
                return File(imageData, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { message = "Error al obtener la imagen: " + ex.Message }
                );
            }
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
