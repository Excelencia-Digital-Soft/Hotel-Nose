using hotel.DTOs;
using hotel.DTOs.Common;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace hotel.Controllers.V1;

/// <summary>
/// API V1 controller for hotel statistics and analytics
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v1/statistics")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(IStatisticsService statisticsService, ILogger<StatisticsController> logger)
    {
        _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Health check endpoint to verify the controller is working
    /// </summary>
    /// <returns>Simple health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        _logger.LogInformation("StatisticsController V1 Health check called");
        return Ok(new 
        { 
            service = "StatisticsService V1", 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Gets room usage ranking statistics for a date range
    /// </summary>
    /// <param name="dateRange">Date range and institution filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of rooms ranked by total reservations</returns>
    /// <response code="200">Returns room ranking statistics</response>
    /// <response code="400">Invalid date range or missing institution ID</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("room-ranking")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoomRankingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoomRankingDto>>>> GetRoomRanking(
        [FromBody] DateRangeDto dateRange,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("StatisticsController V1 - GetRoomRanking called");
        try
        {
            if (dateRange == null)
            {
                return BadRequest(ApiResponse<IEnumerable<RoomRankingDto>>.Failure(
                    "Date range information is required"));
            }

            if (dateRange.FechaInicio > dateRange.FechaFin)
            {
                return BadRequest(ApiResponse<IEnumerable<RoomRankingDto>>.Failure(
                    "Start date must be less than or equal to end date"));
            }

            if (dateRange.InstitucionID <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<RoomRankingDto>>.Failure(
                    "Institution ID must be a valid positive number"));
            }

            var result = await _statisticsService.GetRoomRankingAsync(dateRange);
            
            _logger.LogInformation("Retrieved room ranking statistics for institution {InstitucionId} from {Start} to {End}",
                dateRange.InstitucionID, dateRange.FechaInicio, dateRange.FechaFin);

            return Ok(ApiResponse<IEnumerable<RoomRankingDto>>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving room ranking statistics");
            return StatusCode(500, ApiResponse<IEnumerable<RoomRankingDto>>.Failure(
                "An error occurred while retrieving room ranking statistics"));
        }
    }

    /// <summary>
    /// Gets room revenue statistics including reservations and consumption income
    /// </summary>
    /// <param name="dateRange">Date range and institution filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of rooms with detailed revenue breakdown</returns>
    /// <response code="200">Returns room revenue statistics</response>
    /// <response code="400">Invalid date range or missing institution ID</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("room-revenue")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoomRevenueDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoomRevenueDto>>>> GetRoomRevenue(
        [FromBody] DateRangeDto dateRange,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (dateRange == null)
            {
                return BadRequest(ApiResponse<IEnumerable<RoomRevenueDto>>.Failure(
                    "Date range information is required"));
            }

            if (dateRange.FechaInicio > dateRange.FechaFin)
            {
                return BadRequest(ApiResponse<IEnumerable<RoomRevenueDto>>.Failure(
                    "Start date must be less than or equal to end date"));
            }

            if (dateRange.InstitucionID <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<RoomRevenueDto>>.Failure(
                    "Institution ID must be a valid positive number"));
            }

            var result = await _statisticsService.GetRoomRevenueAsync(dateRange);
            
            _logger.LogInformation("Retrieved room revenue statistics for institution {InstitucionId} from {Start} to {End}",
                dateRange.InstitucionID, dateRange.FechaInicio, dateRange.FechaFin);

            return Ok(ApiResponse<IEnumerable<RoomRevenueDto>>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving room revenue statistics");
            return StatusCode(500, ApiResponse<IEnumerable<RoomRevenueDto>>.Failure(
                "An error occurred while retrieving room revenue statistics"));
        }
    }

    /// <summary>
    /// Gets category occupancy rate statistics
    /// </summary>
    /// <param name="dateRange">Date range and institution filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of room categories with occupancy rates</returns>
    /// <response code="200">Returns category occupancy statistics</response>
    /// <response code="400">Invalid date range or missing institution ID</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("category-occupancy")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryOccupancyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryOccupancyDto>>>> GetCategoryOccupancy(
        [FromBody] DateRangeDto dateRange,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (dateRange == null)
            {
                return BadRequest(ApiResponse<IEnumerable<CategoryOccupancyDto>>.Failure(
                    "Date range information is required"));
            }

            if (dateRange.FechaInicio > dateRange.FechaFin)
            {
                return BadRequest(ApiResponse<IEnumerable<CategoryOccupancyDto>>.Failure(
                    "Start date must be less than or equal to end date"));
            }

            if (dateRange.InstitucionID <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<CategoryOccupancyDto>>.Failure(
                    "Institution ID must be a valid positive number"));
            }

            var result = await _statisticsService.GetCategoryOccupancyAsync(dateRange);
            
            _logger.LogInformation("Retrieved category occupancy statistics for institution {InstitucionId} from {Start} to {End}",
                dateRange.InstitucionID, dateRange.FechaInicio, dateRange.FechaFin);

            return Ok(ApiResponse<IEnumerable<CategoryOccupancyDto>>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category occupancy statistics");
            return StatusCode(500, ApiResponse<IEnumerable<CategoryOccupancyDto>>.Failure(
                "An error occurred while retrieving category occupancy statistics"));
        }
    }

    /// <summary>
    /// Gets room consumption statistics with detailed breakdown by products
    /// </summary>
    /// <param name="dateRange">Date range and institution filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of rooms with consumption details</returns>
    /// <response code="200">Returns room consumption statistics</response>
    /// <response code="400">Invalid date range or missing institution ID</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("room-consumption")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoomConsumptionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoomConsumptionDto>>>> GetRoomConsumption(
        [FromBody] DateRangeDto dateRange,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (dateRange == null)
            {
                return BadRequest(ApiResponse<IEnumerable<RoomConsumptionDto>>.Failure(
                    "Date range information is required"));
            }

            if (dateRange.FechaInicio > dateRange.FechaFin)
            {
                return BadRequest(ApiResponse<IEnumerable<RoomConsumptionDto>>.Failure(
                    "Start date must be less than or equal to end date"));
            }

            if (dateRange.InstitucionID <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<RoomConsumptionDto>>.Failure(
                    "Institution ID must be a valid positive number"));
            }

            var result = await _statisticsService.GetRoomConsumptionAsync(dateRange);
            
            _logger.LogInformation("Retrieved room consumption statistics for institution {InstitucionId} from {Start} to {End}",
                dateRange.InstitucionID, dateRange.FechaInicio, dateRange.FechaFin);

            return Ok(ApiResponse<IEnumerable<RoomConsumptionDto>>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving room consumption statistics");
            return StatusCode(500, ApiResponse<IEnumerable<RoomConsumptionDto>>.Failure(
                "An error occurred while retrieving room consumption statistics"));
        }
    }
}