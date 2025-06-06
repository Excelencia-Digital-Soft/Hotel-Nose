using ApiObjetos.DTOs;
using ApiObjetos.Interfaces;
using ApiObjetos.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiObjetos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        }

        [HttpPost("Rankinghabitaciones")]
        public async Task<IActionResult> GetRoomRanking([FromBody] DateRangeDto dateRange)
        {
            if (dateRange == null || dateRange.FechaInicio > dateRange.FechaFin || dateRange.InstitucionID <= 0)
                return BadRequest("Fecha de inicio debe ser menor o igual a fecha de fin y el InstitucionID debe ser válido.");

            var result = await _statisticsService.GetRoomRankingAsync(dateRange);
            return Ok(result);
        }

        [HttpPost("MonetizacionHabitaciones")]
        public async Task<IActionResult> GetRoomRevenue([FromBody] DateRangeDto dateRange)
        {
            if (dateRange == null || dateRange.FechaInicio > dateRange.FechaFin || dateRange.InstitucionID <= 0)
                return BadRequest("Fecha de inicio debe ser menor o igual a fecha de fin y el InstitucionID debe ser válido.");

            var result = await _statisticsService.GetRoomRevenueAsync(dateRange);
            return Ok(result);
        }

        [HttpPost("TasaOcupacion")]
        public async Task<IActionResult> GetCategoryOccupancy([FromBody] DateRangeDto dateRange)
        {
            if (dateRange == null || dateRange.FechaInicio > dateRange.FechaFin || dateRange.InstitucionID <= 0)
                return BadRequest("Fecha de inicio debe ser menor o igual a fecha de fin y el InstitucionID debe ser válido.");

            var result = await _statisticsService.GetCategoryOccupancyAsync(dateRange);
            return Ok(result);
        }

        [HttpPost("ConsumosAsociados")]
        public async Task<IActionResult> GetRoomConsumption([FromBody] DateRangeDto dateRange)
        {
            if (dateRange == null || dateRange.FechaInicio > dateRange.FechaFin || dateRange.InstitucionID <= 0)
                return BadRequest("Fecha de inicio debe ser menor o igual a fecha de fin y el InstitucionID debe ser válido.");

            var result = await _statisticsService.GetRoomConsumptionAsync(dateRange);
            return Ok(result);
        }
    }
}