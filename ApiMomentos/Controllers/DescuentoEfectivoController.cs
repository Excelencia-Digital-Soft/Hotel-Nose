using ApiObjetos.Data;
using ApiObjetos.Models.Sistema;
using ApiObjetos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiObjetos.Controllers
{
    public class DescuentoEfectivoController
    {
        private readonly HotelDbContext _db;

        public DescuentoEfectivoController(HotelDbContext db)
        {
            _db = db;
        }


        [HttpPost]
        [Route("CrearDescuentoEfectivo")]
        [AllowAnonymous]
        public async Task<Respuesta> CrearDescuentoEfectivo(string Nombre, int Monto)
        {
            Respuesta res = new Respuesta();
            try
            {
                DescuentoEfectivo nuevoDescuento = new DescuentoEfectivo
                {
                    MontoPorcentual = Monto,
                };

                _db.Add(nuevoDescuento);

                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Nuevo descuento creado correctamente.";
                res.Data = nuevoDescuento;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message} {ex.InnerException}";
                res.Ok = false;
            }
            return res;
        }
        [HttpGet]
        [Route("GetDescuentos")]
        public async Task<Respuesta> GetDescuentos()
        {
            Respuesta res = new Respuesta();
            try
            {
                var descuentos = await _db.DescuentoEfectivo.ToListAsync();
                res.Ok = true;
                res.Data = descuentos;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }
            return res;
        }

        [HttpGet]
        [Route("GetDescuentoEfectivo")]
        public async Task<Respuesta> GetDescuento(int institucionID)
        {
            Respuesta res = new Respuesta();
            try
            {
                var descuento = await _db.DescuentoEfectivo.FirstAsync(d => d.InstitucionID == 0);
                if (descuento == null)
                {
                    res.Ok = false;
                    res.Message = "Tarjeta no encontrada.";
                }
                else
                {
                    res.Ok = true;
                    res.Data = descuento;
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
        [Route("UpdateDescuento")]
        public async Task<Respuesta> ActualizarDescuento(int id, int Monto)
        {
            Respuesta res = new Respuesta();
            try
            {
                var descuento = await _db.DescuentoEfectivo.FindAsync(id);
                if (descuento == null)
                {
                    res.Ok = false;
                    res.Message = "Descuento no encontrado.";
                    return res;
                }

                descuento.MontoPorcentual = Monto;
                _db.DescuentoEfectivo.Update(descuento);
                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Tarjeta actualizada correctamente.";
                res.Data = descuento;
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
