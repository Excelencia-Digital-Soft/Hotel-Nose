using ApiObjetos.Models.Sistema;
using ApiObjetos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ApiObjetos.Data;
using Microsoft.EntityFrameworkCore;
using ApiObjetos.DTOs;

namespace ApiObjetos.Controllers
{
    public class HabitacionesController : Controller
    {
        private readonly HotelDbContext _db;

        public HabitacionesController(HotelDbContext db, IConfiguration configuration)
        {
            _db = db;
        }
            #region Habitaciones
            [HttpPost]
            [Route("CrearHabitacion")] // Crea un nuevo paciente
            [AllowAnonymous]

            public async Task<Respuesta> CrearHabitacion(int institucionID, string nombreHabitacion, int categoriaID)
            {
                Respuesta res = new Respuesta();
                try
                {
                    Habitaciones nuevaHabitacion = new Habitaciones
                    {

                        NombreHabitacion = nombreHabitacion,
                        InstitucionID = institucionID,
                        CategoriaId = categoriaID,
                    };

                    _db.Add(nuevaHabitacion);
                    await _db.SaveChangesAsync();

                    res.Message = "La habitación se creó correctamente";
                    res.Ok = true;
                }
                catch (Exception ex)
                {
                    res.Message = $"Error: {ex.Message} {ex.InnerException}";
                    res.Ok = false;
                }

                return res;
            }


            [HttpGet]
            [Route("GetHabitacion")] // Obtiene un paciente basado en su idPaciente. Se obtiene la lista de los idPaciente con el metodo GetPacientes
            [AllowAnonymous]

            public async Task<Respuesta> GetHabitacion(int idHabitacion)
            {
                Respuesta res = new Respuesta();
                try
                {
                var Objeto = await _db.Habitaciones
                    .Where(t => t.HabitacionId == idHabitacion)
                    .Include(h => h.Categoria)  // Include Categoria to access PrecioNormal
                    .Select(h => new {
                        Habitacion = h,
                        Visita = h.Visita,
                        Precio = h.Categoria.PrecioNormal,  // Include PrecioNormal from Categoria
                        ReservaActiva = h.Visita.Reservas.FirstOrDefault(r => r.FechaFin == null) // Only the active reserva
                    })
                    .ToListAsync();
                res.Ok = true;
                    res.Data = Objeto[0];
                    return res;



                }
                catch (Exception ex)
                {
                    res.Message = ex.ToString();
                    res.Ok = false;
                }
                return res;
            }

        [HttpGet]
        [Route("GetHabitaciones")]
        [AllowAnonymous]
        public async Task<Respuesta> GetHabitaciones(int institucionID)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Obtener las habitaciones junto con su visita, verificar pedidos pendientes y incluir imágenes
                var habitaciones = await _db.Habitaciones
                    .Include(h => h.Visita)
                    .ThenInclude(v => v.Reservas) // Include all Reservas
                    .Include(h => h.Categoria)  // Include Categoria to access PrecioNormal
                    .Include(h => h.HabitacionImagenes) // Include HabitacionImagenes to access ImagenId
                    .Include(h => h.HabitacionCaracteristicas)
                    .ThenInclude(hc => hc.Caracteristica) // Incluye las características
                    .Where(h => h.Anulado == false && h.InstitucionID == institucionID)
                    .Select(h => new
                    {
                        h.HabitacionId,
                        h.NombreHabitacion,
                        h.CategoriaId,
                        h.Disponible,
                        h.ProximaReserva,
                        h.UsuarioId,
                        h.FechaRegistro,
                        h.Anulado,
                        h.VisitaID,
                        h.Visita,
                        Precio = h.Categoria.PrecioNormal,  // Directly include PrecioNormal from Categoria
                        ReservaActiva = h.Visita.Reservas.FirstOrDefault(r => r.FechaFin == null), // Only active reserva
                        PedidosPendientes = _db.Encargos
                            .Any(e => e.VisitaId == h.VisitaID && (e.Anulado ?? false) == false && (e.Entregado ?? false) == false),
                        Imagenes = h.HabitacionImagenes
                        .Where(hi => hi.Anulado == false) // Filtro para excluir imágenes anuladas    
                        .Select(hi => hi.ImagenId) // Select ImagenId from HabitacionImagenes
                            .ToList(),
                        Caracteristicas = h.HabitacionCaracteristicas
                            .Select(hc => new {
                                caracteristicaId = hc.CaracteristicaId,
                                nombre = hc.Caracteristica.Nombre,
                                descripcion = hc.Caracteristica.Descripcion,
                                icono = hc.Caracteristica.Icono

                            })
                            .ToList()
                    })
                    .ToListAsync();

                res.Ok = true;
                res.Data = habitaciones;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Error " + ex.ToString();
                res.Ok = false;
            }
            return res;
        }

        [HttpGet]
        [Route("GetHabLibresXCat")]
        [AllowAnonymous]
        public async Task<Respuesta> GetHabLibresXCat(int institucionID)
        {
            var res = new Respuesta();
            try
            {
                var categoriasConHabitaciones = await _db.CategoriasHabitaciones
                    .AsNoTracking()
                    .Where(c => c.Habitaciones.Any(h =>
                        (h.Disponible ?? false) &&
                        !(h.Anulado ?? false) &&
                        h.InstitucionID == institucionID))
                    .Select(c => new
                    {
                        CategoriaId = c.CategoriaId,
                        NombreCategoria = c.NombreCategoria,
                        PrecioNormal = c.PrecioNormal,
                        Habitaciones = c.Habitaciones
                            .Where(h =>
                                (h.Disponible ?? false) &&
                                !(h.Anulado ?? false) &&
                                h.InstitucionID == institucionID)
                            .Select(h => new
                            {
                               
                                h.HabitacionId,
                                h.NombreHabitacion,
                                h.Disponible,
                                CategoriaId = c.CategoriaId,
                                Precio = c.PrecioNormal,
                                ReservaActiva = h.Visita != null ?
                                    h.Visita.Reservas.FirstOrDefault(r => r.FechaFin == null) : null,
                                PedidosPendientes = h.VisitaID.HasValue ?
                                    _db.Encargos
                                        .Any(e => e.VisitaId == h.VisitaID &&
                                               (e.Anulado == null || e.Anulado == false) &&
                                               (e.Entregado == null || e.Entregado == false)) : false,
                                Imagenes = h.HabitacionImagenes
                                    .Where(hi => hi.Anulado == false)
                                    .Select(hi => hi.ImagenId)
                                    .ToList(),
                                Caracteristicas = h.HabitacionCaracteristicas
                                    .Select(hc => new
                                    {
                                        hc.CaracteristicaId,
                                        hc.Caracteristica.Nombre,
                                        Descripcion = hc.Caracteristica.Descripcion ?? string.Empty,
                                        Icono = hc.Caracteristica.Icono ?? string.Empty
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToListAsync();

                res.Ok = true;
                res.Data = categoriasConHabitaciones;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = "Error al obtener habitaciones";
            }
            return res;
        }


        [HttpPost]
        [Route("CrearHabitacionConImagenes")]
        [AllowAnonymous]
        public async Task<Respuesta> CrearHabitacionConImagenes(
        int institucionID,
        [FromForm] string nombreHabitacion,
        [FromForm] int categoriaID,
        [FromForm] IFormFile[] imagenes)
        {
            Respuesta res = new Respuesta();
            try
            {
                Habitaciones nuevaHabitacion = new Habitaciones
                {
                    NombreHabitacion = nombreHabitacion,
                    InstitucionID = institucionID,
                    CategoriaId = categoriaID,
                    FechaRegistro = DateTime.Now
                };

                _db.Habitaciones.Add(nuevaHabitacion);
                await _db.SaveChangesAsync(); // Guardar la nueva habitación antes de procesar imágenes

                List<Imagenes> imagenesAAgregar = new List<Imagenes>();
                List<HabitacionImagenes> relacionesAAgregar = new List<HabitacionImagenes>();

                // Guardar imágenes en lista para insertarlas en lote
                if (imagenes != null && imagenes.Length > 0)
                {
                    foreach (var imagen in imagenes)
                    {
                        if (imagen.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                            var filePath = Path.Combine("wwwroot/uploads", fileName);

                            Directory.CreateDirectory("wwwroot/uploads");
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await imagen.CopyToAsync(stream);
                            }

                            Imagenes nuevaImagen = new Imagenes
                            {
                                Origen = filePath,
                                NombreArchivo = fileName
                            };

                            imagenesAAgregar.Add(nuevaImagen);
                        }
                    }

                    // Insertar todas las imágenes en la base de datos
                    _db.Imagenes.AddRange(imagenesAAgregar);
                    await _db.SaveChangesAsync();

                    // Crear relaciones con la habitación
                    foreach (var img in imagenesAAgregar)
                    {
                        relacionesAAgregar.Add(new HabitacionImagenes
                        {
                            HabitacionId = nuevaHabitacion.HabitacionId,
                            ImagenId = img.ImagenId
                        });
                    }

                    // Guardar relaciones en un solo SaveChangesAsync
                    _db.HabitacionImagenes.AddRange(relacionesAAgregar);
                    await _db.SaveChangesAsync();
                }

                res.Message = "Habitación creada con imágenes correctamente";
                res.Ok = true;
                res.Data = new HabitacionDTO
                {
                    HabitacionId = nuevaHabitacion.HabitacionId,
                    NombreHabitacion = nuevaHabitacion.NombreHabitacion,
                    InstitucionID = nuevaHabitacion.InstitucionID,
                    CategoriaId = nuevaHabitacion.CategoriaId,
                    Imagenes = _db.HabitacionImagenes
                .Where(hi => hi.HabitacionId == nuevaHabitacion.HabitacionId)
                .Select(hi => hi.Imagen.Origen)
                .ToList()
                };
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpGet]
        [Route("GetCategorias")] // Obtiene un paciente basado en su idPaciente. Se obtiene la lista de los idPaciente con el metodo GetPacientes
        [AllowAnonymous]

        public async Task<Respuesta> GetCategorias(int institucionID)
        {
            Respuesta res = new Respuesta();
            try
            {

                var Objeto = await _db.CategoriasHabitaciones.Where(c => c.Anulado == false && c.InstitucionID == institucionID).ToListAsync();
                res.Ok = true;
                res.Data = Objeto;
                return res;



            }
            catch (Exception ex)
            {
                res.Message = "Error " + ex.ToString();
                res.Ok = false;
            }
            return res;
        }

        [HttpGet]
        [Route("GetVisitaId")]
        [AllowAnonymous]
        public async Task<Respuesta> GetVisitaId(int idHabitacion)
        {
            Respuesta res = new Respuesta();
            try
            {
                var habitacion = await _db.Habitaciones
                    .Where(h => h.HabitacionId == idHabitacion && h.Anulado == false)
                    .Select(h => h.VisitaID)
                    .FirstOrDefaultAsync();

                if (habitacion == null)
                {
                    res.Ok = false;
                    res.Message = "No se encontró la habitación o está anulada.";
                }
                else
                {
                    res.Ok = true;
                    res.Data = habitacion;
                    res.Message = "VisitaID encontrado exitosamente.";
                }
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }
            return res;
        }

        [HttpPut]
        [Route("ActualizarHabitacion")]
        [AllowAnonymous]
        public async Task<Respuesta> ActualizarHabitacion(
    int id,
    [FromForm] string? nuevoNombre,
    [FromForm] int nuevaCategoria,
    [FromForm] string? disponibilidad,
    [FromForm] DateTime? proximaReserva,
    [FromForm] int usuarioId,
    [FromForm] IFormFile[]? nuevasImagenes) // Add this parameter for new images
        {
            Respuesta res = new Respuesta();
            try
            {
                var habitacion = await _db.Habitaciones.FindAsync(id);
                if (habitacion == null)
                {
                    res.Ok = false;
                    res.Message = "No se encontró la habitación";
                    return res;
                }

                // Update room details
                if (!string.IsNullOrEmpty(nuevoNombre))
                {
                    habitacion.NombreHabitacion = nuevoNombre;
                }

                if (nuevaCategoria > 0)
                {
                    habitacion.CategoriaId = nuevaCategoria;
                }

                if (!string.IsNullOrEmpty(disponibilidad))
                {
                    habitacion.Disponible = disponibilidad == "1" ? true : false;
                }

                if (proximaReserva.HasValue && proximaReserva.Value >= new DateTime(1753, 1, 1) && proximaReserva.Value <= new DateTime(9999, 12, 31))
                {
                    habitacion.ProximaReserva = proximaReserva.Value;
                }

                if (usuarioId > 0)
                {
                    habitacion.UsuarioId = usuarioId;
                }

                // Save changes to the room
                await _db.SaveChangesAsync();

                // Handle new images
                if (nuevasImagenes != null && nuevasImagenes.Length > 0)
                {
                    List<Imagenes> imagenesAAgregar = new List<Imagenes>();
                    List<HabitacionImagenes> relacionesAAgregar = new List<HabitacionImagenes>();

                    foreach (var imagen in nuevasImagenes)
                    {
                        if (imagen.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                            var filePath = Path.Combine("wwwroot/uploads", fileName);

                            Directory.CreateDirectory("wwwroot/uploads");
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await imagen.CopyToAsync(stream);
                            }

                            Imagenes nuevaImagen = new Imagenes
                            {
                                Origen = filePath,
                                NombreArchivo = fileName
                            };

                            imagenesAAgregar.Add(nuevaImagen);
                        }
                    }

                    // Insert new images into the database
                    _db.Imagenes.AddRange(imagenesAAgregar);
                    await _db.SaveChangesAsync();

                    // Create relationships between the room and the new images
                    foreach (var img in imagenesAAgregar)
                    {
                        relacionesAAgregar.Add(new HabitacionImagenes
                        {
                            HabitacionId = habitacion.HabitacionId,
                            ImagenId = img.ImagenId
                        });
                    }

                    // Save relationships
                    _db.HabitacionImagenes.AddRange(relacionesAAgregar);
                    await _db.SaveChangesAsync();
                }

                // Return successful response
                res.Ok = true;
                res.Message = "Se actualizó la habitación y se agregaron nuevas imágenes";
                return res;
            }
            catch (Exception e)
            {
                // Handle exceptions
                res.Ok = false;
                res.Message = "Error: " + e.Message + e.StackTrace;
                return res;
            }
        }

        [HttpDelete("EliminarImagenHabitacion")]
        public async Task<Respuesta> EliminarImagenHabitacion(int imagenId)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Find the relationship to anular
                var relacion = await _db.HabitacionImagenes
                    .FirstOrDefaultAsync(hi => hi.ImagenId == imagenId);

                if (relacion == null)
                {
                    res.Ok = false;
                    res.Message = "No se encontró la relación entre la habitación y la imagen.";
                    return res;
                }

                // Anular the relationship
                relacion.Anulado = true;
                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Relación anulada correctamente.";
                return res;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = "Error: " + ex.Message;
                return res;
            }
        }

        [HttpDelete]
            [Route("AnularHabitacion")] // Encuentra el ID del paciente para luego eliminarlo
            [AllowAnonymous]
            public async Task<Respuesta> AnularHabitacion(int idHabitacion, bool Estado)
            {
                Respuesta res = new Respuesta();
                try
                {
                    var habitacion = await _db.Habitaciones.FindAsync(idHabitacion);

                    if (habitacion == null)
                    {
                        res.Ok = false;
                        res.Message = $"La habitación con el id {idHabitacion} no se encontró.";
                    }
                    else
                    {
                        habitacion.Anulado = Estado;
                        await _db.SaveChangesAsync();

                        res.Ok = true;
                        res.Message = $"Se anuló la habitación correctamente";
                    }
                }
                catch (Exception ex)
                {
                    res.Ok = false;
                    res.Message = $"Ocurrió un error: {ex.Message}";
                }

                return res;
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
                        Icono = c.Icono ?? string.Empty // Incluir el ícono en la respuesta
                    })
                    .ToListAsync();

                res.Ok = true;
                res.Data = caracteristicas;
                return res;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = "Error al obtener las características: " + ex.Message;
                return res;
            }
        }


        #endregion

    }
    }

