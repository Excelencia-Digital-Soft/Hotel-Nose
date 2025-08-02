using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service focused solely on core inventory CRUD operations
/// Follows Single Responsibility Principle
/// </summary>
public class InventoryCoreService : IInventoryCoreService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<InventoryCoreService> _logger;

    public InventoryCoreService(HotelDbContext context, ILogger<InventoryCoreService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetInventoryAsync(
        int institucionId,
        InventoryLocationType? locationType = null,
        int? locationId = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .InventarioUnificado.AsNoTracking()
                .Where(i => i.InstitucionID == institucionId);

            if (locationType.HasValue)
            {
                query = query.Where(i => i.TipoUbicacion == (int)locationType.Value);
            }

            if (locationId.HasValue)
            {
                query = query.Where(i => i.UbicacionId == locationId.Value);
            }

            var inventory = await query
                .Include(i => i.Articulo)
                .ThenInclude(a => a!.Imagen)
                .Include(i => i.Habitacion)
                .ToListAsync(cancellationToken);

            var inventoryDtos = inventory.Select(i => new InventoryDto
            {
                InventoryId = i.InventarioId,
                ArticuloId = i.ArticuloId,
                ArticuloNombre = i.Articulo?.NombreArticulo ?? "Desconocido",
                ArticuloDescripcion = "",
                ArticuloPrecio = i.Articulo?.Precio ?? 0,
                ArticuloImagenUrl = i.Articulo?.Imagen?.Origen,
                Cantidad = i.Cantidad,
                LocationType = (InventoryLocationType)i.TipoUbicacion,
                LocationId = i.UbicacionId,
                LocationName = GetLocationName(i),
                InstitucionId = i.InstitucionID,
                FechaRegistro = i.FechaRegistro,
                UserId = i.UsuarioRegistro,
                UserName = "",
                IsActive = i.Anulado != true,
            });

            return ApiResponse<IEnumerable<InventoryDto>>.Success(inventoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving inventory for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<InventoryDto>>.Failure("Error retrieving inventory");
        }
    }

    public async Task<ApiResponse<InventoryDto>> GetInventoryByIdAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var inventory = await _context
                .InventarioUnificado.AsNoTracking()
                .Include(i => i.Articulo)
                .ThenInclude(a => a!.Imagen)
                .Include(i => i.Habitacion)
                .FirstOrDefaultAsync(
                    i => i.InventarioId == inventoryId && i.InstitucionID == institucionId,
                    cancellationToken
                );

            if (inventory == null)
            {
                return ApiResponse<InventoryDto>.Failure("Inventory not found");
            }

            var inventoryDto = new InventoryDto
            {
                InventoryId = inventory.InventarioId,
                ArticuloId = inventory.ArticuloId,
                ArticuloNombre = inventory.Articulo?.NombreArticulo ?? "Desconocido",
                ArticuloDescripcion = "",
                ArticuloPrecio = inventory.Articulo?.Precio ?? 0,
                ArticuloImagenUrl = inventory.Articulo?.Imagen?.Origen,
                Cantidad = inventory.Cantidad,
                LocationType = (InventoryLocationType)inventory.TipoUbicacion,
                LocationId = inventory.UbicacionId,
                LocationName = GetLocationName(inventory),
                InstitucionId = inventory.InstitucionID,
                FechaRegistro = inventory.FechaRegistro,
                UserId = inventory.UsuarioRegistro,
                UserName = "",
                IsActive = inventory.Anulado != true,
            };

            return ApiResponse<InventoryDto>.Success(inventoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving inventory {InventoryId} for institution {InstitucionId}",
                inventoryId,
                institucionId
            );
            return ApiResponse<InventoryDto>.Failure("Error retrieving inventory item");
        }
    }

    public async Task<ApiResponse<InventoryDto>> CreateInventoryAsync(
        InventoryCreateDto createDto,
        int institucionId,
        string? userId = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Check if inventory already exists for this article and location
            var existingInventory = await _context.InventarioUnificado.FirstOrDefaultAsync(
                i =>
                    i.ArticuloId == createDto.ArticuloId
                    && i.TipoUbicacion == (int)createDto.LocationType
                    && i.UbicacionId == createDto.LocationId
                    && i.InstitucionID == institucionId,
                cancellationToken
            );

            if (existingInventory != null)
            {
                return ApiResponse<InventoryDto>.Failure(
                    "Inventory already exists for this article in this location"
                );
            }

            // Validate article exists
            var articleExists = await _context.Articulos.AnyAsync(
                a => a.ArticuloId == createDto.ArticuloId && a.InstitucionID == institucionId,
                cancellationToken
            );

            if (!articleExists)
            {
                return ApiResponse<InventoryDto>.Failure("Article not found");
            }

            var inventory = new InventarioUnificado
            {
                ArticuloId = createDto.ArticuloId,
                InstitucionID = institucionId,
                TipoUbicacion = (int)createDto.LocationType,
                UbicacionId = createDto.LocationId,
                Cantidad = createDto.Cantidad,
                FechaRegistro = DateTime.UtcNow,
                FechaUltimaActualizacion = DateTime.UtcNow,
                UsuarioRegistro = userId ?? "system",
            };

            _context.InventarioUnificado.Add(inventory);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Inventory created for article {ArticuloId} in location {TipoUbicacion}:{UbicacionId}",
                createDto.ArticuloId,
                createDto.LocationType,
                createDto.LocationId
            );

            return await GetInventoryByIdAsync(
                inventory.InventarioId,
                institucionId,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating inventory for article {ArticuloId} in institution {InstitucionId}",
                createDto.ArticuloId,
                institucionId
            );
            return ApiResponse<InventoryDto>.Failure("Error creating inventory");
        }
    }

    public async Task<ApiResponse<InventoryDto>> UpdateInventoryQuantityAsync(
        int inventoryId,
        int newQuantity,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var inventory = await _context.InventarioUnificado.FirstOrDefaultAsync(
                i => i.InventarioId == inventoryId && i.InstitucionID == institucionId,
                cancellationToken
            );

            if (inventory == null)
            {
                return ApiResponse<InventoryDto>.Failure("Inventory not found");
            }

            inventory.Cantidad = newQuantity;
            inventory.FechaUltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Inventory {InventoryId} quantity updated to {NewQuantity}",
                inventoryId,
                newQuantity
            );

            return await GetInventoryByIdAsync(inventoryId, institucionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating inventory {InventoryId} in institution {InstitucionId}",
                inventoryId,
                institucionId
            );
            return ApiResponse<InventoryDto>.Failure("Error updating inventory");
        }
    }

    public async Task<ApiResponse> DeleteInventoryAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var inventory = await _context.InventarioUnificado.FirstOrDefaultAsync(
                i => i.InventarioId == inventoryId && i.InstitucionID == institucionId,
                cancellationToken
            );

            if (inventory == null)
            {
                return ApiResponse.Failure("Inventory not found");
            }

            _context.InventarioUnificado.Remove(inventory);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Inventory {InventoryId} deleted from institution {InstitucionId}",
                inventoryId,
                institucionId
            );

            return ApiResponse.Success("Inventory deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error deleting inventory {InventoryId} in institution {InstitucionId}",
                inventoryId,
                institucionId
            );
            return ApiResponse.Failure("Error deleting inventory");
        }
    }

    public async Task<ApiResponse<string>> SynchronizeGeneralInventoryAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var articles = await _context
                .Articulos.AsNoTracking()
                .Where(a => a.InstitucionID == institucionId)
                .ToListAsync(cancellationToken);

            var existingInventory = await _context
                .InventarioUnificado.AsNoTracking()
                .Where(i =>
                    i.InstitucionID == institucionId
                    && i.TipoUbicacion == (int)InventoryLocationType.General
                )
                .Select(i => i.ArticuloId)
                .ToListAsync(cancellationToken);

            var articlesToAdd = articles
                .Where(a => !existingInventory.Contains(a.ArticuloId))
                .ToList();

            foreach (var article in articlesToAdd)
            {
                var inventory = new InventarioUnificado
                {
                    ArticuloId = article.ArticuloId,
                    InstitucionID = institucionId,
                    TipoUbicacion = (int)InventoryLocationType.General,
                    UbicacionId = null,
                    Cantidad = 0,
                    FechaRegistro = DateTime.UtcNow,
                    FechaUltimaActualizacion = DateTime.UtcNow,
                };

                _context.InventarioUnificado.Add(inventory);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Synchronized {Count} articles to general inventory for institution {InstitucionId}",
                articlesToAdd.Count,
                institucionId
            );

            return ApiResponse<string>.Success(
                $"Synchronized {articlesToAdd.Count} articles to general inventory"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error synchronizing general inventory for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<string>.Failure("Error synchronizing general inventory");
        }
    }

    public async Task<ApiResponse<string>> BatchUpdateInventoryAsync(
        InventoryBatchUpdateDto batchUpdateDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            int updatedCount = 0;

            foreach (var update in batchUpdateDto.Items)
            {
                var result = await UpdateInventoryQuantityAsync(
                    update.InventoryId,
                    update.Cantidad,
                    institucionId,
                    cancellationToken
                );

                if (result.IsSuccess)
                {
                    updatedCount++;
                }
            }

            return ApiResponse<string>.Success(
                $"Successfully updated {updatedCount} inventory items"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error in batch update for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<string>.Failure("Error in batch update");
        }
    }

    private string GetLocationName(InventarioUnificado inventory)
    {
        return inventory.TipoUbicacion switch
        {
            0 => "Inventario General",
            1 => inventory.Habitacion?.NombreHabitacion ?? $"Habitación {inventory.UbicacionId}",
            2 => $"Almacén {inventory.UbicacionId}",
            _ => "Ubicación Desconocida",
        };
    }
}

