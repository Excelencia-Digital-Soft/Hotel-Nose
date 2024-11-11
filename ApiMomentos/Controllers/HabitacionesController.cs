using ApiObjetos.Models.Sistema;
using ApiObjetos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ApiObjetos.Data;
using Microsoft.EntityFrameworkCore;

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

            public async Task<Respuesta> CrearHabitacion(string nombreHabitacion, int categoriaID)
            {
                Respuesta res = new Respuesta();
                try
                {
                    Habitaciones nuevaHabitacion = new Habitaciones
                    {

                        NombreHabitacion = nombreHabitacion,
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

                    var Objeto = await _db.Habitaciones.Include(h => h.Visita).Where(
                    t => t.HabitacionId == idHabitacion
                    ).ToListAsync();
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
        public async Task<Respuesta> GetHabitaciones()
        {
            Respuesta res = new Respuesta();
            try
            {
                // Obtener las habitaciones junto con su visita y verificar pedidos pendientes
                var habitaciones = await _db.Habitaciones
                    .Include(h => h.Visita)
                    .Where(h => h.Anulado == false)
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
                        // Verificar si hay al menos un encargo pendiente para esta habitación
                        PedidosPendientes = _db.Encargos
                            .Any(e => e.VisitaId == h.VisitaID && (e.Anulado ?? false) == false && (e.Entregado ?? false) == false)
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
        [Route("GetCategorias")] // Obtiene un paciente basado en su idPaciente. Se obtiene la lista de los idPaciente con el metodo GetPacientes
        [AllowAnonymous]

        public async Task<Respuesta> GetCategorias()
        {
            Respuesta res = new Respuesta();
            try
            {

                var Objeto = await _db.CategoriasHabitaciones.ToListAsync();
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
        public async Task<Respuesta> ActualizarHabitacion(int id, string? nuevoNombre, int nuevaCategoria, string? disponibilidad, DateTime? proximaReserva, int usuarioId)
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

                try
                {
                    if (!string.IsNullOrEmpty(nuevoNombre))
                    {
                        _db.Database.ExecuteSqlRaw(
                            "UPDATE Habitaciones SET NombreHabitacion = @Nombre WHERE HabitacionID = @Id",
                            new SqlParameter("@Nombre", nuevoNombre),
                            new SqlParameter("@Id", id)
                        );
                    }

                    if (nuevaCategoria > 0)
                    {
                        _db.Database.ExecuteSqlRaw(
                            "UPDATE Habitaciones SET CategoriaID = @CategoriaID WHERE HabitacionID = @Id",
                            new SqlParameter("@CategoriaID", nuevaCategoria),
                            new SqlParameter("@Id", id)
                        );
                    }

                    if (!string.IsNullOrEmpty(disponibilidad))
                    {
                        _db.Database.ExecuteSqlRaw(
                            "UPDATE Habitaciones SET Disponible = @Disponibilidad WHERE HabitacionID = @Id",
                            new SqlParameter("@Disponibilidad", disponibilidad),
                            new SqlParameter("@Id", id)
                        );
                    }

                    if (proximaReserva.HasValue && proximaReserva.Value >= new DateTime(1753, 1, 1) && proximaReserva.Value <= new DateTime(9999, 12, 31))
                    {
                        _db.Database.ExecuteSqlRaw(
                            "UPDATE Habitaciones SET ProximaReserva = @ProximaReserva WHERE HabitacionID = @Id",
                            new SqlParameter("@ProximaReserva", proximaReserva.Value),
                            new SqlParameter("@Id", id)
                        );
                    }

                    if (usuarioId > 0)
                    {
                        _db.Database.ExecuteSqlRaw(
                            "UPDATE Habitaciones SET UsuarioID = @UsuarioID WHERE HabitacionID = @Id",
                            new SqlParameter("@UsuarioID", usuarioId),
                            new SqlParameter("@Id", id)
                        );
                    }

                    // Return successful response
                    res.Ok = true;
                    res.Message = "Se actualizó la habitación";
                    return res;
                }
                catch (Exception e)
                {
                    // Handle inner exception
                    res.Ok = false;
                    res.Message = "Error: " + e.Message + e.StackTrace;
                    return res;
                }
            }
            catch (Exception e)
            {
                // Handle outer exception
                res.Ok = false;
                res.Message = "Error: " + e.Message + e.StackTrace;
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
            #endregion

        }
    }

