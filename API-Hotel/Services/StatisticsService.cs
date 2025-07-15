using hotel.Data;
using hotel.DTOs;
using hotel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly HotelDbContext _context;

        public StatisticsService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomRankingDto>> GetRoomRankingAsync(DateRangeDto dateRange)
        {
            return await _context
                .Reservas.AsNoTracking()
                .Where(r =>
                    r.FechaReserva >= dateRange.FechaInicio
                    && r.FechaReserva <= dateRange.FechaFin
                    && !r.FechaAnula.HasValue
                    && r.InstitucionID == dateRange.InstitucionID
                )
                .Join(
                    _context.Habitaciones,
                    r => r.HabitacionId,
                    h => h.HabitacionId,
                    (r, h) => new { r, h }
                )
                .Join(
                    _context.CategoriasHabitaciones,
                    x => x.h.CategoriaId,
                    c => c.CategoriaId,
                    (x, c) => new { x.h, c }
                )
                .GroupBy(x => new
                {
                    x.h.HabitacionId,
                    x.h.NombreHabitacion,
                    x.c.NombreCategoria,
                })
                .Select(g => new RoomRankingDto
                {
                    HabitacionID = g.Key.HabitacionId,
                    NombreHabitacion = g.Key.NombreHabitacion ?? string.Empty,
                    NombreCategoria = g.Key.NombreCategoria ?? string.Empty,
                    TotalReservas = g.Count(),
                })
                .OrderByDescending(r => r.TotalReservas)
                .ToListAsync();
        }

        public async Task<List<RoomRevenueDto>> GetRoomRevenueAsync(DateRangeDto dateRange)
        {
            var reservaIngresos = await _context
                .Reservas.AsNoTracking()
                .Where(r =>
                    r.FechaReserva >= dateRange.FechaInicio
                    && r.FechaReserva <= dateRange.FechaFin
                    && !r.FechaAnula.HasValue
                    && r.InstitucionID == dateRange.InstitucionID
                )
                .Join(
                    _context.Movimientos,
                    r => r.MovimientoId,
                    m => m.MovimientosId,
                    (r, m) => new { r, m }
                )
                .Join(_context.Pagos, x => x.m.PagoId, p => p.PagoId, (x, p) => new { x.r, p })
                .Join(
                    _context.Habitaciones,
                    x => x.r.HabitacionId,
                    h => h.HabitacionId,
                    (x, h) =>
                        new
                        {
                            x.r,
                            x.p,
                            h,
                        }
                )
                .Join(
                    _context.CategoriasHabitaciones,
                    x => x.h.CategoriaId,
                    c => c.CategoriaId,
                    (x, c) =>
                        new
                        {
                            x.h.HabitacionId,
                            x.h.NombreHabitacion,
                            c.NombreCategoria,
                            Ingresos = (x.p.MontoEfectivo ?? 0)
                                + (x.p.MontoBillVirt ?? 0)
                                + (x.p.MontoTarjeta ?? 0)
                                - x.p.MontoDescuento
                                + (x.p.Adicional ?? 0),
                        }
                )
                .GroupBy(x => new
                {
                    x.HabitacionId,
                    x.NombreHabitacion,
                    x.NombreCategoria,
                })
                .Select(g => new
                {
                    g.Key.HabitacionId,
                    g.Key.NombreHabitacion,
                    g.Key.NombreCategoria,
                    IngresosReservas = g.Sum(x => x.Ingresos),
                })
                .ToListAsync();

            var consumoIngresos = await _context
                .Consumo.AsNoTracking()
                .Where(c => c.MovimientosId.HasValue && (c.Anulado == false))
                .Join(
                    _context.Movimientos,
                    c => c.MovimientosId,
                    m => m.MovimientosId,
                    (c, m) => new { c, m }
                )
                .Join(
                    _context.Reservas,
                    x => x.m.VisitaId,
                    r => r.VisitaId,
                    (x, r) => new { x.c, r }
                )
                .Where(x =>
                    x.r.FechaReserva >= dateRange.FechaInicio
                    && x.r.FechaReserva <= dateRange.FechaFin
                    && !x.r.FechaAnula.HasValue
                    && x.r.InstitucionID == dateRange.InstitucionID
                )
                .Join(
                    _context.Habitaciones,
                    x => x.r.HabitacionId,
                    h => h.HabitacionId,
                    (x, h) => new { x.c, h }
                )
                .Join(
                    _context.CategoriasHabitaciones,
                    x => x.h.CategoriaId,
                    c => c.CategoriaId,
                    (x, c) =>
                        new
                        {
                            x.h.HabitacionId,
                            x.h.NombreHabitacion,
                            c.NombreCategoria,
                            IngresosConsumos = (x.c.PrecioUnitario ?? 0) * (x.c.Cantidad ?? 0),
                        }
                )
                .GroupBy(x => new
                {
                    x.HabitacionId,
                    x.NombreHabitacion,
                    x.NombreCategoria,
                })
                .Select(g => new
                {
                    g.Key.HabitacionId,
                    g.Key.NombreHabitacion,
                    g.Key.NombreCategoria,
                    IngresosConsumos = g.Sum(x => x.IngresosConsumos),
                })
                .ToListAsync();

            return (
                from r in reservaIngresos
                join c in consumoIngresos
                    on new
                    {
                        r.HabitacionId,
                        r.NombreHabitacion,
                        r.NombreCategoria,
                    } equals new
                    {
                        c.HabitacionId,
                        c.NombreHabitacion,
                        c.NombreCategoria,
                    }
                    into consumos
                from c in consumos.DefaultIfEmpty()
                select new RoomRevenueDto
                {
                    HabitacionID = r.HabitacionId,
                    NombreHabitacion = r.NombreHabitacion,
                    NombreCategoria = r.NombreCategoria,
                    IngresosReservas = r.IngresosReservas ?? 0,
                    IngresosConsumos = c?.IngresosConsumos ?? 0,
                    TotalIngresos = (r.IngresosReservas ?? 0) + (c?.IngresosConsumos ?? 0),
                }
            ).OrderByDescending(x => x.TotalIngresos).ToList();
        }

        public async Task<List<CategoryOccupancyDto>> GetCategoryOccupancyAsync(
            DateRangeDto dateRange
        )
        {
            var totalHorasPeriodo = (dateRange.FechaFin - dateRange.FechaInicio).TotalHours;

            var ocupacion = await _context
                .Reservas.AsNoTracking()
                .Where(r =>
                    r.FechaReserva >= dateRange.FechaInicio
                    && r.FechaReserva <= dateRange.FechaFin
                    && !r.FechaAnula.HasValue
                    && r.InstitucionID == dateRange.InstitucionID
                )
                .Join(
                    _context.Habitaciones,
                    r => r.HabitacionId,
                    h => h.HabitacionId,
                    (r, h) => new { r, h }
                )
                .Join(
                    _context.CategoriasHabitaciones,
                    x => x.h.CategoriaId,
                    c => c.CategoriaId,
                    (x, c) =>
                        new
                        {
                            c.CategoriaId,
                            c.NombreCategoria,
                            HorasOcupadas = x.r.TotalHoras ?? 0,
                        }
                )
                .GroupBy(x => new { x.CategoriaId, x.NombreCategoria })
                .Select(g => new
                {
                    g.Key.CategoriaId,
                    g.Key.NombreCategoria,
                    TotalHorasOcupadas = g.Sum(x => x.HorasOcupadas),
                })
                .ToListAsync();

            var totalHabitacionesPorCategoria = await _context
                .Habitaciones.AsNoTracking()
                .Where(h => h.InstitucionID == dateRange.InstitucionID)
                .GroupBy(h => h.CategoriaId)
                .Select(g => new { CategoriaId = g.Key, TotalHabitaciones = g.Count() })
                .ToListAsync();

            return (
                from o in ocupacion
                join t in totalHabitacionesPorCategoria on o.CategoriaId equals t.CategoriaId
                select new CategoryOccupancyDto
                {
                    CategoriaID = o.CategoriaId,
                    NombreCategoria = o.NombreCategoria,
                    TotalHorasOcupadas = o.TotalHorasOcupadas,
                    TasaOcupacion =
                        totalHorasPeriodo > 0
                            ? (o.TotalHorasOcupadas / (t.TotalHabitaciones * totalHorasPeriodo))
                                * 100
                            : 0,
                }
            ).OrderByDescending(x => x.TasaOcupacion).ToList();
        }

        public async Task<List<RoomConsumptionDto>> GetRoomConsumptionAsync(DateRangeDto dateRange)
        {
            var consumos = await _context
                .Consumo.AsNoTracking()
                .Where(c =>
                    c.MovimientosId.HasValue && (c.Anulado == false || c.Anulado == null)
                )
                .Join(
                    _context.Movimientos,
                    c => c.MovimientosId,
                    m => m.MovimientosId,
                    (c, m) => new { c, m }
                )
                .Join(
                    _context.Reservas,
                    x => x.m.VisitaId,
                    r => r.VisitaId,
                    (x, r) => new { x.c, r }
                )
                .Where(x =>
                    x.r.FechaReserva >= dateRange.FechaInicio
                    && x.r.FechaReserva <= dateRange.FechaFin
                    && !x.r.FechaAnula.HasValue
                    && x.r.InstitucionID == dateRange.InstitucionID
                )
                .Join(
                    _context.Habitaciones,
                    x => x.r.HabitacionId,
                    h => h.HabitacionId,
                    (x, h) => new { x.c, h }
                )
                .Join(
                    _context.CategoriasHabitaciones,
                    x => x.h.CategoriaId,
                    c => c.CategoriaId,
                    (x, c) =>
                        new
                        {
                            Consumo = x.c,
                            Habitacion = x.h,
                            Categoria = c,
                        }
                )
                .Join(
                    _context.Articulos,
                    x => x.Consumo.ArticuloId,
                    a => a.ArticuloId,
                    (x, a) =>
                        new
                        {
                            x.Habitacion.HabitacionId,
                            x.Habitacion.NombreHabitacion,
                            x.Categoria.NombreCategoria,
                            x.Consumo.ArticuloId,
                            a.NombreArticulo,
                            x.Consumo.Cantidad,
                            PrecioTotal = (x.Consumo.PrecioUnitario ?? 0)
                                * (x.Consumo.Cantidad ?? 0),
                        }
                )
                .ToListAsync();

            return consumos
                .GroupBy(x => new
                {
                    x.HabitacionId,
                    x.NombreHabitacion,
                    x.NombreCategoria,
                })
                .Select(g => new RoomConsumptionDto
                {
                    HabitacionID = g.Key.HabitacionId,
                    NombreHabitacion = g.Key.NombreHabitacion!,
                    NombreCategoria = g.Key.NombreCategoria!,
                    TotalConsumos = g.Sum(x => x.PrecioTotal),
                    Detalles = g.Select(x => new ConsumptionDetailDto
                        {
                            ArticuloID = x.ArticuloId ?? 0,
                            NombreArticulo = x.NombreArticulo ?? string.Empty,
                            Cantidad = x.Cantidad ?? 0,
                            PrecioTotal = x.PrecioTotal,
                        })
                        .ToList(),
                })
                .OrderByDescending(x => x.TotalConsumos)
                .ToList();
        }
    }
}

