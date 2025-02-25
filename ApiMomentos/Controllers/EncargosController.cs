using ApiObjetos.Data;
using ApiObjetos.DTOs;
using ApiObjetos.Models;
using ApiObjetos.Models.Sistema;
using ApiObjetos.NotificacionesHub;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class EncargosController : ControllerBase
{
    private readonly HotelDbContext _db;
    private readonly IMapper _mapper;

    public EncargosController(HotelDbContext context, IMapper mapper)
    {
        _db = context;
        _mapper = mapper;
    }

    // GET: api/Encargos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Encargos>>> GetEncargos()
    {
        return await _db.Encargos.ToListAsync();
    }

    // GET: api/Encargos/Habitaciones
    [HttpGet("Habitaciones")]
    public async Task<Respuesta> GetEncargosConHabitaciones(int idInstitucion)
    {
        Respuesta res = new Respuesta();

        try
        {
            var result = await (from e in _db.Encargos
                                join r in _db.Reservas on e.VisitaId equals r.VisitaId
                                join v in _db.Visitas on r.VisitaId equals v.VisitaId
                                join h in _db.Habitaciones on r.HabitacionId equals h.HabitacionId
                                join a in _db.Articulos on e.ArticuloId equals a.ArticuloId
                                where (!v.Anulado && e.Entregado != true && h.InstitucionID == idInstitucion)
                                select new
                                {
                                    h.HabitacionId,
                                    h.NombreHabitacion,
                                    h.VisitaID,
                                    Encargo = new
                                    {
                                        e.EncargosId,
                                        e.FechaCrea,
                                        a.ArticuloId,
                                        NombreArt = a.NombreArticulo,
                                        e.CantidadArt,
                                        e.Entregado
                                    }
                                })
                                .ToListAsync();

            if (result == null || result.Count == 0)
            {
                res.Message = "No encargo entries found.";
                res.Ok = false;
            }
            else
            {
                // Agrupar por HabitacionId
                var groupedResult = result.GroupBy(x => new { x.HabitacionId, x.NombreHabitacion, x.VisitaID })
                    .Select(g => new
                    {
                        HabitacionId = g.Key.HabitacionId,
                        NombreHabitacion = g.Key.NombreHabitacion,
                        VisitaId = g.Key.VisitaID,
                        Encargos = g.Select(x => x.Encargo).ToList()
                    }).ToList();

                res.Ok = true;
                res.Message = "Encargos y habitaciones retrieved successfully.";
                res.Data = groupedResult;
            }
        }
        catch (Exception ex)
        {
            res.Message = $"Error: {ex.Message} {ex.InnerException}";
            res.Ok = false;
        }

        return res;
    }

    // GET: api/Encargos/Visita/5
    [HttpGet("Visita/{visitaId}")]
    public async Task<Respuesta> GetEncargosByVisitaId(int visitaId)
    {
        Respuesta res = new Respuesta();

        try
        {
            var result = await (from e in _db.Encargos
                                join r in _db.Reservas on e.VisitaId equals r.VisitaId
                                join h in _db.Habitaciones on r.HabitacionId equals h.HabitacionId
                                where e.VisitaId == visitaId
                                select new
                                {
                                    h.HabitacionId,
                                    h.NombreHabitacion,
                                    Encargo = new EncargoDTO
                                    {
                                        EncargosId = e.EncargosId,
                                        FechaCrea = e.FechaCrea,
                                        ArticuloId = e.ArticuloId,
                                        CantidadArt = e.CantidadArt,
                                        Entregado = e.Entregado
                                    }
                                })
                                .ToListAsync();

            if (result == null || result.Count == 0)
            {
                res.Message = "No encargo entries found for the specified VisitaId.";
                res.Ok = false;
            }
            else
            {
                // Agrupar por HabitacionId
                var groupedResult = result.GroupBy(x => new { x.HabitacionId, x.NombreHabitacion })
                    .Select(g => new HabitacionEncargosDTO
                    {
                        HabitacionId = g.Key.HabitacionId,
                        NombreHabitacion = g.Key.NombreHabitacion,
                        Encargos = g.Select(x => x.Encargo).ToList()
                    }).ToList();

                res.Ok = true;
                res.Message = "Encargos retrieved successfully.";
                res.Data = groupedResult;
            }
        }
        catch (Exception ex)
        {
            res.Message = $"Error: {ex.Message} {ex.InnerException}";
            res.Ok = false;
        }

        return res;
    }


    [HttpPost]
    [Route("AddEncargo")]
    public async Task<Respuesta> AddEncargo(int Cantidad, int ArticuloId, int VisitaId)
    {
        Respuesta res = new Respuesta();

        try
        {
            // Check if the Articulo exists
            var articulo = await _db.Articulos.FindAsync(ArticuloId);
            if (articulo == null)
            {
                res.Message = $"Articulo with ID {ArticuloId} not found.";
                res.Ok = false;
                return res;
            }

            // Validate VisitaId
            var visita = await _db.Visitas.FindAsync(VisitaId);
            if (visita == null)
            {
                res.Message = $"Habitacion with ID {VisitaId} not found.";
                res.Ok = false;
                return res;
            }

            //// Check if an Inventario entry already exists for the given ArticuloId and VisitaId
            //var existingInventario = await _db.Encargos
            //    .FirstOrDefaultAsync(i => i.ArticuloId == ArticuloId && i.VisitaId == VisitaId && i.Entregado==0);

            //if (existingInventario != null)
            //{
            //    res.Message = "An existing Inventario entry found. Please use UpdateStock method to update the stock.";
            //    res.Ok = false;
            //    return res;
            //}

            // Create new Encargo entity
            var newEncargo = new Encargos
            {
                ArticuloId = ArticuloId,
                VisitaId = VisitaId,
                CantidadArt = Cantidad,
                FechaCrea = DateTime.Now,
                Anulado = false // Default to not annulled
            };

            // Add the Encargo entity to the DbContext
            await _db.Encargos.AddAsync(newEncargo);
            await _db.SaveChangesAsync();

            // Set response on success
            res.Ok = true;
            res.Message = "Encargo added successfully.";
            res.Data = newEncargo; 
        }
        catch (Exception ex)
        {
            res.Message = $"Error: {ex.Message} {ex.InnerException}";
            res.Ok = false;
        }

        return res;
    }

    [HttpPost]
    [Route("AddEncargos")]
    public async Task<Respuesta> AddEncargos(List<EncargoRequestDTO> encargos, [FromServices] IHubContext<NotificationsHub> hubContext)
    {
        Respuesta res = new Respuesta();
        List<Encargos> addedEncargos = new List<Encargos>();

        try
        {
            foreach (var EncargoRequestDTO in encargos)
            {
                // Validar si el Articulo existe
                var articulo = await _db.Articulos.FindAsync(EncargoRequestDTO.ArticuloId);
                if (articulo == null)
                {
                    res.Message = $"Articulo con ID {EncargoRequestDTO.ArticuloId} no encontrado.";
                    res.Ok = false;
                    return res;
                }

                // Validar si la Visita existe
                var visita = await _db.Visitas.FindAsync(EncargoRequestDTO.VisitaId);
                if (visita == null)
                {
                    res.Message = $"Visita con ID {EncargoRequestDTO.VisitaId} no encontrada.";
                    res.Ok = false;
                    return res;
                }

                // Mapear DTO a entidad Encargos
                var newEncargo = new Encargos
                {
                    ArticuloId = EncargoRequestDTO.ArticuloId,
                    VisitaId = EncargoRequestDTO.VisitaId,
                    CantidadArt = EncargoRequestDTO.Cantidad,
                    Comentario = EncargoRequestDTO.Comentario,
                    FechaCrea = DateTime.Now,
                    Anulado = false
                };

                // Añadir la entidad Encargo al DbContext
                await _db.Encargos.AddAsync(newEncargo);
                addedEncargos.Add(newEncargo);
            }

            // Guardar todos los cambios en la base de datos
            await _db.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("ReceiveNotification", "¡Hay una nueva orden!");

            // Configurar la respuesta en caso de éxito
            res.Ok = true;
            res.Message = "Encargos agregados exitosamente.";
            res.Data = addedEncargos;
        }
        catch (Exception ex)
        {
            res.Message = $"Error: {ex.Message} {ex.InnerException}";
            res.Ok = false;
        }

        return res;
    }

    [HttpPost]
    [Route("EntregarEncargo")]
    public async Task<Respuesta> EntregarEncargo(int encargoId)
    {
        Respuesta res = new Respuesta();

        try
        {
            // Buscar el encargo por ID
            var encargo = await _db.Encargos.FindAsync(encargoId);

            // Validar si el encargo existe
            if (encargo == null)
            {
                res.Message = $"Encargo con ID {encargoId} no encontrado.";
                res.Ok = false;
                return res;
            }

            // Actualizar el estado de 'Entregado' a true
            encargo.Entregado = true;

            // Guardar los cambios en la base de datos
            await _db.SaveChangesAsync();

            // Configurar la respuesta en caso de éxito
            res.Ok = true;
            res.Message = $"Encargo con ID {encargoId} marcado como entregado.";
            res.Data = encargo;
        }
        catch (Exception ex)
        {
            res.Message = $"Error: {ex.Message} {ex.InnerException}";
            res.Ok = false;
        }

        return res;
    }

    [HttpPost]
    [Route("AnularEncargo")]
    public async Task<Respuesta> AnularEncargo(int encargoId)
    {
        Respuesta res = new Respuesta();

        try
        {
            // Buscar el encargo por ID
            var encargo = await _db.Encargos.FindAsync(encargoId);

            // Validar si el encargo existe
            if (encargo == null)
            {
                res.Message = $"Encargo con ID {encargoId} no encontrado.";
                res.Ok = false;
                return res;
            }

            encargo.Anulado = true;

            // Guardar los cambios en la base de datos
            await _db.SaveChangesAsync();

            // Configurar la respuesta en caso de éxito
            res.Ok = true;
            res.Message = $"Encargo con ID {encargoId} marcado como anulado.";
            res.Data = encargo;
        }
        catch (Exception ex)
        {
            res.Message = $"Error: {ex.Message} {ex.InnerException}";
            res.Ok = false;
        }

        return res;
    }

    // PUT: api/Encargos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEncargo(int id, Encargos encargo)
    {
        if (id != encargo.EncargosId)
        {
            return BadRequest();
        }

        _db.Entry(encargo).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EncargoExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    private bool EncargoExists(int id)
    {
        return _db.Encargos.Any(e => e.EncargosId == id);
    }
}
