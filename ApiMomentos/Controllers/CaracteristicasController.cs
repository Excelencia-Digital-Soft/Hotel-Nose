using ApiObjetos.Models.Sistema;
using ApiObjetos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiObjetos.Data;
using Microsoft.AspNetCore.Authorization;
using ApiObjetos.DTOs;
using Microsoft.AspNetCore.StaticFiles;

namespace ApiObjetos.Controllers
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
        public async Task<Respuesta> GetCaracteristicas()
        {
            Respuesta res = new Respuesta();
            try
            {
                var caracteristicas = await _db.Caracteristicas
                    .Select(c => new
                    {
                        c.CaracteristicaId,
                        c.Nombre,
                        Descripcion = c.Descripcion ?? string.Empty,
                        Icono = c.Icono ?? string.Empty
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
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Icono.FileName);
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
                    Icono = iconoPath
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
        public async Task<Respuesta> ActualizarCaracteristica(int id, [FromForm] CaracteristicaDTO request)
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
                    if (!string.IsNullOrEmpty(caracteristica.Icono) && System.IO.File.Exists(caracteristica.Icono))
                    {
                        System.IO.File.Delete(caracteristica.Icono);
                    }

                    // Guardar nueva imagen
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Icono.FileName);
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
        public async Task<Respuesta> AsignarCaracteristicasAHabitacion(int habitacionId, [FromBody] List<int> caracteristicaIds)
        {
            Respuesta res = new Respuesta();

            // Validación inicial
            if (caracteristicaIds == null || !caracteristicaIds.Any())
            {
                res.Ok = false;
                res.Message = "Debe proporcionar al menos una característica";
                return res;
            }

            try
            {
                // Materializar primero la lista de IDs para evitar múltiples enumeraciones
                var idsUnicos = caracteristicaIds.Distinct().ToList();

                // Usar transacción para operaciones atómicas
                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Verificar existencia de habitación y características en una sola consulta
                        var habitacion = await _db.Habitaciones
                            .AsNoTracking() // No necesitamos tracking para esta consulta
                            .FirstOrDefaultAsync(h => h.HabitacionId == habitacionId);

                        if (habitacion == null)
                        {
                            res.Ok = false;
                            res.Message = "Habitación no encontrada";
                            return res;
                        }

                        // Verificar existencia de características
                        var countCaracteristicas = await _db.Caracteristicas
                            .Where(c => idsUnicos.Contains(c.CaracteristicaId))
                            .CountAsync();

                        if (countCaracteristicas != idsUnicos.Count)
                        {
                            res.Ok = false;
                            res.Message = "Algunas características no existen";
                            return res;
                        }

                        // 2. Eliminar relaciones existentes (optimizado)
                        await _db.HabitacionCaracteristicas
                            .Where(hc => hc.HabitacionId == habitacionId)
                            .ExecuteDeleteAsync(); // EF Core 7+ (más eficiente)

                        // 3. Crear nuevas relaciones
                        var nuevasRelaciones = idsUnicos
                            .Select(cid => new HabitacionCaracteristica
                            {
                                HabitacionId = habitacionId,
                                CaracteristicaId = cid
                            }).ToList();

                        await _db.HabitacionCaracteristicas.AddRangeAsync(nuevasRelaciones);
                        await _db.SaveChangesAsync();

                        await transaction.CommitAsync();

                        res.Ok = true;
                        res.Message = "Características actualizadas correctamente";
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        res.Ok = false;
                        res.Message = "Error al asignar características: " + ex.Message;

                    }
                }
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = "Error en la transacción: " + ex.Message;
            }

            return res;
        }
        [HttpGet("GetImage/{idCaracteristica}")]
        public IActionResult GetImage(int idCaracteristica)
        {
            try
            {
                // Buscar la característica en la base de datos
                var caracteristica = _db.Caracteristicas.FirstOrDefault(c => c.CaracteristicaId == idCaracteristica);

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
                return StatusCode(500, new { message = "Error al obtener la imagen: " + ex.Message });
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
