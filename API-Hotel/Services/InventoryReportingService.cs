using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service focused solely on inventory reporting and statistics
/// Follows Single Responsibility Principle
/// </summary>
public class InventoryReportingService : IInventoryReportingService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<InventoryReportingService> _logger;

    public InventoryReportingService(
        HotelDbContext context,
        ILogger<InventoryReportingService> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<IEnumerable<InventorySummaryDto>>> GetInventorySummaryAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var summaries = await _context
                .InventarioUnificado.AsNoTracking()
                .Where(i => i.InstitucionID == institucionId)
                .Include(i => i.Articulo)
                .GroupBy(i => new { i.TipoUbicacion, i.UbicacionId })
                .Select(g => new InventorySummaryDto
                {
                    LocationType = (InventoryLocationType)g.Key.TipoUbicacion,
                    LocationId = g.Key.UbicacionId,
                    LocationName =
                        g.Key.TipoUbicacion == 1
                            ? _context
                                .Habitaciones.Where(h => h.HabitacionId == g.Key.UbicacionId)
                                .Select(h => h.NombreHabitacion)
                                .FirstOrDefault() ?? $"Habitación {g.Key.UbicacionId}"
                        : g.Key.TipoUbicacion == 0 ? "Inventario General"
                        : $"Almacén {g.Key.UbicacionId}",
                    UniqueArticles = g.Count(),
                    TotalItems = g.Sum(i => i.Cantidad),
                    TotalValue = g.Sum(i => i.Cantidad * i.Articulo!.Precio),
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Generated inventory summary with {Count} locations for institution {InstitucionId}",
                summaries.Count,
                institucionId
            );

            return ApiResponse<IEnumerable<InventorySummaryDto>>.Success(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting inventory summary for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<InventorySummaryDto>>.Failure(
                "Error getting inventory summary"
            );
        }
    }

    public async Task<ApiResponse<InventoryStatisticsDto>> GetInventoryStatisticsAsync(
        int institucionId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context.InventarioUnificado.AsNoTracking()
                .Where(i => i.InstitucionID == institucionId);

            if (fromDate.HasValue)
            {
                query = query.Where(i => i.FechaRegistro >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.FechaRegistro <= toDate.Value);
            }

            var inventoryItems = await query
                .Include(i => i.Articulo)
                .ToListAsync(cancellationToken);

            var statistics = new InventoryStatisticsDto
            {
                TotalItems = inventoryItems.Sum(i => i.Cantidad),
                TotalValue = inventoryItems.Sum(i => i.Cantidad * (i.Articulo?.Precio ?? 0)),
                UniqueArticles = inventoryItems.Count,
                LocationBreakdown = inventoryItems
                    .GroupBy(i => i.TipoUbicacion)
                    .Select(g => new LocationStatisticsDto
                    {
                        LocationType = (InventoryLocationType)g.Key,
                        TotalItems = g.Sum(i => i.Cantidad),
                        TotalValue = g.Sum(i => i.Cantidad * (i.Articulo?.Precio ?? 0)),
                        UniqueArticles = g.Count(),
                    })
                    .ToList(),
                LowStockItems = inventoryItems
                    .Where(i => i.Cantidad <= 5) // Configurable threshold
                    .Count(),
                ZeroStockItems = inventoryItems
                    .Where(i => i.Cantidad == 0)
                    .Count(),
                GeneratedDate = DateTime.UtcNow,
                PeriodStart = fromDate,
                PeriodEnd = toDate,
            };

            _logger.LogInformation(
                "Generated inventory statistics for institution {InstitucionId}: {TotalItems} items, {TotalValue:C} value",
                institucionId,
                statistics.TotalItems,
                statistics.TotalValue
            );

            return ApiResponse<InventoryStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting inventory statistics for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<InventoryStatisticsDto>.Failure("Error getting inventory statistics");
        }
    }

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetLowStockItemsAsync(
        int institucionId,
        int threshold = 5,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var lowStockItems = await _context
                .InventarioUnificado.AsNoTracking()
                .Where(i => i.InstitucionID == institucionId && i.Cantidad <= threshold)
                .Include(i => i.Articulo)
                .ThenInclude(a => a!.Imagen)
                .Include(i => i.Habitacion)
                .ToListAsync(cancellationToken);

            var inventoryDtos = lowStockItems.Select(i => new InventoryDto
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

            _logger.LogInformation(
                "Found {Count} low stock items (threshold: {Threshold}) for institution {InstitucionId}",
                lowStockItems.Count,
                threshold,
                institucionId
            );

            return ApiResponse<IEnumerable<InventoryDto>>.Success(inventoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting low stock items for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<InventoryDto>>.Failure("Error getting low stock items");
        }
    }

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetCombinedInventoryAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var roomInventory = await _context
                .InventarioUnificado.AsNoTracking()
                .Where(i =>
                    i.InstitucionID == institucionId
                    && i.TipoUbicacion == (int)InventoryLocationType.Room
                    && i.UbicacionId == habitacionId
                )
                .Include(i => i.Articulo)
                .ThenInclude(a => a!.Imagen)
                .Include(i => i.Habitacion)
                .ToListAsync(cancellationToken);

            var generalInventory = await _context
                .InventarioUnificado.AsNoTracking()
                .Where(i =>
                    i.InstitucionID == institucionId
                    && i.TipoUbicacion == (int)InventoryLocationType.General
                )
                .Include(i => i.Articulo)
                .ThenInclude(a => a!.Imagen)
                .ToListAsync(cancellationToken);

            var combined = roomInventory.Concat(generalInventory);

            var inventoryDtos = combined.Select(i => new InventoryDto
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

            _logger.LogInformation(
                "Generated combined inventory report for room {HabitacionId}: {RoomItems} room items, {GeneralItems} general items",
                habitacionId,
                roomInventory.Count(),
                generalInventory.Count()
            );

            return ApiResponse<IEnumerable<InventoryDto>>.Success(inventoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting combined inventory for room {HabitacionId} in institution {InstitucionId}",
                habitacionId,
                institucionId
            );
            return ApiResponse<IEnumerable<InventoryDto>>.Failure(
                "Error getting combined inventory"
            );
        }
    }

    private string GetLocationName(hotel.Models.InventarioUnificado inventory)
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