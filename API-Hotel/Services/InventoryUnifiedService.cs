using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Extensions;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service implementation for unified inventory management using InventarioUnificado table
/// </summary>
public class InventoryUnifiedService : IInventoryService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<InventoryUnifiedService> _logger;

    public InventoryUnifiedService(HotelDbContext context, ILogger<InventoryUnifiedService> logger)
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
            var query = _context.InventarioUnificado
                .AsNoTracking()
                .Where(i => i.InstitucionID == institucionId)
                .Include(i => i.Articulo)
                .Include(i => i.Habitacion);

            // Apply location type filter
            if (locationType.HasValue)
            {
                query = query.Where(i => i.TipoUbicacion == (int)locationType.Value)
                    .Include(i => i.Articulo)
                    .Include(i => i.Habitacion);
            }

            // Apply location ID filter (for room-specific queries)
            if (locationId.HasValue)
            {
                query = query.Where(i => i.UbicacionId == locationId.Value)
                    .Include(i => i.Articulo)
                    .Include(i => i.Habitacion);
            }

            var inventories = await query
                .Select(i => new InventoryDto
                {
                    InventoryId = i.InventarioId,
                    ArticuloId = i.ArticuloId,
                    ArticuloNombre = i.Articulo!.NombreArticulo ?? string.Empty,
                    ArticuloDescripcion = null, // No description field in current model
                    ArticuloPrecio = i.Articulo.Precio,
                    Cantidad = i.Cantidad,
                    LocationType = (InventoryLocationType)i.TipoUbicacion,
                    LocationId = i.UbicacionId,
                    LocationName = i.TipoUbicacion == 1 ? i.Habitacion!.NombreHabitacion : "Inventario General",
                    InstitucionId = i.InstitucionID,
                    FechaRegistro = i.FechaRegistro,
                    UserId = i.UsuarioRegistro,
                    UserName = i.CreadoPor != null ? i.CreadoPor.UserName : null,
                    IsActive = !(i.Anulado ?? false)
                })
                .ToListAsync(cancellationToken);

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
            var inventory = await _context.InventarioUnificado
                .AsNoTracking()
                .Where(i => i.InventarioId == inventoryId && i.InstitucionID == institucionId)
                .Include(i => i.Articulo)
                .Include(i => i.Habitacion)
                .Include(i => i.CreadoPor)
                .FirstOrDefaultAsync(cancellationToken);

            if (inventory == null)
            {
                return ApiResponse<InventoryDto>.Failure(
                    "Inventory not found",
                    $"No inventory found with ID {inventoryId}");
            }

            var dto = new InventoryDto
            {
                InventoryId = inventory.InventarioId,
                ArticuloId = inventory.ArticuloId,
                ArticuloNombre = inventory.Articulo?.NombreArticulo ?? string.Empty,
                ArticuloDescripcion = null,
                ArticuloPrecio = inventory.Articulo?.Precio ?? 0,
                Cantidad = inventory.Cantidad,
                LocationType = (InventoryLocationType)inventory.TipoUbicacion,
                LocationId = inventory.UbicacionId,
                LocationName = inventory.TipoUbicacion == 1 ? inventory.Habitacion?.NombreHabitacion : "Inventario General",
                InstitucionId = inventory.InstitucionID,
                FechaRegistro = inventory.FechaRegistro,
                UserId = inventory.UsuarioRegistro,
                UserName = inventory.CreadoPor?.UserName,
                IsActive = !(inventory.Anulado ?? false)
            };

            return ApiResponse<InventoryDto>.Success(dto);
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
            var inventories = await _context.InventarioUnificado
                .AsNoTracking()
                .Where(i => i.ArticuloId == articuloId && i.InstitucionID == institucionId)
                .Include(i => i.Articulo)
                .Include(i => i.Habitacion)
                .Include(i => i.CreadoPor)
                .Select(i => new InventoryDto
                {
                    InventoryId = i.InventarioId,
                    ArticuloId = i.ArticuloId,
                    ArticuloNombre = i.Articulo!.NombreArticulo ?? string.Empty,
                    ArticuloDescripcion = null,
                    ArticuloPrecio = i.Articulo.Precio,
                    Cantidad = i.Cantidad,
                    LocationType = (InventoryLocationType)i.TipoUbicacion,
                    LocationId = i.UbicacionId,
                    LocationName = i.TipoUbicacion == 1 ? i.Habitacion!.NombreHabitacion : "Inventario General",
                    InstitucionId = i.InstitucionID,
                    FechaRegistro = i.FechaRegistro,
                    UserId = i.UsuarioRegistro,
                    UserName = i.CreadoPor != null ? i.CreadoPor.UserName : null,
                    IsActive = !(i.Anulado ?? false)
                })
                .ToListAsync(cancellationToken);

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
            var existingInventory = await _context.InventarioUnificado
                .AnyAsync(i => i.ArticuloId == createDto.ArticuloId &&
                              i.TipoUbicacion == 1 &&
                              i.UbicacionId == habitacionId &&
                              i.InstitucionID == institucionId, cancellationToken);

            if (existingInventory)
            {
                return ApiResponse<InventoryDto>.Failure("Duplicate entry",
                    "This article already exists in the room inventory");
            }

            var inventory = new InventarioUnificado
            {
                ArticuloId = createDto.ArticuloId,
                Cantidad = createDto.Cantidad,
                InstitucionID = institucionId,
                TipoUbicacion = 1, // Room
                UbicacionId = habitacionId,
                FechaRegistro = DateTime.Now,
                UsuarioRegistro = null, // Will be set by controller from current user context
                Anulado = false,
                Notas = createDto.Notes
            };

            _context.InventarioUnificado.Add(inventory);
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
            var existingInventory = await _context.InventarioUnificado
                .AnyAsync(i => i.ArticuloId == createDto.ArticuloId &&
                              i.TipoUbicacion == 0 &&
                              i.InstitucionID == institucionId, cancellationToken);

            if (existingInventory)
            {
                return ApiResponse<InventoryDto>.Failure("Duplicate entry",
                    "This article already exists in the general inventory");
            }

            var inventory = new InventarioUnificado
            {
                ArticuloId = createDto.ArticuloId,
                Cantidad = createDto.Cantidad,
                InstitucionID = institucionId,
                TipoUbicacion = 0, // General
                UbicacionId = null,
                FechaRegistro = DateTime.Now,
                UsuarioRegistro = null, // Will be set by controller from current user context
                Anulado = false,
                Notas = createDto.Notes
            };

            _context.InventarioUnificado.Add(inventory);
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

            // Get existing general inventory articles
            var existingInventoryArticleIds = await _context.InventarioUnificado
                .Where(i => i.InstitucionID == institucionId && i.TipoUbicacion == 0)
                .Select(i => i.ArticuloId)
                .ToListAsync(cancellationToken);

            // Add missing articles with zero quantity
            foreach (var articleId in activeArticleIds)
            {
                if (!existingInventoryArticleIds.Contains(articleId))
                {
                    var newInventory = new InventarioUnificado
                    {
                        ArticuloId = articleId,
                        Cantidad = 0,
                        InstitucionID = institucionId,
                        TipoUbicacion = 0, // General
                        UbicacionId = null,
                        FechaRegistro = DateTime.Now,
                        Anulado = false
                    };

                    _context.InventarioUnificado.Add(newInventory);
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
            var inventory = await _context.InventarioUnificado
                .FirstOrDefaultAsync(i => i.InventarioId == inventoryId && i.InstitucionID == institucionId, cancellationToken);

            if (inventory == null)
            {
                return ApiResponse<InventoryDto>.Failure("Inventory not found", $"No inventory found with ID {inventoryId}");
            }

            inventory.Cantidad = updateDto.Cantidad;
            inventory.FechaUltimaActualizacion = DateTime.Now;
            inventory.UsuarioUltimaActualizacion = null; // Will be set by controller from current user context
            inventory.Notas = updateDto.Notes ?? inventory.Notas;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated inventory {InventoryId} to quantity {Cantidad}",
                inventoryId, updateDto.Cantidad);

            return await GetInventoryByIdAsync(inventoryId, institucionId, cancellationToken);
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
                var inventory = await _context.InventarioUnificado
                    .FirstOrDefaultAsync(i => i.InventarioId == item.InventoryId && i.InstitucionID == institucionId, cancellationToken);

                if (inventory != null)
                {
                    inventory.Cantidad = item.Cantidad;
                    inventory.FechaUltimaActualizacion = DateTime.Now;
                    updatedCount++;
                }
                else
                {
                    errors.Add($"Inventory with ID {item.InventoryId} not found");
                }
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
            var inventory = await _context.InventarioUnificado
                .FirstOrDefaultAsync(i => i.InventarioId == inventoryId && i.InstitucionID == institucionId, cancellationToken);

            if (inventory == null)
            {
                return ApiResponse.Failure("Inventory not found", $"No inventory found with ID {inventoryId}");
            }

            inventory.Anulado = true;
            inventory.FechaAnulacion = DateTime.Now;
            inventory.UsuarioAnulacion = null; // Will be set by controller from current user context
            inventory.MotivoAnulacion = "Deleted via V1 API";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted inventory {InventoryId}", inventoryId);
            return ApiResponse.Success("Inventory item deleted successfully");
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
        // Placeholder implementation - will be implemented in phase 2
        await Task.Delay(1, cancellationToken);
        return ApiResponse<string>.Success("Transfer functionality will be implemented in phase 2 with movement tracking");
    }

    public async Task<ApiResponse<IEnumerable<InventoryMovementDto>>> GetInventoryMovementsAsync(
        int institucionId,
        int? articuloId = null,
        int? locationId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        // Placeholder implementation - will be implemented in phase 2
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
            var inventorySummary = await _context.InventarioUnificado
                .AsNoTracking()
                .Where(i => i.InstitucionID == institucionId)
                .Include(i => i.Articulo)
                .Include(i => i.Habitacion)
                .GroupBy(i => new { i.TipoUbicacion, i.UbicacionId, 
                    LocationName = i.TipoUbicacion == 1 ? i.Habitacion!.NombreHabitacion : "Inventario General" })
                .Select(g => new InventorySummaryDto
                {
                    LocationType = (InventoryLocationType)g.Key.TipoUbicacion,
                    LocationId = g.Key.UbicacionId,
                    LocationName = g.Key.LocationName,
                    TotalItems = g.Sum(i => i.Cantidad),
                    UniqueArticles = g.Count(),
                    TotalValue = g.Sum(i => i.Cantidad * i.Articulo!.Precio),
                    Items = g.Select(i => new InventoryItemSummaryDto
                    {
                        ArticuloId = i.ArticuloId,
                        ArticuloNombre = i.Articulo!.NombreArticulo ?? string.Empty,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.Articulo.Precio,
                        ValorTotal = i.Cantidad * i.Articulo.Precio,
                        UltimaActualizacion = i.FechaUltimaActualizacion ?? i.FechaRegistro
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Generated inventory summary with {Count} locations for institution {InstitucionId}",
                inventorySummary.Count, institucionId);

            return ApiResponse<IEnumerable<InventorySummaryDto>>.Success(inventorySummary);
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
            var query = _context.InventarioUnificado
                .Where(i => i.ArticuloId == articuloId && i.InstitucionID == institucionId)
                .Where(i => i.TipoUbicacion == (int)locationType);

            if (locationType == InventoryLocationType.Room && locationId.HasValue)
            {
                query = query.Where(i => i.UbicacionId == locationId.Value);
            }

            var availableQuantity = await query.SumAsync(i => i.Cantidad, cancellationToken);

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