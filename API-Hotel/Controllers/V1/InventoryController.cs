using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Extensions;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers.V1;

/// <summary>
/// V1 API controller for unified inventory management using specialized services
/// Follows Single Responsibility Principle by delegating to focused services
/// </summary>
[ApiController]
[Route("api/v1/inventory")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryCoreService _coreService;
    private readonly IInventoryValidationService _validationService;
    private readonly IInventoryReportingService _reportingService;
    private readonly IInventoryMovementService _movementService;
    private readonly IInventoryAlertService _alertService;
    private readonly IInventoryTransferService _transferService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IInventoryCoreService coreService,
        IInventoryValidationService validationService,
        IInventoryReportingService reportingService,
        IInventoryMovementService movementService,
        IInventoryAlertService alertService,
        IInventoryTransferService transferService,
        ILogger<InventoryController> logger
    )
    {
        _coreService = coreService ?? throw new ArgumentNullException(nameof(coreService));
        _validationService =
            validationService ?? throw new ArgumentNullException(nameof(validationService));
        _reportingService =
            reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        _movementService =
            movementService ?? throw new ArgumentNullException(nameof(movementService));
        _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
        _transferService =
            transferService ?? throw new ArgumentNullException(nameof(transferService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Core Inventory Operations - Using IInventoryCoreService

    /// <summary>
    /// Get all inventory items for the current institution
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetInventory(
        [FromQuery] InventoryLocationType? locationType = null,
        [FromQuery] int? locationId = null,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _coreService.GetInventoryAsync(
            institucionId.Value,
            locationType,
            locationId,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get inventory item by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> GetInventoryById(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _coreService.GetInventoryByIdAsync(
            id,
            institucionId.Value,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found"))
                ? NotFound(result)
                : StatusCode(500, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new inventory item
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> CreateInventory(
        [FromBody] InventoryCreateDto createDto,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = this.GetCurrentUserId();
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _coreService.CreateInventoryAsync(
            createDto,
            institucionId.Value,
            userId,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            // Record initial movement if quantity > 0
            if (createDto.Cantidad > 0)
            {
                await _movementService.CreateMovementAsync(
                    new MovimientoInventarioCreateDto
                    {
                        InventarioId = result.Data!.InventoryId,
                        TipoMovimiento = "Entrada",
                        CantidadAnterior = 0,
                        CantidadNueva = createDto.Cantidad,
                        CantidadCambiada = createDto.Cantidad,
                        Motivo = "Creación de inventario inicial",
                    },
                    institucionId.Value,
                    userId ?? "system",
                    cancellationToken: cancellationToken
                );
            }

            return CreatedAtAction(
                nameof(GetInventoryById),
                new { id = result.Data!.InventoryId },
                result
            );
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Update inventory quantity
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> UpdateInventory(
        int id,
        [FromBody] InventoryUpdateDto updateDto,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = this.GetCurrentUserId();
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        // Get current inventory to calculate changes
        var currentInventory = await _coreService.GetInventoryByIdAsync(
            id,
            institucionId.Value,
            cancellationToken
        );
        if (!currentInventory.IsSuccess)
        {
            return currentInventory.Errors.Any(e => e.Contains("not found"))
                ? NotFound(currentInventory)
                : StatusCode(500, currentInventory);
        }

        var previousQuantity = currentInventory.Data!.Cantidad;

        // Update inventory quantity
        var result = await _coreService.UpdateInventoryQuantityAsync(
            id,
            updateDto.Cantidad,
            institucionId.Value,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            // Record adjustment movement
            await _movementService.RecordAdjustmentAsync(
                id,
                updateDto.Cantidad,
                updateDto.Notes ?? "Ajuste manual de inventario",
                institucionId.Value,
                userId ?? "system",
                cancellationToken: cancellationToken
            );

            // Check and generate alerts
            await _alertService.CheckAndGenerateAlertsAsync(
                id,
                institucionId.Value,
                cancellationToken
            );
        }

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Delete inventory item
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeleteInventory(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _coreService.DeleteInventoryAsync(
            id,
            institucionId.Value,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found"))
                ? NotFound(result)
                : StatusCode(500, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Batch update multiple inventory items
    /// </summary>
    /// <param name="batchUpdateDto">Batch update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update result</returns>
    [HttpPut("batch")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> BatchUpdateInventory(
        [FromBody] InventoryBatchUpdateDto batchUpdateDto,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _coreService.BatchUpdateInventoryAsync(
            batchUpdateDto,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Synchronize general inventory with articles catalog
    /// </summary>
    [HttpPost("general/synchronize")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> SynchronizeGeneralInventory(
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _coreService.SynchronizeGeneralInventoryAsync(
            institucionId.Value,
            cancellationToken
        );
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    #endregion

    #region Stock Validation - Using IInventoryValidationService

    /// <summary>
    /// Validate stock availability
    /// </summary>
    [HttpGet("validate-stock")]
    [ProducesResponseType(typeof(ApiResponse<StockValidationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<StockValidationDto>>> ValidateStock(
        [FromQuery] int articleId,
        [FromQuery] int requestedQuantity,
        [FromQuery] InventoryLocationType locationType,
        [FromQuery] int? locationId = null,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _validationService.ValidateStockAsync(
            articleId,
            requestedQuantity,
            locationType,
            locationId,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get available quantity for an article
    /// </summary>
    [HttpGet("available-quantity")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<int>>> GetAvailableQuantity(
        [FromQuery] int articleId,
        [FromQuery] InventoryLocationType locationType,
        [FromQuery] int? locationId = null,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _validationService.GetAvailableQuantityAsync(
            articleId,
            locationType,
            locationId,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Validate multiple stock items at once
    /// </summary>
    [HttpPost("validate-multiple")]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<StockValidationDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<ApiResponse<IEnumerable<StockValidationDto>>>
    > ValidateMultipleStock(
        [FromBody] IEnumerable<StockValidationRequestDto> requests,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _validationService.ValidateMultipleStockAsync(
            requests,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    #endregion

    #region Reporting and Analytics - Using IInventoryReportingService

    /// <summary>
    /// Get inventory summary by location
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<InventorySummaryDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<ApiResponse<IEnumerable<InventorySummaryDto>>>
    > GetInventorySummary(CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _reportingService.GetInventorySummaryAsync(
            institucionId.Value,
            cancellationToken
        );
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get comprehensive inventory statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<InventoryStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryStatisticsDto>>> GetInventoryStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _reportingService.GetInventoryStatisticsAsync(
            institucionId.Value,
            fromDate,
            toDate,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get items with low stock
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetLowStockItems(
        [FromQuery] int threshold = 5,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _reportingService.GetLowStockItemsAsync(
            institucionId.Value,
            threshold,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get combined inventory view (general + room inventories)
    /// </summary>
    [HttpGet("rooms/{roomId}/combined")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetCombinedInventory(
        int roomId,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _reportingService.GetCombinedInventoryAsync(
            roomId,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    #endregion

    #region Room and General Inventory - Using Core Service

    /// <summary>
    /// Get all inventory items for a specific room
    /// </summary>
    [HttpGet("rooms/{roomId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetRoomInventory(
        int roomId,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _coreService.GetInventoryAsync(
            institucionId.Value,
            InventoryLocationType.Room,
            roomId,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Add inventory item to a room
    /// </summary>
    [HttpPost("rooms/{roomId}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> AddRoomInventory(
        int roomId,
        [FromBody] InventoryCreateDto createDto,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var roomCreateDto = new InventoryCreateDto
        {
            ArticuloId = createDto.ArticuloId,
            LocationType = InventoryLocationType.Room,
            LocationId = roomId,
            Cantidad = createDto.Cantidad,
        };

        var result = await _coreService.CreateInventoryAsync(
            roomCreateDto,
            institucionId.Value,
            this.GetCurrentUserId(),
            cancellationToken
        );

        return result.IsSuccess
            ? CreatedAtAction(
                nameof(GetInventoryById),
                new { id = result.Data!.InventoryId },
                result
            )
            : StatusCode(500, result);
    }

    /// <summary>
    /// Get general inventory for institution
    /// </summary>
    [HttpGet("general")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetGeneralInventory(
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _coreService.GetInventoryAsync(
            institucionId.Value,
            InventoryLocationType.General,
            null,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Add item to general inventory
    /// </summary>
    [HttpPost("general")]
    [ProducesResponseType(typeof(ApiResponse<InventoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> AddGeneralInventory(
        [FromBody] InventoryCreateDto createDto,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var generalCreateDto = new InventoryCreateDto
        {
            ArticuloId = createDto.ArticuloId,
            LocationType = InventoryLocationType.General,
            LocationId = null,
            Cantidad = createDto.Cantidad,
        };

        var result = await _coreService.CreateInventoryAsync(
            generalCreateDto,
            institucionId.Value,
            this.GetCurrentUserId(),
            cancellationToken
        );

        return result.IsSuccess
            ? CreatedAtAction(
                nameof(GetInventoryById),
                new { id = result.Data!.InventoryId },
                result
            )
            : StatusCode(500, result);
    }

    #endregion

    #region Movement Operations - Using IInventoryMovementService

    /// <summary>
    /// Register an inventory movement
    /// </summary>
    [HttpPost("{id}/movements")]
    [ProducesResponseType(
        typeof(ApiResponse<MovimientoInventarioDto>),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MovimientoInventarioDto>>> RegisterMovement(
        int id,
        [FromBody] MovimientoInventarioCreateDto movementDto,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = this.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        // Set the inventory ID from route
        movementDto.InventarioId = id;

        // First update the inventory quantity
        await _coreService.UpdateInventoryQuantityAsync(
            id,
            movementDto.CantidadNueva,
            institucionId.Value,
            cancellationToken
        );

        // Then record the movement
        var result = await _movementService.CreateMovementAsync(
            movementDto,
            institucionId.Value,
            userId,
            this.GetClientIpAddress(),
            cancellationToken
        );

        if (result.IsSuccess)
        {
            // Check and generate alerts after inventory update
            await _alertService.CheckAndGenerateAlertsAsync(
                id,
                institucionId.Value,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetMovement),
                new { id = result.Data!.MovimientoId },
                result
            );
        }

        return result.Errors.Any(e => e.Contains("not found"))
            ? NotFound(result)
            : StatusCode(500, result);
    }

    /// <summary>
    /// Get movement history for a specific inventory item
    /// </summary>
    [HttpGet("{id}/movements")]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<MovimientoInventarioDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<ApiResponse<PagedResult<MovimientoInventarioDto>>>
    > GetInventoryMovements(
        int id,
        [FromQuery] MovimientoInventarioFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _movementService.GetMovementsAsync(
            id,
            institucionId.Value,
            filter,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result)
            : result.Errors.Any(e => e.Contains("not found")) ? NotFound(result)
            : StatusCode(500, result);
    }

    /// <summary>
    /// Get a specific movement by ID
    /// </summary>
    [HttpGet("movements/{id}")]
    [ProducesResponseType(typeof(ApiResponse<MovimientoInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MovimientoInventarioDto>>> GetMovement(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _movementService.GetMovementByIdAsync(
            id,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result)
            : result.Errors.Any(e => e.Contains("not found")) ? NotFound(result)
            : StatusCode(500, result);
    }

    /// <summary>
    /// Get movement statistics
    /// </summary>
    [HttpGet("movements/statistics")]
    [ProducesResponseType(typeof(ApiResponse<MovimientoEstadisticasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MovimientoEstadisticasDto>>> GetMovementStatistics(
        [FromQuery] MovimientoEstadisticasFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _movementService.GetMovementStatisticsAsync(
            institucionId.Value,
            filter,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    #endregion

    #region Alert Operations - Using IInventoryAlertService

    /// <summary>
    /// Get active inventory alerts
    /// </summary>
    [HttpGet("alerts/active")]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<AlertaInventarioDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<AlertaInventarioDto>>>> GetActiveAlerts(
        [FromQuery] AlertaInventarioFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _alertService.GetActiveAlertsAsync(
            institucionId.Value,
            filter,
            cancellationToken
        );
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Configure alert thresholds for inventory items
    /// </summary>
    [HttpPost("alerts/configure")]
    [ProducesResponseType(
        typeof(ApiResponse<ConfiguracionAlertaDto>),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ConfiguracionAlertaDto>>> ConfigureAlerts(
        [FromBody] ConfiguracionAlertaCreateUpdateDto configDto,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = this.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _alertService.ConfigureAlertsAsync(
            configDto,
            institucionId.Value,
            userId,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetAlertConfiguration),
                new { inventoryId = configDto.InventarioId },
                result
            );
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Acknowledge an inventory alert
    /// </summary>
    [HttpPut("alerts/{id}/acknowledge")]
    [ProducesResponseType(typeof(ApiResponse<AlertaInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AlertaInventarioDto>>> AcknowledgeAlert(
        int id,
        [FromBody] AlertaReconocimientoDto acknowledgmentDto,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = this.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID is required"));
        }

        var result = await _alertService.AcknowledgeAlertAsync(
            id,
            acknowledgmentDto,
            institucionId.Value,
            userId,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result)
            : result.Errors.Any(e => e.Contains("not found")) ? NotFound(result)
            : StatusCode(500, result);
    }

    /// <summary>
    /// Get alert configuration for an inventory item
    /// </summary>
    [HttpGet("alerts/configuration/{inventoryId}")]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracionAlertaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ConfiguracionAlertaDto>>> GetAlertConfiguration(
        int inventoryId,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _alertService.GetAlertConfigurationAsync(
            inventoryId,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result)
            : result.Errors.Any(e => e.Contains("not found")) ? NotFound(result)
            : StatusCode(500, result);
    }

    #endregion

    #region Transfer Operations - Using IInventoryTransferService

    /// <summary>
    /// Create a single transfer request
    /// </summary>
    [HttpPost("transfer")]
    [ProducesResponseType(
        typeof(ApiResponse<TransferenciaInventarioDto>),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TransferenciaInventarioDto>>> CreateTransfer(
        [FromBody] TransferenciaCreateDto transferDto,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = this.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _transferService.CreateTransferAsync(
            transferDto,
            institucionId.Value,
            userId,
            this.GetClientIpAddress(),
            cancellationToken
        );

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetTransfer),
                new { id = result.Data!.TransferenciaId },
                result
            );
        }

        return StatusCode(500, result);
    }

    /// <summary>
    /// Get all transfers with filtering
    /// </summary>
    [HttpGet("transfers")]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<TransferenciaInventarioDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<ApiResponse<PagedResult<TransferenciaInventarioDto>>>
    > GetTransfers(
        [FromQuery] TransferenciaInventarioFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _transferService.GetTransfersAsync(
            institucionId.Value,
            filter,
            cancellationToken
        );
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Approve a transfer request
    /// </summary>
    [HttpPut("transfer/{id}/approve")]
    [ProducesResponseType(typeof(ApiResponse<TransferenciaInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TransferenciaInventarioDto>>> ApproveTransfer(
        int id,
        [FromBody] TransferenciaApprovalDto approvalDto,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var userId = this.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID is required"));
        }

        var result = await _transferService.ApproveTransferAsync(
            id,
            approvalDto,
            institucionId.Value,
            userId,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result)
            : result.Errors.Any(e => e.Contains("not found")) ? NotFound(result)
            : StatusCode(500, result);
    }

    /// <summary>
    /// Get a specific transfer by ID
    /// </summary>
    [HttpGet("transfer/{id}")]
    [ProducesResponseType(typeof(ApiResponse<TransferenciaInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TransferenciaInventarioDto>>> GetTransfer(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _transferService.GetTransferByIdAsync(
            id,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result)
            : result.Errors.Any(e => e.Contains("not found")) ? NotFound(result)
            : StatusCode(500, result);
    }

    #endregion

    #region Health Check

    /// <summary>
    /// Health check endpoint for inventory service
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(
            new
            {
                service = "Inventory Service V1 (Refactored with SRP)",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "2.0.0",
                architecture = "Single Responsibility Principle",
                services = new[]
                {
                    "InventoryCoreService - CRUD operations",
                    "InventoryValidationService - Stock validation",
                    "InventoryReportingService - Reports and analytics",
                    "InventoryMovementService - Movement tracking",
                    "InventoryAlertService - Alert management",
                    "InventoryTransferService - Transfer operations",
                },
                features = new[]
                {
                    "Unified room and general inventory",
                    "Multi-tenant support",
                    "Stock validation",
                    "Comprehensive reporting",
                    "Real-time alerts",
                    "Transfer management",
                    "Movement tracking",
                },
            }
        );
    }

    #endregion
}

