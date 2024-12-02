using ApiObjetos.Data;
using ApiObjetos.Models;
using ApiObjetos.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApiObjetos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromocionesController : ControllerBase
    {
        private readonly HotelDbContext _db;

        public PromocionesController(HotelDbContext context)
        {
            _db = context;
        }

        [HttpPost]
        [Route("AddPromocion")]
        public async Task<Respuesta> AddPromocion(double tarifa, int cantidadHoras, int categoriaID)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Validate CategoriaID
                var categoria = await _db.CategoriasHabitaciones.FindAsync(categoriaID);
                if (categoria == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró la categoría con ID: {categoriaID}.";
                    return res;
                }

                // Create new Promocion
                var newPromocion = new Promociones
                {
                    Tarifa = tarifa,
                    CantidadHoras = cantidadHoras,
                    CategoriaID = categoriaID
                };

                // Add to the DbContext
                await _db.Promociones.AddAsync(newPromocion);
                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Promoción agregada exitosamente.";
                res.Data = newPromocion;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }

        [HttpGet]
        [Route("GetPromocion")]
        public async Task<Respuesta> GetPromocion(int promocionID)
        {
            Respuesta res = new Respuesta();

            try
            {
                var promocion = await _db.Promociones.FindAsync(promocionID);
                if (promocion == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró la promoción con ID: {promocionID}.";
                    return res;
                }

                res.Ok = true;
                res.Message = "Promoción encontrada.";
                res.Data = promocion;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }

        [HttpGet]
        [Route("GetAllPromociones")]
        public async Task<Respuesta> GetAllPromociones()
        {
            Respuesta res = new Respuesta();

            try
            {
                var promociones = await _db.Promociones.ToListAsync();
                if (promociones == null || promociones.Count == 0)
                {
                    res.Ok = false;
                    res.Message = "No se encontraron promociones.";
                }
                else
                {
                    res.Ok = true;
                    res.Message = "Promociones encontradas.";
                    res.Data = promociones;
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
        [Route("UpdatePromocion")]
        public async Task<Respuesta> UpdatePromocion(int promocionID, double tarifa, int cantidadHoras, int categoriaID)
        {
            Respuesta res = new Respuesta();

            try
            {
                var promocion = await _db.Promociones.FindAsync(promocionID);
                if (promocion == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró la promoción con ID: {promocionID}.";
                    return res;
                }

                // Validate CategoriaID
                var categoria = await _db.CategoriasHabitaciones.FindAsync(categoriaID);
                if (categoria == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró la categoría con ID: {categoriaID}.";
                    return res;
                }

                // Update fields
                promocion.Tarifa = tarifa;
                promocion.CantidadHoras = cantidadHoras;
                promocion.CategoriaID = categoriaID;

                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Promoción actualizada exitosamente.";
                res.Data = promocion;
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }

        [HttpDelete]
        [Route("DeletePromocion")]
        public async Task<Respuesta> DeletePromocion(int promocionID)
        {
            Respuesta res = new Respuesta();

            try
            {
                var promocion = await _db.Promociones.FindAsync(promocionID);
                if (promocion == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró la promoción con ID: {promocionID}.";
                    return res;
                }

                _db.Promociones.Remove(promocion);
                await _db.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Promoción eliminada exitosamente.";
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Message = $"Error: {ex.Message}";
            }

            return res;
        }
    }
}
