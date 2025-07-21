using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Extensions;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service implementation for unified inventory management
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(HotelDbContext context, ILogger<InventoryService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region General Inventory Management

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetInventoryAsync(
        int institucionId,
        InventoryLocationType? locationType = null,
        int? locationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var inventories = new List<InventoryDto>();

            // Get room inventories
            if (!locationType.HasValue || locationType == InventoryLocationType.Room)
            {
                var roomInventories = await _context.Inventarios
                    .AsNoTracking()
                    .Where(i => i.InstitucionID == institucionId && (i.Anulado != true))
                    .Where(i => !locationId.HasValue || i.HabitacionId == locationId.Value)
                    .Include(i => i.Articulo)
                    .Include(i => i.Habitacion)
                    .Select(i => new InventoryDto
                    {
                        InventoryId = i.InventarioId,
                        ArticuloId = i.ArticuloId ?? 0,
                        ArticuloNombre = i.Articulo!.NombreArticulo ?? string.Empty,
                        ArticuloDescripcion = null,
                        ArticuloPrecio = i.Articulo.Precio,
                        Cantidad = i.Cantidad ?? 0,
                        LocationType = InventoryLocationType.Room,
                        LocationId = i.HabitacionId,
                        LocationName = i.Habitacion!.NombreHabitacion,
                        InstitucionId = i.InstitucionID,
                        FechaRegistro = i.FechaRegistro ?? DateTime.Now,
                        UserId = null,
                        UserName = null,
                        IsActive = !(i.Anulado ?? false)
                    })
                    .ToListAsync(cancellationToken);

                inventories.AddRange(roomInventories);
            }

            // Get general inventories
            if (!locationType.HasValue || locationType == InventoryLocationType.General)
            {
                var generalInventories = await _context.InventarioGeneral
                    .AsNoTracking()
                    .Where(i => i.InstitucionID == institucionId && (i.Anulado != true))
                    .Include(i => i.Articulo)
                    .Select(i => new InventoryDto
                    {
                        InventoryId = i.InventarioId,
                        ArticuloId = i.ArticuloId ?? 0,
                        ArticuloNombre = i.Articulo!.NombreArticulo ?? string.Empty,
                        ArticuloDescripcion = null,
                        ArticuloPrecio = i.Articulo.Precio,
                        Cantidad = i.Cantidad ?? 0,
                        LocationType = InventoryLocationType.General,
                        LocationId = null,
                        LocationName = "Inventario General",
                        InstitucionId = i.InstitucionID,
                        FechaRegistro = i.FechaRegistro ?? DateTime.Now,
                        UserId = null,
                        UserName = null,
                        IsActive = !(i.Anulado ?? false)
                    })
                    .ToListAsync(cancellationToken);

                inventories.AddRange(generalInventories);
            }

            _logger.LogInformation("Retrieved {Count} inventory items for institution {InstitucionId}", 
                inventories.Count, institucionId);

            return ApiResponse<IEnumerable<InventoryDto>>.Success(inventories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory for institution {InstitucionId}", institucionId);
            return ApiResponse<IEnumerable<InventoryDto>>.Failure(
                "Error retrieving inventory", 
                "An error occurred while retrieving the inventory");
        }
    }

    public async Task<ApiResponse<InventoryDto>> GetInventoryByIdAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First try room inventory
            var roomInventory = await _context.Inventarios
                .AsNoTracking()
                .Where(i => i.InventarioId == inventoryId && i.InstitucionID == institucionId)
                .Include(i => i.Articulo)
                .Include(i => i.Habitacion)
                .FirstOrDefaultAsync(cancellationToken);

            if (roomInventory != null)
            {
                var roomDto = new InventoryDto
                {
                    InventoryId = roomInventory.InventarioId,
                    ArticuloId = roomInventory.ArticuloId ?? 0,
                    ArticuloNombre = roomInventory.Articulo?.NombreArticulo ?? string.Empty,
                    ArticuloDescripcion = null,
                    ArticuloPrecio = roomInventory.Articulo?.Precio ?? 0,
                    Cantidad = roomInventory.Cantidad ?? 0,
                    LocationType = InventoryLocationType.Room,
                    LocationId = roomInventory.HabitacionId,
                    LocationName = roomInventory.Habitacion?.NombreHabitacion,
                    InstitucionId = roomInventory.InstitucionID,
                    FechaRegistro = roomInventory.FechaRegistro ?? DateTime.Now,
                    UserId = null,
                    UserName = null,
                    IsActive = !(roomInventory.Anulado ?? false)
                };

                return ApiResponse<InventoryDto>.Success(roomDto);
            }

            // Try general inventory
            var generalInventory = await _context.InventarioGeneral
                .AsNoTracking()
                .Where(i => i.InventarioId == inventoryId && i.InstitucionID == institucionId)
                .Include(i => i.Articulo)
                .FirstOrDefaultAsync(cancellationToken);

            if (generalInventory != null)
            {
                var generalDto = new InventoryDto
                {
                    InventoryId = generalInventory.InventarioId,
                    ArticuloId = generalInventory.ArticuloId ?? 0,
                    ArticuloNombre = generalInventory.Articulo?.NombreArticulo ?? string.Empty,
                    ArticuloDescripcion = null,
                    ArticuloPrecio = generalInventory.Articulo?.Precio ?? 0,
                    Cantidad = generalInventory.Cantidad ?? 0,
                    LocationType = InventoryLocationType.General,
                    LocationId = null,
                    LocationName = "Inventario General",
                    InstitucionId = generalInventory.InstitucionID,
                    FechaRegistro = generalInventory.FechaRegistro ?? DateTime.Now,
                    UserId = null,
                    UserName = null,
                    IsActive = !(generalInventory.Anulado ?? false)
                };

                return ApiResponse<InventoryDto>.Success(generalDto);
            }

            return ApiResponse<InventoryDto>.Failure(
                "Inventory not found",
                $"No inventory found with ID {inventoryId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory {InventoryId} for institution {InstitucionId}",
                inventoryId, institucionId);
            return ApiResponse<InventoryDto>.Failure(
                "Error retrieving inventory",
                "An error occurred while retrieving the inventory item");
        }
    }

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetInventoryByArticleAsync(
        int articuloId,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var inventories = new List<InventoryDto>();

            // Get from room inventory
            var roomInventories = await _context.Inventarios
                .AsNoTracking()
                .Where(i => i.ArticuloId == articuloId && i.InstitucionID == institucionId && (i.Anulado != true))
                .Include(i => i.Articulo)
                .Include(i => i.Habitacion)
                .ToListAsync(cancellationToken);

            foreach (var item in roomInventories)
            {
                inventories.Add(new InventoryDto
                {
                    InventoryId = item.InventarioId,
                    ArticuloId = item.ArticuloId ?? 0,
                    ArticuloNombre = item.Articulo?.NombreArticulo ?? string.Empty,
                    ArticuloDescripcion = null,
                    ArticuloPrecio = item.Articulo?.Precio ?? 0,
                    Cantidad = item.Cantidad ?? 0,
                    LocationType = InventoryLocationType.Room,
                    LocationId = item.HabitacionId,
                    LocationName = item.Habitacion?.NombreHabitacion,
                    InstitucionId = item.InstitucionID,
                    FechaRegistro = item.FechaRegistro ?? DateTime.Now,
                    UserId = null,
                    UserName = null,
                    IsActive = !(item.Anulado ?? false)
                });
            }

            // Get from general inventory
            var generalInventories = await _context.InventarioGeneral
                .AsNoTracking()
                .Where(i => i.ArticuloId == articuloId && i.InstitucionID == institucionId && (i.Anulado != true))
                .Include(i => i.Articulo)
                .ToListAsync(cancellationToken);

            foreach (var item in generalInventories)
            {
                inventories.Add(new InventoryDto
                {
                    InventoryId = item.InventarioId,
                    ArticuloId = item.ArticuloId ?? 0,
                    ArticuloNombre = item.Articulo?.NombreArticulo ?? string.Empty,
                    ArticuloDescripcion = null,
                    ArticuloPrecio = item.Articulo?.Precio ?? 0,
                    Cantidad = item.Cantidad ?? 0,
                    LocationType = InventoryLocationType.General,
                    LocationId = null,
                    LocationName = "Inventario General",
                    InstitucionId = item.InstitucionID,
                    FechaRegistro = item.FechaRegistro ?? DateTime.Now,
                    UserId = null,
                    UserName = null,
                    IsActive = !(item.Anulado ?? false)
                });
            }

            _logger.LogInformation("Retrieved {Count} inventory items for article {ArticuloId} in institution {InstitucionId}",
                inventories.Count, articuloId, institucionId);

            return ApiResponse<IEnumerable<InventoryDto>>.Success(inventories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory for article {ArticuloId} in institution {InstitucionId}",
                articuloId, institucionId);
            return ApiResponse<IEnumerable<InventoryDto>>.Failure(
                "Error retrieving inventory",
                "An error occurred while retrieving the inventory items");
        }
    }

    #endregion

    #region Room Inventory Management

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetRoomInventoryAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        return await GetInventoryAsync(institucionId, InventoryLocationType.Room, habitacionId, cancellationToken);
    }

    public async Task<ApiResponse<InventoryDto>> AddRoomInventoryAsync(
        int habitacionId,
        InventoryCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate room exists
            var roomExists = await _context.Habitaciones
                .AsNoTracking()
                .AnyAsync(h => h.HabitacionId == habitacionId && h.InstitucionID == institucionId, cancellationToken);

            if (!roomExists)
            {
                return ApiResponse<InventoryDto>.Failure("Room not found", $"No room found with ID {habitacionId}");
            }

            // Validate article exists
            var articleExists = await _context.Articulos
                .AsNoTracking()
                .AnyAsync(a => a.ArticuloId == createDto.ArticuloId, cancellationToken);

            if (!articleExists)
            {
                return ApiResponse<InventoryDto>.Failure("Article not found", $"No article found with ID {createDto.ArticuloId}");
            }

            // Check for duplicates
            var existingInventory = await _context.Inventarios
                .AnyAsync(i => i.HabitacionId == habitacionId && 
                              i.ArticuloId == createDto.ArticuloId &&
                              i.InstitucionID == institucionId &&
                              (i.Anulado != true), cancellationToken);

            if (existingInventory)
            {
                return ApiResponse<InventoryDto>.Failure("Duplicate entry", 
                    "This article already exists in the room inventory");
            }

            var inventory = new Inventarios
            {
                HabitacionId = habitacionId,
                ArticuloId = createDto.ArticuloId,
                Cantidad = createDto.Cantidad,
                InstitucionID = institucionId,
                FechaRegistro = DateTime.Now,
                Anulado = false
            };

            _context.Inventarios.Add(inventory);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added room inventory {InventoryId} for article {ArticuloId} in room {HabitacionId}",
                inventory.InventarioId, createDto.ArticuloId, habitacionId);

            return await GetInventoryByIdAsync(inventory.InventarioId, institucionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding room inventory for room {HabitacionId} and article {ArticuloId}",
                habitacionId, createDto.ArticuloId);
            return ApiResponse<InventoryDto>.Failure("Error adding inventory", 
                "An error occurred while adding the inventory item");
        }
    }

    #endregion

    #region General Institution Inventory

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetGeneralInventoryAsync(
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        return await GetInventoryAsync(institucionId, InventoryLocationType.General, null, cancellationToken);
    }

    public async Task<ApiResponse<InventoryDto>> AddGeneralInventoryAsync(
        InventoryCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate article exists
            var articleExists = await _context.Articulos
                .AsNoTracking()
                .AnyAsync(a => a.ArticuloId == createDto.ArticuloId, cancellationToken);

            if (!articleExists)
            {
                return ApiResponse<InventoryDto>.Failure("Article not found", $"No article found with ID {createDto.ArticuloId}");
            }

            // Check for duplicates
            var existingInventory = await _context.InventarioGeneral
                .AnyAsync(i => i.ArticuloId == createDto.ArticuloId &&
                              i.InstitucionID == institucionId &&
                              (i.Anulado != true), cancellationToken);

            if (existingInventory)
            {
                return ApiResponse<InventoryDto>.Failure("Duplicate entry", 
                    "This article already exists in the general inventory");
            }

            var inventory = new InventarioGeneral
            {
                ArticuloId = createDto.ArticuloId,
                Cantidad = createDto.Cantidad,
                InstitucionID = institucionId,
                FechaRegistro = DateTime.Now,
                Anulado = false
            };

            _context.InventarioGeneral.Add(inventory);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added general inventory {InventoryId} for article {ArticuloId}",
                inventory.InventarioId, createDto.ArticuloId);

            return await GetInventoryByIdAsync(inventory.InventarioId, institucionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding general inventory for article {ArticuloId}", createDto.ArticuloId);
            return ApiResponse<InventoryDto>.Failure("Error adding inventory", 
                "An error occurred while adding the inventory item");
        }
    }

    public async Task<ApiResponse<string>> SynchronizeGeneralInventoryAsync(
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var processedCount = 0;
            var addedCount = 0;

            // Get all active articles
            var activeArticleIds = await _context.Articulos
                .Where(a => a.InstitucionID == institucionId && (a.Anulado != true))
                .Select(a => a.ArticuloId)
                .ToListAsync(cancellationToken);

            // Get existing inventory articles
            var existingInventoryArticleIds = await _context.InventarioGeneral
                .Where(i => i.InstitucionID == institucionId && (i.Anulado != true))
                .Select(i => i.ArticuloId ?? 0)
                .ToListAsync(cancellationToken);

            // Add missing articles with zero quantity
            foreach (var articleId in activeArticleIds)
            {
                if (!existingInventoryArticleIds.Contains(articleId))
                {
                    var newInventory = new InventarioGeneral
                    {
                        ArticuloId = articleId,
                        Cantidad = 0,
                        InstitucionID = institucionId,
                        FechaRegistro = DateTime.Now,
                        Anulado = false
                    };

                    _context.InventarioGeneral.Add(newInventory);
                    addedCount++;
                }
                processedCount++;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var message = $"Synchronization completed: processed {processedCount} articles, added {addedCount} missing entries";
            
            _logger.LogInformation("Synchronized general inventory for institution {InstitucionId}: added {AddedCount} entries",
                institucionId, addedCount);

            return ApiResponse<string>.Success(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing general inventory for institution {InstitucionId}", institucionId);
            return ApiResponse<string>.Failure("Error synchronizing inventory", 
                "An error occurred while synchronizing the inventory");
        }
    }

    #endregion

    #region CRUD Operations

    public async Task<ApiResponse<InventoryDto>> CreateInventoryAsync(
        InventoryCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        if (createDto.LocationType == InventoryLocationType.Room)
        {
            if (!createDto.LocationId.HasValue)
            {
                return ApiResponse<InventoryDto>.Failure("Location ID required", 
                    "Location ID is required for room inventory");
            }

            return await AddRoomInventoryAsync(createDto.LocationId.Value, createDto, institucionId, cancellationToken);
        }
        else
        {
            return await AddGeneralInventoryAsync(createDto, institucionId, cancellationToken);
        }
    }

    public async Task<ApiResponse<InventoryDto>> UpdateInventoryAsync(
        int inventoryId,
        InventoryUpdateDto updateDto,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Try room inventory first
            var roomInventory = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.InventarioId == inventoryId && i.InstitucionID == institucionId, cancellationToken);

            if (roomInventory != null)
            {
                roomInventory.Cantidad = updateDto.Cantidad;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated room inventory {InventoryId} to quantity {Cantidad}",
                    inventoryId, updateDto.Cantidad);

                return await GetInventoryByIdAsync(inventoryId, institucionId, cancellationToken);
            }

            // Try general inventory
            var generalInventory = await _context.InventarioGeneral
                .FirstOrDefaultAsync(i => i.InventarioId == inventoryId && i.InstitucionID == institucionId, cancellationToken);

            if (generalInventory != null)
            {
                generalInventory.Cantidad = updateDto.Cantidad;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated general inventory {InventoryId} to quantity {Cantidad}",
                    inventoryId, updateDto.Cantidad);

                return await GetInventoryByIdAsync(inventoryId, institucionId, cancellationToken);
            }

            return ApiResponse<InventoryDto>.Failure("Inventory not found", $"No inventory found with ID {inventoryId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory {InventoryId}", inventoryId);
            return ApiResponse<InventoryDto>.Failure("Error updating inventory", 
                "An error occurred while updating the inventory");
        }
    }

    public async Task<ApiResponse<string>> BatchUpdateInventoryAsync(
        InventoryBatchUpdateDto batchUpdateDto,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var updatedCount = 0;
            var errors = new List<string>();

            foreach (var item in batchUpdateDto.Items)
            {
                var updated = false;

                // Try room inventory first
                var roomInventory = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.InventarioId == item.InventoryId && i.InstitucionID == institucionId, cancellationToken);

                if (roomInventory != null)
                {
                    roomInventory.Cantidad = item.Cantidad;
                    updated = true;
                }
                else
                {
                    // Try general inventory
                    var generalInventory = await _context.InventarioGeneral
                        .FirstOrDefaultAsync(i => i.InventarioId == item.InventoryId && i.InstitucionID == institucionId, cancellationToken);

                    if (generalInventory != null)
                    {
                        generalInventory.Cantidad = item.Cantidad;
                        updated = true;
                    }
                }

                if (updated)
                    updatedCount++;
                else
                    errors.Add($"Inventory with ID {item.InventoryId} not found");
            }

            if (errors.Any())
            {
                await transaction.RollbackAsync(cancellationToken);
                return ApiResponse<string>.Failure("Batch update failed", string.Join("; ", errors));
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var message = $"Successfully updated {updatedCount} inventory items";
            _logger.LogInformation("Batch updated {Count} inventory items for institution {InstitucionId}",
                updatedCount, institucionId);

            return ApiResponse<string>.Success(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing batch update for institution {InstitucionId}", institucionId);
            return ApiResponse<string>.Failure("Error performing batch update", 
                "An error occurred while performing the batch update");
        }
    }

    public async Task<ApiResponse> DeleteInventoryAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Try room inventory first
            var roomInventory = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.InventarioId == inventoryId && i.InstitucionID == institucionId, cancellationToken);

            if (roomInventory != null)
            {
                roomInventory.Anulado = true;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Deleted room inventory {InventoryId}", inventoryId);
                return ApiResponse.Success("Room inventory item deleted successfully");
            }

            // Try general inventory
            var generalInventory = await _context.InventarioGeneral
                .FirstOrDefaultAsync(i => i.InventarioId == inventoryId && i.InstitucionID == institucionId, cancellationToken);

            if (generalInventory != null)
            {
                generalInventory.Anulado = true;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Deleted general inventory {InventoryId}", inventoryId);
                return ApiResponse.Success("General inventory item deleted successfully");
            }

            return ApiResponse.Failure("Inventory not found", $"No inventory found with ID {inventoryId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory {InventoryId}", inventoryId);
            return ApiResponse.Failure("Error deleting inventory", 
                "An error occurred while deleting the inventory item");
        }
    }

    #endregion

    #region Transfer Operations (Placeholder)

    public async Task<ApiResponse<string>> TransferInventoryAsync(
        InventoryTransferDto transferDto,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        // Placeholder implementation
        await Task.Delay(1, cancellationToken);
        return ApiResponse<string>.Success("Transfer functionality will be implemented in phase 2");
    }

    public async Task<ApiResponse<IEnumerable<InventoryMovementDto>>> GetInventoryMovementsAsync(
        int institucionId,
        int? articuloId = null,
        int? locationId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        // Placeholder implementation
        await Task.Delay(1, cancellationToken);
        var movements = new List<InventoryMovementDto>();
        return ApiResponse<IEnumerable<InventoryMovementDto>>.Success(movements);
    }

    #endregion

    #region Reporting and Analysis

    public async Task<ApiResponse<IEnumerable<InventorySummaryDto>>> GetInventorySummaryAsync(
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summaries = new List<InventorySummaryDto>();

            // General inventory summary
            var generalInventories = await _context.InventarioGeneral
                .AsNoTracking()
                .Where(i => i.InstitucionID == institucionId && (i.Anulado != true))
                .Include(i => i.Articulo)
                .ToListAsync(cancellationToken);

            if (generalInventories.Any())
            {
                var generalItems = generalInventories.Select(i => new InventoryItemSummaryDto
                {
                    ArticuloId = i.ArticuloId ?? 0,
                    ArticuloNombre = i.Articulo?.NombreArticulo ?? string.Empty,
                    Cantidad = i.Cantidad ?? 0,
                    PrecioUnitario = i.Articulo?.Precio ?? 0,
                    ValorTotal = (i.Cantidad ?? 0) * (i.Articulo?.Precio ?? 0),
                    UltimaActualizacion = i.FechaRegistro ?? DateTime.Now
                }).ToList();

                summaries.Add(new InventorySummaryDto
                {
                    LocationType = InventoryLocationType.General,
                    LocationId = null,
                    LocationName = "Inventario General",
                    TotalItems = generalItems.Sum(i => i.Cantidad),
                    UniqueArticles = generalItems.Count,
                    TotalValue = generalItems.Sum(i => i.ValorTotal),
                    Items = generalItems
                });
            }

            _logger.LogInformation("Generated inventory summary with {Count} locations for institution {InstitucionId}",
                summaries.Count, institucionId);

            return ApiResponse<IEnumerable<InventorySummaryDto>>.Success(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory summary for institution {InstitucionId}", institucionId);
            return ApiResponse<IEnumerable<InventorySummaryDto>>.Failure("Error generating summary", 
                "An error occurred while generating the inventory summary");
        }
    }

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetCombinedInventoryAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var roomResult = await GetRoomInventoryAsync(habitacionId, institucionId, cancellationToken);
            var generalResult = await GetGeneralInventoryAsync(institucionId, cancellationToken);

            if (!roomResult.IsSuccess)
                return roomResult;

            if (!generalResult.IsSuccess)
                return generalResult;

            var combinedInventory = roomResult.Data!.Concat(generalResult.Data!);

            _logger.LogInformation("Retrieved combined inventory for room {HabitacionId}: {RoomCount} room items, {GeneralCount} general items",
                habitacionId, roomResult.Data.Count(), generalResult.Data.Count());

            return ApiResponse<IEnumerable<InventoryDto>>.Success(combinedInventory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving combined inventory for room {HabitacionId}", habitacionId);
            return ApiResponse<IEnumerable<InventoryDto>>.Failure("Error retrieving combined inventory", 
                "An error occurred while retrieving the combined inventory");
        }
    }

    #endregion

    #region Stock Validation

    public async Task<ApiResponse<StockValidationDto>> ValidateStockAsync(
        int articuloId,
        int requestedQuantity,
        InventoryLocationType locationType,
        int? locationId,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            int availableQuantity = 0;

            if (locationType == InventoryLocationType.Room && locationId.HasValue)
            {
                availableQuantity = await _context.Inventarios
                    .Where(i => i.ArticuloId == articuloId && 
                               i.HabitacionId == locationId.Value && 
                               i.InstitucionID == institucionId && 
                               (i.Anulado != true))
                    .SumAsync(i => i.Cantidad ?? 0, cancellationToken);
            }
            else if (locationType == InventoryLocationType.General)
            {
                availableQuantity = await _context.InventarioGeneral
                    .Where(i => i.ArticuloId == articuloId && 
                               i.InstitucionID == institucionId && 
                               (i.Anulado != true))
                    .SumAsync(i => i.Cantidad ?? 0, cancellationToken);
            }

            var isValid = availableQuantity >= requestedQuantity;
            var errorMessage = isValid ? null : 
                $"Insufficient stock. Available: {availableQuantity}, Requested: {requestedQuantity}";

            var validation = new StockValidationDto
            {
                IsValid = isValid,
                ErrorMessage = errorMessage,
                ArticuloId = articuloId,
                RequestedQuantity = requestedQuantity,
                AvailableQuantity = availableQuantity,
                LocationType = locationType,
                LocationId = locationId
            };

            _logger.LogInformation("Stock validation for article {ArticuloId}: Available={Available}, Requested={Requested}, Valid={Valid}",
                articuloId, availableQuantity, requestedQuantity, isValid);

            return ApiResponse<StockValidationDto>.Success(validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating stock for article {ArticuloId}", articuloId);
            return ApiResponse<StockValidationDto>.Failure("Error validating stock", 
                "An error occurred while validating the stock");
        }
    }

    #endregion
}