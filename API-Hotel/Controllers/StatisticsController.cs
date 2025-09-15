using hotel.DTOs;
using hotel.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Obsolete("This controller is deprecated. Use V1/StatisticsController instead.")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService =
                statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        }

        [HttpPost("Rankinghabitaciones")]
        [Obsolete("This endpoint is deprecated. Use POST /api/v1/statistics/room-ranking instead.")]
        public async Task<IActionResult> GetRoomRanking([FromBody] DateRangeDto dateRange)
        {
            if (
                dateRange == null
                || dateRange.FechaInicio > dateRange.FechaFin
                || dateRange.InstitucionID <= 0
            )
                return BadRequest(
                    "Fecha de inicio debe ser menor o igual a fecha de fin y el InstitucionID debe ser válido."
                );

            var result = await _statisticsService.GetRoomRankingAsync(dateRange);
            return Ok(result);
        }

        [HttpPost("MonetizacionHabitaciones")]
        [Obsolete("This endpoint is deprecated. Use POST /api/v1/statistics/room-revenue instead.")]
        public async Task<IActionResult> GetRoomRevenue([FromBody] DateRangeDto dateRange)
        {
            if (
                dateRange == null
                || dateRange.FechaInicio > dateRange.FechaFin
                || dateRange.InstitucionID <= 0
            )
                return BadRequest(
                    "Fecha de inicio debe ser menor o igual a fecha de fin y el InstitucionID debe ser válido."
                );

            var result = await _statisticsService.GetRoomRevenueAsync(dateRange);
            return Ok(result);
        }

        [HttpPost("TasaOcupacion")]
        [Obsolete("This endpoint is deprecated. Use POST /api/v1/statistics/category-occupancy instead.")]
        public async Task<IActionResult> GetCategoryOccupancy([FromBody] DateRangeDto dateRange)
        {
            if (
                dateRange == null
                || dateRange.FechaInicio > dateRange.FechaFin
                || dateRange.InstitucionID <= 0
            )
                return BadRequest(
                    "Fecha de inicio debe ser menor o igual a fecha de fin y el InstitucionID debe ser válido."
                );

            var result = await _statisticsService.GetCategoryOccupancyAsync(dateRange);
            return Ok(result);
        }

        [HttpPost("ConsumosAsociados")]
        [Obsolete("This endpoint is deprecated. Use POST /api/v1/statistics/room-consumption instead.")]
        public async Task<IActionResult> GetRoomConsumption([FromBody] DateRangeDto dateRange)
        {
            if (
                dateRange == null
                || dateRange.FechaInicio > dateRange.FechaFin
                || dateRange.InstitucionID <= 0
            )
                return BadRequest(
                    "Fecha de inicio debe ser menor o igual a fecha de fin y el InstitucionID debe ser válido."
                );

            var result = await _statisticsService.GetRoomConsumptionAsync(dateRange);
            return Ok(result);
        }
    }
}

