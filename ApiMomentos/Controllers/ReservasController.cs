﻿using ApiObjetos.Models.Sistema;
using ApiObjetos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ApiObjetos.Data;
using Microsoft.EntityFrameworkCore;
namespace ApiObjetos.Controllers
{
    public class ReservasController : Controller
    {
        private readonly HotelDbContext _db;
        private readonly MovimientosController _movimiento;
        private readonly VisitasController _visita;

        public ReservasController(HotelDbContext db, IConfiguration configuration)
        {
            _db = db;
            _movimiento = new MovimientosController(db);
            _visita =  new VisitasController(db);

        }
        #region Reservas
        /*    [HttpPost]
            [Route("ReservarHabitacion")]
            [AllowAnonymous]
            public async Task<Respuesta> ReservarHabitacion([FromBody] ReservaRequest request)
            {
                Respuesta res = new Respuesta();
                try
                {
                    // Step 1: Create a new VisitaID based on the provided vehicle, phone number, and identifier
                    var VisitaID = await _visita.CrearVisita(request.EsReserva, request.PatenteVehiculo, request.NumeroTelefono, request.Identificador);

                    // Step 2: Retrieve the room details
                    var habitacion = await GetHabitacionById(request.HabitacionID);
                    if (habitacion == null)
                    {
                        res.Message = "Habitación no encontrada.";
                        res.Ok = false;
                        return res;
                    }

                    // Step 3: Retrieve free time slots for the specified day
                    var horariosLibres = await HorariosLibres(request.HabitacionID, request.FechaReserva.Date);

                    // Step 4: Check if the requested reservation time falls within any of the free slots
                    bool isValidReservation = horariosLibres.Any(horario =>
                        request.FechaReserva.TimeOfDay >= horario.Start && request.FechaFin.TimeOfDay <= horario.End);

                    if (!isValidReservation)
                    {
                        res.Message = "El horario solicitado no está disponible.";
                        res.Ok = false;
                        return res;
                    }

                    // Step 5: If the reservation is valid, proceed with creating the reservation
                    Reservas nuevaReserva = new Reservas
                    {
                        VisitaId = VisitaID,
                        HabitacionId = request.HabitacionID,
                        FechaReserva = request.FechaReserva,
                        FechaFin = request.FechaFin,
                        TotalHoras = request.TotalHoras,
                        UsuarioId = request.UsuarioID,
                        FechaRegistro = DateTime.Now,
                        Anulado = false,
                        Habitacion = habitacion
                    };

                    _db.Add(nuevaReserva);
                    await _movimiento.CrearMovimientoHabitacion(VisitaID, (int)habitacion.Categoria.PrecioNormal, request.HabitacionID, habitacion);
                    await _db.SaveChangesAsync();

                    res.Message = "La reserva se grabó correctamente";
                    res.Ok = true;
                }
                catch (Exception ex)
                {
                    res.Message = $"Error: {ex.Message} {ex.InnerException}";
                    res.Ok = false;
                }

                return res;
            }
        */

        [HttpPost]
        [Route("ReservarHabitacion")]
        [AllowAnonymous]
        public async Task<Respuesta> ReservarHabitacion([FromBody] ReservaRequest request)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Step 1: Create a new VisitaID based on the provided vehicle, phone number, and identifier
                var VisitaID = await _visita.CrearVisita(request.EsReserva, request.PatenteVehiculo, request.NumeroTelefono, request.Identificador);

                // Step 2: Retrieve the room details
                Habitaciones habitacion = await GetHabitacionById(request.HabitacionID);
                if (habitacion == null)
                {
                    res.Message = "Habitación no encontrada.";
                    res.Ok = false;
                    return res;
                }

                // Step 3: Check if the room is available for reservation
                if (habitacion.Disponible == false)
                {
                    res.Message = "La habitación no está disponible.";
                    res.Ok = false;
                    return res;
                }

                // Step 4: If the room is available, proceed with creating the reservation
                Reservas nuevaReserva = new Reservas
                {
                    VisitaId = VisitaID,
                    HabitacionId = request.HabitacionID,
                    FechaReserva = request.FechaReserva,
                    FechaFin = null,
                    TotalHoras = request.TotalHoras,
                    TotalMinutos = request.TotalMinutos,
                    UsuarioId = request.UsuarioID,
                    FechaRegistro = DateTime.Now,
                    FechaAnula = null,
                    Habitacion = habitacion
                };

                decimal? tarifa = habitacion.Categoria.PrecioNormal; // Default price
                if (request.PromocionID != null && request.PromocionID != 0)
                {
                    nuevaReserva.PromocionId = request.PromocionID;
                    // Fetch the promotion from the database
                    var promocion = await _db.Promociones.FindAsync(request.PromocionID);
                    if (promocion == null)
                    {
                        res.Message = "La promoción no es válida.";
                        res.Ok = false;
                        return res;
                    }
                    tarifa = promocion.Tarifa; // Use the promotional rate
                }
                else nuevaReserva.PromocionId = null;
                var prueba = Math.Round((decimal)((int)tarifa * (request.TotalHoras + (request.TotalMinutos / 60.0))), 2);
                _db.Add(nuevaReserva);
                var movimientoID = await _movimiento.CrearMovimientoHabitacion(VisitaID,
                Math.Round((decimal)((int)tarifa * (request.TotalHoras + (request.TotalMinutos / 60.0))), 2), request.HabitacionID);
                nuevaReserva.MovimientoId = movimientoID;
                await _db.SaveChangesAsync();

                // Step 5: Update the room's availability and set the current VisitaID
                habitacion.Disponible = false;
                habitacion.VisitaID = VisitaID;
                await _db.SaveChangesAsync();

                res.Message = "La reserva se grabó correctamente";
                res.Ok = true;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message} {ex.InnerException}";
                res.Ok = false;
            }

            return res;
        }

        [HttpDelete]
        [Route("AnularOcupacion")]
        [AllowAnonymous]
        public async Task<Respuesta> AnularOcupacion(int reservaId, string motivo)
        {
            Respuesta res = new Respuesta();
            if (motivo.Length > 150)
            {
                res.Message = "Motivo muy largo.";
                res.Ok = false;
                return res;
            }
            try
            {
                var reserva = await _db.Reservas.FindAsync(reservaId);
                var habitacion = await _db.Habitaciones.FirstAsync(h => h.VisitaID == reserva.VisitaId);
                if (reserva != null && reserva.FechaAnula == null)
                {
                    var Movimientos = await _db.Movimientos.Where(m => m.VisitaId == reserva.VisitaId).ToListAsync();
                    foreach (var movimiento in Movimientos)
                    {
                        var Consumos = await _db.Consumo.Where(c => c.MovimientosId == movimiento.MovimientosId).ToListAsync();
                        foreach(var consumo in Consumos)
                        {
                            consumo.Anulado = true;
                            if (consumo.EsHabitacion == true) { 
                                var inventario = await _db.Inventarios.FirstAsync(i => i.ArticuloId == consumo.ArticuloId && i.HabitacionId == reserva.HabitacionId);
                                inventario.Cantidad += consumo.Cantidad;
                            }
                            else { var inventarioGeneral = await _db.InventarioGeneral.FirstAsync(i => i.ArticuloId == consumo.ArticuloId);
                                inventarioGeneral.Cantidad += consumo.Cantidad;
                            }
                        }
                        movimiento.Anulado = true;
                    }
                    reserva.FechaAnula = DateTime.Now;
                    habitacion.Disponible = true;
                    habitacion.VisitaID = null;
                    var visita = await _db.Visitas.FirstAsync(v => v.VisitaId == reserva.VisitaId);
                    visita.Anulado = true;
                    string formattedDate = DateTime.Now.ToString("d/M/yyyy HH:mm");
                    Registros registro = new Registros();
                    registro.Contenido = "Se anuló la visita a la habitación " + habitacion.NombreHabitacion + " a las " + formattedDate + " por el motivo de: " + motivo;
                    registro.ReservaId = reserva.ReservaId;
                    _db.Registros.Add(registro);
                    await _db.SaveChangesAsync();
                }
                else{
                    res.Message = "Reserva no encontrada.";
                    res.Ok = false;
                    return res;
                }
            }
            catch(Exception ex)
            {
                res.Message = $"Error: {ex.Message} {ex.InnerException}";
                res.Ok = false;
            }
            res.Message = "Reserva anulada";
            res.Ok = true;
            return res;
        }

        [HttpPut]
        [Route("PausarOcupacion")]
        [AllowAnonymous]
        public async Task<Respuesta> PausarOcupacion(int visitaId)
        {
            Respuesta res = new Respuesta();
            var reserva = await _db.Reservas.FirstAsync(r => r.VisitaId == visitaId);
            try
            {
                DateTime fechaReserva = (DateTime)reserva.FechaReserva;
                var fechaActual = (DateTime.Now - TimeSpan.FromHours((double)reserva.TotalHoras) - TimeSpan.FromMinutes((double)reserva.TotalMinutos));
                TimeSpan timer = fechaReserva - fechaActual;
                reserva.PausaHoras = timer.Hours;
                reserva.PausaMinutos = timer.Minutes;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                res.Message = "Error a la hora de pausar el timer " + ex.InnerException;
                res.Ok = false;
                return res;
            }
            res.Message = "El timer se pausó a las " + reserva.PausaHoras + ":" + reserva.PausaMinutos;
            res.Ok = true;
            return res;
        }

        [HttpPut]
        [Route("RecalcularOcupacion")]
        [AllowAnonymous]
        public async Task<Respuesta> RecalcularOcupacion(int visitaId)
        {
            Respuesta res = new Respuesta();
            var reserva = await _db.Reservas.FirstAsync(r => r.VisitaId == visitaId);
            try
            {
                reserva.PausaHoras = null;
                reserva.PausaMinutos = null;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                res.Message = "Error a la hora de pausar el timer " + ex.InnerException;
                res.Ok = false;
                return res;
            }
            res.Message = "El timer se reanudo";
            res.Ok = true;
            return res;
        }
        [HttpPut]
        [Route("ActualizarReservaPromocion")]
        [AllowAnonymous]
        public async Task<Respuesta> ActualizarReservaPromocion(int reservaId, int? promocionId)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Fetch the reservation from the database
                var reserva = await _db.Reservas.FindAsync(reservaId);
                if (reserva == null)
                {
                    res.Message = "La reserva no existe.";
                    res.Ok = false;
                    return res;
                }

                // Fetch the associated Movimiento
                var movimiento = await _db.Movimientos.FindAsync(reserva.MovimientoId);
                if (movimiento == null)
                {
                    res.Message = "El movimiento asociado a la reserva no existe.";
                    res.Ok = false;
                    return res;
                }

                decimal? tarifa;
                decimal? totalFacturado;

                // Step 2: Validate the provided promotion ID, if any
                if (promocionId != null)
                {
                    var promocion = await _db.Promociones.FindAsync(promocionId);
                    if (promocion == null)
                    {
                        res.Message = "La promoción no es válida.";
                        res.Ok = false;
                        return res;
                    }

                    tarifa = promocion.Tarifa;
                    reserva.PromocionId = promocionId; // Update the promotion

                    // Calculate the new total facturado using Reserva's TotalHoras and TotalMinutos
                    var totalHours = reserva.TotalHoras.GetValueOrDefault() + (reserva.TotalMinutos.GetValueOrDefault() / 60m);
                    totalFacturado = promocion.Tarifa * totalHours;
                }
                else
                {
                    var habitacion = await _db.Habitaciones
                        .Include(h => h.Categoria)
                        .FirstOrDefaultAsync(h => h.HabitacionId == reserva.HabitacionId);

                    if (habitacion == null)
                    {
                        res.Message = "La habitación asociada a la reserva no existe.";
                        res.Ok = false;
                        return res;
                    }

                    tarifa = habitacion.Categoria.PrecioNormal;
                    reserva.PromocionId = null; // Clear the promotion

                    // Calculate the new total facturado using Reserva's TotalHoras and TotalMinutos
                    var totalHours = reserva.TotalHoras.GetValueOrDefault() + (reserva.TotalMinutos.GetValueOrDefault() / 60m);
                    totalFacturado = habitacion.Categoria.PrecioNormal * totalHours;
                }

                // Update the Movimiento
                movimiento.TotalFacturado = totalFacturado;

                // Step 3: Save the changes to the database
                await _db.SaveChangesAsync();

                res.Message = "La promoción de la reserva se actualizó correctamente.";
                res.Ok = true;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message} {ex.InnerException?.Message}";
                res.Ok = false;
            }

            return res;
        }


        [HttpGet]
        [Route("GetReserva")]
        [AllowAnonymous]
        public async Task<Respuesta> GetReserva(int idReserva)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Find the reservation by idReserva
                var reserva = await _db.Reservas.FindAsync(idReserva);

                // Check if the reservation exists
                if (reserva != null)
                {
                    res.Data = reserva;
                    res.Message = "Reserva encontrada.";
                    res.Ok = true;
                }
                else
                {
                    res.Message = "No se encontró la reserva.";
                    res.Ok = false;
                }
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message} {ex.InnerException?.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpGet]
        [Route("GetReservas")]
        [AllowAnonymous]
        public async Task<Respuesta> GetReservas(int institucionID)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Retrieve all reservations
                var reservas = await _db.Reservas.Where(r => r.InstitucionID == institucionID).ToListAsync();

                // Check if any reservations exist
                if (reservas.Any())
                {
                    res.Data = reservas;
                    res.Message = "Reservas encontradas.";
                    res.Ok = true;
                }
                else
                {
                    res.Message = "No se encontraron reservas.";
                    res.Ok = false;
                }
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message} {ex.InnerException?.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpGet]
        [Route("GetReservasFuturas")]
        [AllowAnonymous]
        public async Task<Respuesta> GetReservasFuturas()
        {
            Respuesta res = new Respuesta();
            try
            {
                // Get the current date and time
                var now = DateTime.Now;

                // Retrieve all future reservations (from now onwards)
                var futurasReservas = await _db.Reservas
                                               .Where(r => r.FechaReserva >= now)
                                               .ToListAsync();

                // Check if any future reservations exist
                if (futurasReservas.Any())
                {
                    res.Data = futurasReservas;
                    res.Message = "Reservas futuras encontradas.";
                    res.Ok = true;
                }
                else
                {
                    res.Message = "No se encontraron reservas futuras.";
                    res.Ok = false;
                }
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message} {ex.InnerException?.Message}";
                res.Ok = false;
            }

            return res;
        }

        /* [HttpPut]
         [Route("FinalizarReserva")]
         [AllowAnonymous]
         public async Task<Respuesta> FinalizarReserva(int idReserva)
         {
             Respuesta res = new Respuesta();
             try
             {
                 // Find the Reserva by idReserva
                 var reserva = await _db.Reservas.FindAsync(idReserva);

                 // Check if the reservation exists
                 if (reserva != null)
                 {
                     // Update the FechaFin to DateTime.Now
                     reserva.FechaFin = DateTime.Now;

                     // Find the associated Habitacion
                     var habitacion = await _db.Habitaciones.FindAsync(reserva.HabitacionId);
                     if (habitacion != null)
                     {
                         // Update the room's availability to true
                         habitacion.Disponible = true;
                         habitacion.VisitaID = null; 
                     }

                     // Save the changes to the database
                     await _db.SaveChangesAsync();

                     res.Message = "La reserva se finalizó correctamente.";
                     res.Ok = true;
                 }
                 else
                 {
                     res.Message = "La reserva no fue encontrada.";
                     res.Ok = false;
                 }
             }
             catch (Exception ex)
             {
                 res.Message = $"Error: {ex.Message} {ex.InnerException?.Message}";
                 res.Ok = false;
             }

             return res;
         }*/

        [HttpPut]
        [Route("FinalizarReserva")]
        [AllowAnonymous]
        public async Task<Respuesta> FinalizarReserva(int idHabitacion)
        {
            Respuesta res = new Respuesta();
            try
            {
                // Find the Reserva by idReserva
                var habitacion = await _db.Habitaciones.FindAsync(idHabitacion);
                var reserva = await _db.Reservas.FirstOrDefaultAsync(r => r.VisitaId == habitacion.VisitaID);
                // Check if the reservation exists

                if (habitacion != null)
                {
                        
                        // Update the room's availability to true
                        habitacion.Disponible = true;
                        habitacion.VisitaID = null;
                        reserva.FechaFin = DateTime.Now;

                    // Save the changes to the database
                    await _db.SaveChangesAsync();

                    res.Message = "La reserva se finalizó correctamente.";
                    res.Ok = true;
                }
                else
                {
                    res.Message = "La reserva no fue encontrada.";
                    res.Ok = false;
                }
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message} {ex.InnerException?.Message}";
                res.Ok = false;
            }

            return res;
        }

        private async Task<Habitaciones?> GetHabitacionById(int habitacionId)
        {
                var a = await _db.Habitaciones
                .Include(h => h.Categoria) // Include Categoria if needed
                .FirstOrDefaultAsync(h => h.HabitacionId == habitacionId);

            return a;
        }

        public async Task<decimal?> ObtenerPrecioNormal(int reservaId)
        {
            // Fetch the Reserva along with related Habitacion and Categoria
            var reserva = await _db.Reservas
                .Include(r => r.Habitacion) // Load related Habitacion
                .ThenInclude(h => h.Categoria) // Load related Categoria
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId);

            // Check if the reserva, Habitacion, or Categoria is null
            if (reserva == null || reserva.Habitacion == null || reserva.Habitacion.Categoria == null)
            {
                return null; // Or handle the null case as needed
            }

            // Return the PrecioNormal from Categoria
            return reserva.Habitacion.Categoria.PrecioNormal;
        }

        [HttpGet]
        [Route("GetHorariosLibres")]
        [AllowAnonymous]
        public async Task<Respuesta> GetHorariosLibres(int idHabitacion, DateTime dia)
        {
            Respuesta res = new Respuesta();
            try
            {
                var horariosLibres = await HorariosLibres(idHabitacion, dia);

                res.Message = "Este es/son los rangos de horario libres";
                res.Ok = true;
                res.Data = horariosLibres;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }

        [HttpGet]
        [Route("HabitacionesOcupadas")]
        [AllowAnonymous]
        public async Task HabitacionesOcupadas()
        {
            // Get the current date and time
            DateTime now = DateTime.Now;
            DateTime yesterday = now.AddDays(-1); // The start of the next day

            // Fetch rooms where the reservation has ended today
            var habitacionesDesocupadas = await _db.Reservas
                .Where(r => r.FechaReserva <= now && r.FechaRegistro >= yesterday && r.FechaFin <= now) // Reservations that have ended by now
                .Select(r => r.Habitacion)
                .ToListAsync();

            // Update the availability of rooms where the reservation has ended
            foreach (var habitacion in habitacionesDesocupadas)
            {
                habitacion.Disponible = true;
                _db.Habitaciones.Update(habitacion);
            }

            // Fetch rooms where the reservation is still ongoing (includes multi-day reservations)
            var habitacionesReservadas = await _db.Reservas
                .Where(r => r.FechaReserva <= now && r.FechaFin >= now) // Active reservations, including those that cross midnight
                .Select(r => r.Habitacion)
                .ToListAsync();

            // Update the availability of rooms that are currently reserved
            foreach (var habitacion in habitacionesReservadas)
            {
                habitacion.Disponible = false;
                _db.Habitaciones.Update(habitacion);
            }

            // Save changes to the database
            await _db.SaveChangesAsync();
        }

        private async Task<List<TimeRange>> HorariosLibres(int idHabitacion, DateTime dia)
        {
            // Set start and end time based on the specific date
            TimeSpan dayStart = new TimeSpan(0, 0, 0); // Start at 00:00 on the given day
            TimeSpan dayEnd = new TimeSpan(23, 59, 59); // End at 23:59 on the given day
            TimeSpan minGap = TimeSpan.FromMinutes(30);

            // Fetch reservations that might affect the availability on the given day
            var reservas = await _db.Reservas
                                    .Where(r => r.HabitacionId == idHabitacion &&
                                                r.FechaReserva.Value.Date <= dia.Date &&
                                                r.FechaFin.Value.Date >= dia.Date)
                                    .Select(r => new {
                                        FechaReserva = r.FechaReserva.Value,
                                        FechaFin = r.FechaFin.Value
                                    })
                                    .OrderBy(r => r.FechaReserva)
                                    .ToListAsync();

                List<TimeRange> availablePeriods = new List<TimeRange>();

            if (!reservas.Any())
            {
                // If no reservations, the whole day is free
                availablePeriods.Add(new TimeRange { Start = dayStart, End = dayEnd });
                return availablePeriods;
            }

            // Adjust reservations that span across the given day
            var adjustedReservas = new List<(TimeSpan Start, TimeSpan End)>();
            foreach (var reserva in reservas)
            {
                var reservaStart = reserva.FechaReserva.TimeOfDay;
                var reservaEnd = reserva.FechaFin.TimeOfDay;

                if ((reserva.FechaFin > reserva.FechaReserva.Date ) && (reserva.FechaFin.Date > reserva.FechaReserva.Date))
                {
                    // Reservation spans across days, adjust times for the specific day
                    if (reserva.FechaReserva.Date == dia.Date)
                    {
                        adjustedReservas.Add((reservaStart, dayEnd));
                    }
                    else if (reserva.FechaFin.Date == dia.Date)
                    {
                        adjustedReservas.Add((dayStart, reservaEnd));
                    }
                }
                else
                {
                    adjustedReservas.Add((reservaStart, reservaEnd));
                }
            }

            // Determine free time ranges on the specific day
            TimeSpan lastEnd = dayStart;
            foreach (var (reservaStart, reservaEnd) in adjustedReservas.OrderBy(r => r.Start))
            {
                if (reservaStart - lastEnd >= minGap)
                {
                    availablePeriods.Add(new TimeRange { Start = lastEnd, End = reservaStart });
                }
                lastEnd = reservaEnd > lastEnd ? reservaEnd : lastEnd;
            }

            // Check the time after the last reservation
            if (dayEnd - lastEnd >= minGap)
            {
                availablePeriods.Add(new TimeRange { Start = lastEnd, End = dayEnd });
            }

            return availablePeriods;
        }
        #endregion

    }
}

public class TimeRange
{
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
}

public class ReservaRequest
{
    public int HabitacionID { get; set; }
    public int? PromocionID { get; set; }
    public DateTime FechaReserva { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalHoras { get; set; }
    public int TotalMinutos { get; set; }
    public int UsuarioID { get; set; }
    public bool EsReserva { get; set; }
    public string? PatenteVehiculo { get; set; }
    public string? NumeroTelefono { get; set; }
    public string? Identificador { get; set; }
}