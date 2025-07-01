using hotel.Data;
using hotel.Models.Sistema;
using hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotel.Controllers
{
    public class TarjetasController
    {
        private readonly HotelDbContext _db;

        public TarjetasController(HotelDbContext db)
        {
            _db = db;
        }


        [HttpPost]
        [Route("CrearRecargoTarjeta")]
        [AllowAnonymous]
        public async Task<Respuesta> CrearRecargoTarjeta(string Nombre, int Monto, int InstitucionID)
        {
            Respuesta res = new Respuesta();
            try
            {
                Tarjetas nuevaTarjeta = new Tarjetas
                {
                    Nombre = Nombre,
                    MontoPorcentual = Monto,
                    InstitucionID = InstitucionID
                };

                _db.Add(nuevaTarjeta);

                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Nueva tarjeta creada correctamente.";
                res.Data = nuevaTarjeta;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }
            return res;
        }
        [HttpGet]
        [Route("GetTarjetas")]
        public async Task<Respuesta> GetTarjetas(int InstitucionID)
        {
                Respuesta res = new Respuesta();
            try
            {
                var tarjetas = await _db.Tarjetas.Where(t => t.InstitucionID == InstitucionID).ToListAsync();
                res.Ok = true;
                res.Data = tarjetas;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }
            return res;
        }

        // READ (GET BY ID)
        [HttpGet]
        [Route("GetTarjeta")]
        public async Task<Respuesta> GetTarjeta(int id)
        {
            Respuesta res = new Respuesta();
            try
            {
                var tarjeta = await _db.Tarjetas.FindAsync(id);
                if (tarjeta == null)
                {
                    res.Ok = false;
                    res.Message = "Tarjeta no encontrada.";
                }
                else
                {
                    res.Ok = true;
                    res.Data = tarjeta;
                }
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }
            return res;
        }

        // UPDATE (PUT)
        [HttpPut]
        [Route("UpdateTarjeta")]
        public async Task<Respuesta> ActualizarRecargoTarjeta(int id, string? Nombre, int? Monto)
        {
            Respuesta res = new Respuesta();
            try
            {
                var tarjeta = await _db.Tarjetas.FindAsync(id);
                if (tarjeta == null)
                {
                    res.Ok = false;
                    res.Message = "Tarjeta no encontrada.";
                    return res;
                }
                else
                {
                   if(Nombre != null) tarjeta.Nombre = Nombre;
                   if(Monto != null) tarjeta.MontoPorcentual = (int)Monto;
                }
                _db.Tarjetas.Update(tarjeta);
                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Tarjeta actualizada correctamente.";
                res.Data = tarjeta;
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
