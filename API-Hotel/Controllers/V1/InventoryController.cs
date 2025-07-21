using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Extensions;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace hotel.Controllers.V1;

/// <summary>
/// V1 API controller for unified inventory management
/// </summary>
[ApiController]
[Route("api/v1/inventory")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetInventoryAsync(
            institucionId.Value, locationType, locationId, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetInventoryByIdAsync(id, institucionId.Value, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found")) ? 
                NotFound(result) : StatusCode(500, result);
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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetInventoryByArticleAsync(
            articleId, institucionId.Value, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _inventoryService.CreateInventoryAsync(
            createDto, institucionId.Value, cancellationToken);

        return result.IsSuccess ? 
            CreatedAtAction(nameof(GetInventoryById), new { id = result.Data!.InventoryId }, result) :
            StatusCode(500, result);
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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _inventoryService.UpdateInventoryAsync(
            id, updateDto, institucionId.Value, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found")) ? 
                NotFound(result) : StatusCode(500, result);
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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _inventoryService.BatchUpdateInventoryAsync(
            batchUpdateDto, institucionId.Value, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.DeleteInventoryAsync(id, institucionId.Value, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found")) ? 
                NotFound(result) : StatusCode(500, result);
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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetRoomInventoryAsync(
            roomId, institucionId.Value, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _inventoryService.AddRoomInventoryAsync(
            roomId, createDto, institucionId.Value, cancellationToken);

        return result.IsSuccess ? 
            CreatedAtAction(nameof(GetInventoryById), new { id = result.Data!.InventoryId }, result) :
            StatusCode(500, result);
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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetCombinedInventoryAsync(
            roomId, institucionId.Value, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetGeneralInventoryAsync(institucionId.Value, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _inventoryService.AddGeneralInventoryAsync(
            createDto, institucionId.Value, cancellationToken);

        return result.IsSuccess ? 
            CreatedAtAction(nameof(GetInventoryById), new { id = result.Data!.InventoryId }, result) :
            StatusCode(500, result);
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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.SynchronizeGeneralInventoryAsync(
            institucionId.Value, cancellationToken);

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
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventorySummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventorySummaryDto>>>> GetInventorySummary(
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetInventorySummaryAsync(institucionId.Value, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.ValidateStockAsync(
            articleId, requestedQuantity, locationType, locationId, 
            institucionId.Value, cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    #endregion

    #region Transfer Operations (Future Implementation)

    /// <summary>
    /// Transfer inventory between locations
    /// </summary>
    /// <param name="transferDto">Transfer data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer result</returns>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> TransferInventory(
        [FromBody] InventoryTransferDto transferDto,
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse.Failure(errors, "Validation failed"));
        }

        var result = await _inventoryService.TransferInventoryAsync(
            transferDto, institucionId.Value, cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get inventory movement history
    /// </summary>
    /// <param name="articleId">Filter by article ID (optional)</param>
    /// <param name="locationId">Filter by location ID (optional)</param>
    /// <param name="fromDate">Filter from date (optional)</param>
    /// <param name="toDate">Filter to date (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of inventory movements</returns>
    [HttpGet("movements")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventoryMovementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryMovementDto>>>> GetInventoryMovements(
        [FromQuery] int? articleId = null,
        [FromQuery] int? locationId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        var result = await _inventoryService.GetInventoryMovementsAsync(
            institucionId.Value, articleId, locationId, fromDate, toDate, cancellationToken);

        return result.IsSuccess ? Ok(result) : StatusCode(500, result);
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
        return Ok(new 
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
                "Inventory synchronization"
            }
        });
    }

    #endregion
}