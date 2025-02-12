using ApiObjetos.Models.Sistema;
using ApiObjetos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ApiObjetos.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiObjetos.Controllers
{
    public class VisitasController
    {
        private readonly HotelDbContext _db;

        public VisitasController(HotelDbContext db)
        {
            _db = db;
        }


        public async Task<int> CrearVisita(bool esReserva, string? PatenteVehiculo, string? NumeroTelefono, string? Identificador)
        {
            Visitas nuevaVisita;

            if (esReserva == false) {
                nuevaVisita = new Visitas
                {
                    PatenteVehiculo = PatenteVehiculo,
                    NumeroTelefono = NumeroTelefono,
                    Identificador = Identificador,
                    FechaPrimerIngreso = DateTime.Now

                };
            }
            else nuevaVisita = new Visitas
            {
                PatenteVehiculo = PatenteVehiculo,
                NumeroTelefono = NumeroTelefono,
                Identificador = Identificador
            };
            _db.Add(nuevaVisita);

                await _db.SaveChangesAsync();



            return nuevaVisita.VisitaId;
        }

      /*  [HttpPost]
        [Route("CrearVisita")] // Crea un nuevo paciente
        [AllowAnonymous]

        public async Task<Respuesta> CrearVisita(string? PatenteVehiculo, string NumeroTelefono)
        {
            Respuesta res = new Respuesta();
            try
            {
                Visita nuevaVisita = new Visita
                {

                    PatenteVehiculo = PatenteVehiculo,
                    NumeroTelefono = NumeroTelefono,
                    FechaPrimerIngreso = DateTime.Now
                };

                _db.Add(nuevaVisita);

                await _db.SaveChangesAsync();

                res.Message = "La visita se creó correctamente";
                res.Ok = true;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        } */


        [HttpGet]
        [Route("GetVisita")] // Obtiene un paciente basado en su idPaciente. Se obtiene la lista de los idPaciente con el metodo GetPacientes
        [AllowAnonymous]

        public async Task<Respuesta> GetVisita(int id)
        {
            Respuesta res = new Respuesta();
            try
            {

                var Objeto = await _db.Visitas.Where(
                t => t.VisitaId == id
                ).Include(r => r.Reservas).ToListAsync();
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
        [Route("GetVisitas")] // Obtiene un paciente basado en su idPaciente. Se obtiene la lista de los idPaciente con el metodo GetPacientes
        [AllowAnonymous]

        public async Task<Respuesta> GetVisitas(int institucionID)
        {
            Respuesta res = new Respuesta();
            try
            {

                var Objeto = await _db.Visitas.Where(v => v.InstitucionID == institucionID).ToListAsync();
                res.Ok = true;
                res.Data = Objeto;
                return res;



            }
            catch (Exception ex)
            {
                res.Message = ex.ToString();
                res.Ok = false;
            }
            return res;
        }

   
        [HttpDelete]
        [Route("AnularVisita")] // Encuentra el ID del paciente para luego eliminarlo
        [AllowAnonymous]
        public async Task<Respuesta> AnularVisita(int id, bool Estado)
        {
            Respuesta res = new Respuesta();
            try
            {
                var visita = await _db.Visitas.FindAsync(id);

                if (visita == null)
                {
                    res.Ok = false;
                    res.Message = $"La visita con el id {id} no se encontró.";
                }
                else
                {
                    visita.Anulado = Estado;
                    await _db.SaveChangesAsync();

                    res.Ok = true;
                    res.Message = $"Se cambió el estado correctamente";
                }
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Ocurrió un error: {ex.Message}";
            }

            return res;
        }
    }
} 
