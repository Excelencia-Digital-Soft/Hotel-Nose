using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Extensions;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Controllers.V1;

/// <summary>
/// V1 API controller for unified inventory management
/// </summary>
[ApiController]
[Route("api/v1/inventory-legacy")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class InventoryControllerLegacy : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryControllerLegacy> _logger;

    public InventoryControllerLegacy(
        IInventoryService inventoryService,
        ILogger<InventoryControllerLegacy> logger
    )
    {
        _inventoryService =
            inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region General Inventory Management

    /// <summary>
    /// Get all inventory items for the current institution
    /// </summary>
    /// <param name="locationType">Filter by location type (optional)</param>
    /// <param name="locationId">Filter by specific location ID (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of inventory items</returns>
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

        var result = await _inventoryService.GetInventoryAsync(
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
    /// <param name="id">Inventory ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Inventory item details</returns>
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

        var result = await _inventoryService.GetInventoryByIdAsync(
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
    /// Get inventory items by article ID
    /// </summary>
    /// <param name="articleId">Article ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of inventory items for the article</returns>
    [HttpGet("by-article/{articleId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDto>>>> GetInventoryByArticle(
        int articleId,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetInventoryByArticleAsync(
            articleId,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    #endregion

    #region CRUD Operations

    /// <summary>
    /// Create a new inventory item
    /// </summary>
    /// <param name="createDto">Inventory creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created inventory item</returns>
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

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _inventoryService.CreateInventoryAsync(
            createDto,
            institucionId.Value,
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
    /// Update inventory quantity
    /// </summary>
    /// <param name="id">Inventory ID</param>
    /// <param name="updateDto">Inventory update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated inventory item</returns>
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

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _inventoryService.UpdateInventoryAsync(
            id,
            updateDto,
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

        var result = await _inventoryService.BatchUpdateInventoryAsync(
            batchUpdateDto,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Delete inventory item (soft delete)
    /// </summary>
    /// <param name="id">Inventory ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
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

        var result = await _inventoryService.DeleteInventoryAsync(
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

    #endregion

    #region Room Inventory

    /// <summary>
    /// Get all inventory items for a specific room
    /// </summary>
    /// <param name="roomId">Room ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of room inventory items</returns>
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

        var result = await _inventoryService.GetRoomInventoryAsync(
            roomId,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Add inventory item to a room
    /// </summary>
    /// <param name="roomId">Room ID</param>
    /// <param name="createDto">Inventory creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created inventory item</returns>
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

        var result = await _inventoryService.AddRoomInventoryAsync(
            roomId,
            createDto,
            institucionId.Value,
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
    /// Get combined inventory view (general + room inventories)
    /// </summary>
    /// <param name="roomId">Room ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Combined inventory view</returns>
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

        var result = await _inventoryService.GetCombinedInventoryAsync(
            roomId,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    #endregion

    #region General Inventory

    /// <summary>
    /// Get general inventory for institution (non-room specific)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of general inventory items</returns>
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

        var result = await _inventoryService.GetGeneralInventoryAsync(
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Add item to general inventory
    /// </summary>
    /// <param name="createDto">Inventory creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created inventory item</returns>
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

        var result = await _inventoryService.AddGeneralInventoryAsync(
            createDto,
            institucionId.Value,
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
    /// Synchronize general inventory with articles catalog
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Synchronization result</returns>
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

        var result = await _inventoryService.SynchronizeGeneralInventoryAsync(
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    #endregion

    #region Reporting and Analysis

    /// <summary>
    /// Get inventory summary by location
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Inventory summary by location</returns>
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

        var result = await _inventoryService.GetInventorySummaryAsync(
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    #endregion

    #region Stock Validation

    /// <summary>
    /// Validate stock availability
    /// </summary>
    /// <param name="articleId">Article ID</param>
    /// <param name="requestedQuantity">Requested quantity</param>
    /// <param name="locationType">Location type</param>
    /// <param name="locationId">Location ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stock validation result</returns>
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

        var result = await _inventoryService.ValidateStockAsync(
            articleId,
            requestedQuantity,
            locationType,
            locationId,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    #endregion

    #region Inventory Movements

    /// <summary>
    /// Register an inventory movement
    /// </summary>
    /// <param name="id">Inventory ID</param>
    /// <param name="movementDto">Movement data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created movement</returns>
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

        var result = await _inventoryService.RegisterMovementAsync(
            movementDto,
            institucionId.Value,
            userId,
            this.GetClientIpAddress(),
            cancellationToken
        );

        if (result.IsSuccess)
        {
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
    /// <param name="id">Inventory ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of movements</returns>
    [HttpGet("{id}/movements")]
    [ProducesResponseType(
        typeof(ApiResponse<MovimientoInventarioResumenDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<ApiResponse<MovimientoInventarioResumenDto>>
    > GetInventoryMovements(int id, CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetInventoryMovementsAsync(
            id,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result)
            : result.Errors.Any(e => e.Contains("not found")) ? NotFound(result)
            : StatusCode(500, result);
    }

    /// <summary>
    /// Get movement audit trail with advanced filtering
    /// </summary>
    /// <param name="request">Audit request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated audit results</returns>
    [HttpGet("movements/audit")]
    [ProducesResponseType(
        typeof(ApiResponse<MovimientoAuditoriaResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MovimientoAuditoriaResponseDto>>> GetMovementAudit(
        [FromQuery] MovimientoAuditoriaRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetMovementAuditAsync(
            request,
            institucionId.Value,
            cancellationToken
        );
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get a specific movement by ID
    /// </summary>
    /// <param name="id">Movement ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Movement details</returns>
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

        var result = await _inventoryService.GetMovementByIdAsync(
            id,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result)
            : result.Errors.Any(e => e.Contains("not found")) ? NotFound(result)
            : StatusCode(500, result);
    }

    #endregion

    #region Inventory Alerts

    /// <summary>
    /// Get active inventory alerts
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active alerts</returns>
    [HttpGet("alerts/active")]
    [ProducesResponseType(typeof(ApiResponse<AlertasActivasResumenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AlertasActivasResumenDto>>> GetActiveAlerts(
        [FromQuery] AlertaFiltroRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetActiveAlertsAsync(
            request,
            institucionId.Value,
            cancellationToken
        );
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Configure alert thresholds for inventory items
    /// </summary>
    /// <param name="configDto">Alert configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created configuration</returns>
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

        var result = await _inventoryService.ConfigureAlertsAsync(
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
    /// <param name="id">Alert ID</param>
    /// <param name="acknowledgmentDto">Acknowledgment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated alert</returns>
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

        var result = await _inventoryService.AcknowledgeAlertAsync(
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
    /// <param name="inventoryId">Inventory ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Alert configuration</returns>
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

        var result = await _inventoryService.GetAlertConfigurationAsync(
            inventoryId,
            institucionId.Value,
            cancellationToken
        );

        return result.IsSuccess ? Ok(result)
            : result.Errors.Any(e => e.Contains("not found")) ? NotFound(result)
            : StatusCode(500, result);
    }

    #endregion

    #region Enhanced Transfer Operations

    /// <summary>
    /// Create a single transfer request
    /// </summary>
    /// <param name="transferDto">Transfer data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transfer</returns>
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

        var result = await _inventoryService.CreateTransferAsync(
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
    /// Create multiple transfer requests in batch
    /// </summary>
    /// <param name="batchDto">Batch transfer data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transfers</returns>
    [HttpPost("transfer/batch")]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<TransferenciaInventarioDto>>),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<ApiResponse<IEnumerable<TransferenciaInventarioDto>>>
    > CreateBatchTransfers(
        [FromBody] TransferenciaBatchCreateDto batchDto,
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

        var result = await _inventoryService.CreateBatchTransfersAsync(
            batchDto,
            institucionId.Value,
            userId,
            this.GetClientIpAddress(),
            cancellationToken
        );
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetTransfers), null, result)
            : StatusCode(500, result);
    }

    /// <summary>
    /// Get pending transfers requiring approval
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending transfers</returns>
    [HttpGet("transfer/pending")]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<TransferenciaInventarioDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<ApiResponse<IEnumerable<TransferenciaInventarioDto>>>
    > GetPendingTransfers(
        [FromQuery] TransferenciaInventarioFilterDto request,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        // Force filter to pending only
        request.Estado = "Pendiente";

        var result = await _inventoryService.GetTransfersAsync(
            request,
            institucionId.Value,
            cancellationToken
        );
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Approve a transfer request
    /// </summary>
    /// <param name="id">Transfer ID</param>
    /// <param name="approvalDto">Approval data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated transfer</returns>
    [HttpPut("transfer/{id}/approve")]
    [ProducesResponseType(typeof(ApiResponse<TransferenciaInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TransferenciaInventarioDto>>> ApproveTransfer(
        int id,
        [FromBody] TransferenciaAprobacionDto approvalDto,
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

        var result = await _inventoryService.ApproveTransferAsync(
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
    /// Get all transfers with filtering
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of transfers</returns>
    [HttpGet("transfers")]
    [ProducesResponseType(
        typeof(ApiResponse<IEnumerable<TransferenciaInventarioDto>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<ApiResponse<IEnumerable<TransferenciaInventarioDto>>>
    > GetTransfers(
        [FromQuery] TransferenciaInventarioFilterDto request,
        CancellationToken cancellationToken = default
    )
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetTransfersAsync(
            request,
            institucionId.Value,
            cancellationToken
        );
        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get a specific transfer by ID
    /// </summary>
    /// <param name="id">Transfer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer details</returns>
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

        var result = await _inventoryService.GetTransferByIdAsync(
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
    /// <returns>Health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(
            new
            {
                service = "Inventory Service V1",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                features = new[]
                {
                    "Unified room and general inventory",
                    "Multi-tenant support",
                    "Stock validation",
                    "Batch operations",
                    "Inventory synchronization",
                },
            }
        );
    }

    #endregion
}
