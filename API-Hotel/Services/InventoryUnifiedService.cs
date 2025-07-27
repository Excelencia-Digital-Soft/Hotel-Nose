using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Refactored service implementation for unified inventory management using specialized services
/// This service now delegates complex operations to specialized services while maintaining core inventory operations
/// </summary>
public class InventoryUnifiedService : IInventoryService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<InventoryUnifiedService> _logger;
    private readonly IInventoryMovementService _movementService;
    private readonly IInventoryAlertService _alertService;
    private readonly IInventoryTransferService _transferService;

    public InventoryUnifiedService(
        HotelDbContext context,
        ILogger<InventoryUnifiedService> logger,
        IInventoryMovementService movementService,
        IInventoryAlertService alertService,
        IInventoryTransferService transferService
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _movementService =
            movementService ?? throw new ArgumentNullException(nameof(movementService));
        _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
        _transferService =
            transferService ?? throw new ArgumentNullException(nameof(transferService));
    }

    #region Core Inventory Management

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
                ArticuloDescripcion = "", // Not available in current model
                ArticuloPrecio = i.Articulo?.Precio ?? 0,
                ArticuloImagenUrl = i.Articulo?.Imagen?.Origen,
                Cantidad = i.Cantidad,
                LocationType = (InventoryLocationType)i.TipoUbicacion,
                LocationId = i.UbicacionId,
                LocationName = GetLocationName(i),
                InstitucionId = i.InstitucionID,
                FechaRegistro = i.FechaRegistro,
                UserId = i.UsuarioRegistro,
                UserName = "", // Would require join with ApplicationUser
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
                ArticuloDescripcion = "", // Not available in current model
                ArticuloPrecio = inventory.Articulo?.Precio ?? 0,
                ArticuloImagenUrl = inventory.Articulo?.Imagen?.Origen,
                Cantidad = inventory.Cantidad,
                LocationType = (InventoryLocationType)inventory.TipoUbicacion,
                LocationId = inventory.UbicacionId,
                LocationName = GetLocationName(inventory),
                InstitucionId = inventory.InstitucionID,
                FechaRegistro = inventory.FechaRegistro,
                UserId = inventory.UsuarioRegistro,
                UserName = "", // Would require join with ApplicationUser
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

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetInventoryByArticleAsync(
        int articuloId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var inventory = await _context
                .InventarioUnificado.AsNoTracking()
                .Where(i => i.ArticuloId == articuloId && i.InstitucionID == institucionId)
                .Include(i => i.Articulo)
                .ThenInclude(a => a!.Imagen)
                .Include(i => i.Habitacion)
                .ToListAsync(cancellationToken);

            var inventoryDtos = inventory.Select(i => new InventoryDto
            {
                InventoryId = i.InventarioId,
                ArticuloId = i.ArticuloId,
                ArticuloNombre = i.Articulo?.NombreArticulo ?? "Desconocido",
                ArticuloDescripcion = "", // Not available in current model
                ArticuloPrecio = i.Articulo?.Precio ?? 0,
                ArticuloImagenUrl = i.Articulo?.Imagen?.Origen,
                Cantidad = i.Cantidad,
                LocationType = (InventoryLocationType)i.TipoUbicacion,
                LocationId = i.UbicacionId,
                LocationName = GetLocationName(i),
                InstitucionId = i.InstitucionID,
                FechaRegistro = i.FechaRegistro,
                UserId = i.UsuarioRegistro,
                UserName = "", // Would require join with ApplicationUser
                IsActive = i.Anulado != true,
            });

            return ApiResponse<IEnumerable<InventoryDto>>.Success(inventoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving inventory for article {ArticuloId} in institution {InstitucionId}",
                articuloId,
                institucionId
            );
            return ApiResponse<IEnumerable<InventoryDto>>.Failure(
                "Error retrieving inventory by article"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetRoomInventoryAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        return await GetInventoryAsync(
            institucionId,
            InventoryLocationType.Room,
            habitacionId,
            cancellationToken
        );
    }

    public async Task<ApiResponse<InventoryDto>> AddRoomInventoryAsync(
        int habitacionId,
        InventoryCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        var roomCreateDto = new InventoryCreateDto
        {
            ArticuloId = createDto.ArticuloId,
            LocationType = InventoryLocationType.Room,
            LocationId = habitacionId,
            Cantidad = createDto.Cantidad,
        };

        return await CreateInventoryAsync(roomCreateDto, institucionId, cancellationToken);
    }

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetGeneralInventoryAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        return await GetInventoryAsync(
            institucionId,
            InventoryLocationType.General,
            null,
            cancellationToken
        );
    }

    public async Task<ApiResponse<InventoryDto>> AddGeneralInventoryAsync(
        InventoryCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        var generalCreateDto = new InventoryCreateDto
        {
            ArticuloId = createDto.ArticuloId,
            LocationType = InventoryLocationType.General,
            LocationId = null,
            Cantidad = createDto.Cantidad,
        };

        return await CreateInventoryAsync(generalCreateDto, institucionId, cancellationToken);
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

    public async Task<ApiResponse<InventoryDto>> CreateInventoryAsync(
        InventoryCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

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
                UsuarioRegistro = "system",
            };

            _context.InventarioUnificado.Add(inventory);
            await _context.SaveChangesAsync(cancellationToken);

            // Record initial movement if quantity > 0
            if (createDto.Cantidad > 0)
            {
                await _movementService.CreateMovementAsync(
                    new MovimientoInventarioCreateDto
                    {
                        InventarioId = inventory.InventarioId,
                        TipoMovimiento = "Entrada",
                        CantidadAnterior = 0,
                        CantidadNueva = createDto.Cantidad,
                        CantidadCambiada = createDto.Cantidad,
                        Motivo = "Creación de inventario inicial",
                    },
                    institucionId,
                    "system",
                    cancellationToken: cancellationToken
                );
            }

            await transaction.CommitAsync(cancellationToken);

            // Return created inventory
            var result = await GetInventoryByIdAsync(
                inventory.InventarioId,
                institucionId,
                cancellationToken
            );

            _logger.LogInformation(
                "Inventory created for article {ArticuloId} in location {TipoUbicacion}:{UbicacionId}",
                createDto.ArticuloId,
                createDto.LocationType,
                createDto.LocationId
            );

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
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
        InventoryUpdateDto updateDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

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

            var previousQuantity = inventory.Cantidad;
            inventory.Cantidad = updateDto.Cantidad;
            inventory.FechaUltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Record adjustment movement
            await _movementService.RecordAdjustmentAsync(
                inventoryId,
                updateDto.Cantidad,
                updateDto.Notes ?? "Ajuste manual de inventario",
                institucionId,
                userId,
                cancellationToken: cancellationToken
            );

            // Check and generate alerts
            await _alertService.CheckAndGenerateAlertsAsync(
                inventoryId,
                institucionId,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);

            var result = await GetInventoryByIdAsync(inventoryId, institucionId, cancellationToken);

            _logger.LogInformation(
                "Inventory {InventoryId} updated: {PreviousQuantity} -> {NewQuantity}",
                inventoryId,
                previousQuantity,
                updateDto.Cantidad
            );

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
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

    #endregion

    #region Room Management

    public async Task<ApiResponse<IEnumerable<RoomInventoryDto>>> GetRoomInventoryDetailedAsync(
        int roomId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var roomInventory = await _context
                .InventarioUnificado.AsNoTracking()
                .Where(i =>
                    i.TipoUbicacion == (int)InventoryLocationType.Room
                    && i.UbicacionId == roomId
                    && i.InstitucionID == institucionId
                )
                .Include(i => i.Articulo)
                .ThenInclude(a => a!.Imagen)
                .Include(i => i.Habitacion)
                .ToListAsync(cancellationToken);

            var roomInventoryDtos = roomInventory.Select(i => new RoomInventoryDto
            {
                InventoryId = i.InventarioId,
                ArticuloId = i.ArticuloId,
                NombreArticulo = i.Articulo?.NombreArticulo ?? "Desconocido",
                CodigoArticulo = i.Articulo?.ArticuloId.ToString() ?? "",
                ImagenUrl = i.Articulo?.Imagen?.Origen,
                Cantidad = i.Cantidad,
                RoomId = roomId,
                NombreHabitacion = i.Habitacion?.NombreHabitacion ?? $"Habitación {roomId}",
                FechaActualizacion = i.FechaUltimaActualizacion,
            });

            return ApiResponse<IEnumerable<RoomInventoryDto>>.Success(roomInventoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving room inventory for room {RoomId} in institution {InstitucionId}",
                roomId,
                institucionId
            );
            return ApiResponse<IEnumerable<RoomInventoryDto>>.Failure(
                "Error retrieving room inventory"
            );
        }
    }

    public async Task<ApiResponse<RoomInventoryDto>> AddToRoomInventoryAsync(
        RoomInventoryCreateDto createDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await CreateInventoryFromRoomDto(
            createDto,
            institucionId,
            userId,
            cancellationToken
        );
    }

    public async Task<ApiResponse<RoomInventoryDto>> UpdateRoomInventoryAsync(
        int inventoryId,
        RoomInventoryUpdateDto updateDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var inventoryUpdateDto = new InventoryUpdateDto
        {
            Cantidad = updateDto.NuevaCantidad,
            Notes = updateDto.Motivo,
        };

        var updateResult = await UpdateInventoryQuantityAsync(
            inventoryId,
            inventoryUpdateDto,
            institucionId,
            userId,
            cancellationToken
        );

        if (!updateResult.IsSuccess)
        {
            return ApiResponse<RoomInventoryDto>.Failure(updateResult.Errors, updateResult.Message);
        }

        // Convert to RoomInventoryDto
        var inventory = updateResult.Data!;
        var roomInventoryDto = new RoomInventoryDto
        {
            InventoryId = inventory.InventoryId,
            ArticuloId = inventory.ArticuloId,
            NombreArticulo = inventory.ArticuloNombre,
            CodigoArticulo = inventory.ArticuloId.ToString(), // Generate from ID
            ImagenUrl = inventory.ArticuloImagenUrl,
            Cantidad = inventory.Cantidad,
            RoomId = inventory.LocationId ?? 0,
            NombreHabitacion = inventory.LocationName ?? "",
            FechaActualizacion = inventory.FechaRegistro, // Use the closest available
        };

        return ApiResponse<RoomInventoryDto>.Success(roomInventoryDto, updateResult.Message);
    }

    public async Task<ApiResponse> RemoveFromRoomInventoryAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        return await DeleteInventoryAsync(inventoryId, institucionId, cancellationToken);
    }

    #endregion

    #region Stock Replenishment

    public async Task<ApiResponse<InventoryDto>> ReplenishStockAsync(
        ReplenishStockDto replenishDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var inventory = await _context.InventarioUnificado.FirstOrDefaultAsync(
                i => i.InventarioId == replenishDto.InventoryId && i.InstitucionID == institucionId,
                cancellationToken
            );

            if (inventory == null)
            {
                return ApiResponse<InventoryDto>.Failure("Inventory not found");
            }

            var previousQuantity = inventory.Cantidad;
            inventory.Cantidad += replenishDto.Cantidad;
            inventory.FechaUltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Record movement
            await _movementService.CreateMovementAsync(
                new MovimientoInventarioCreateDto
                {
                    InventarioId = replenishDto.InventoryId,
                    TipoMovimiento = "Entrada",
                    CantidadAnterior = previousQuantity,
                    CantidadNueva = inventory.Cantidad,
                    CantidadCambiada = replenishDto.Cantidad,
                    Motivo = replenishDto.Motivo ?? "Reabastecimiento de stock",
                    NumeroDocumento = replenishDto.NumeroDocumento,
                },
                institucionId,
                userId,
                ipAddress,
                cancellationToken
            );

            // Check and generate alerts
            await _alertService.CheckAndGenerateAlertsAsync(
                replenishDto.InventoryId,
                institucionId,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);

            var result = await GetInventoryByIdAsync(
                replenishDto.InventoryId,
                institucionId,
                cancellationToken
            );

            _logger.LogInformation(
                "Stock replenished for inventory {InventoryId}: +{Quantity} units",
                replenishDto.InventoryId,
                replenishDto.Cantidad
            );

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error replenishing stock for inventory {InventoryId}",
                replenishDto.InventoryId
            );
            return ApiResponse<InventoryDto>.Failure("Error replenishing stock");
        }
    }

    #endregion

    #region Delegated Operations (using specialized services)

    // Movement operations - delegate to IInventoryMovementService
    public Task<ApiResponse<MovimientoInventarioDto>> CreateMovementAsync(
        MovimientoInventarioCreateDto movementDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        return _movementService.CreateMovementAsync(
            movementDto,
            institucionId,
            userId,
            ipAddress,
            cancellationToken
        );
    }

    public Task<ApiResponse<PagedResult<MovimientoInventarioDto>>> GetMovementsAsync(
        int inventoryId,
        int institucionId,
        MovimientoInventarioFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        return _movementService.GetMovementsAsync(
            inventoryId,
            institucionId,
            filter,
            cancellationToken
        );
    }

    public Task<ApiResponse<MovimientoInventarioDto>> GetMovementByIdAsync(
        int movementId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        return _movementService.GetMovementByIdAsync(movementId, institucionId, cancellationToken);
    }

    public Task<ApiResponse<MovimientoEstadisticasDto>> GetMovementStatisticsAsync(
        int institucionId,
        MovimientoEstadisticasFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        return _movementService.GetMovementStatisticsAsync(
            institucionId,
            filter,
            cancellationToken
        );
    }

    // Alert operations - delegate to IInventoryAlertService
    public Task<ApiResponse<PagedResult<AlertaInventarioDto>>> GetActiveAlertsAsync(
        int institucionId,
        AlertaInventarioFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        return _alertService.GetActiveAlertsAsync(institucionId, filter, cancellationToken);
    }

    public Task<ApiResponse<ConfiguracionAlertaDto>> ConfigureAlertsAsync(
        ConfiguracionAlertaCreateUpdateDto configDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return _alertService.ConfigureAlertsAsync(
            configDto,
            institucionId,
            userId,
            cancellationToken
        );
    }

    public Task<ApiResponse<AlertaInventarioDto>> AcknowledgeAlertAsync(
        int alertId,
        AlertaReconocimientoDto acknowledgmentDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return _alertService.AcknowledgeAlertAsync(
            alertId,
            acknowledgmentDto,
            institucionId,
            userId,
            cancellationToken
        );
    }

    public Task<ApiResponse<ConfiguracionAlertaDto>> GetAlertConfigurationAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        return _alertService.GetAlertConfigurationAsync(
            inventoryId,
            institucionId,
            cancellationToken
        );
    }

    // Transfer operations - delegate to IInventoryTransferService
    public Task<ApiResponse<TransferenciaInventarioDto>> CreateTransferAsync(
        TransferenciaCreateDto transferDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        return _transferService.CreateTransferAsync(
            transferDto,
            institucionId,
            userId,
            ipAddress,
            cancellationToken
        );
    }

    public async Task<
        ApiResponse<IEnumerable<TransferenciaInventarioDto>>
    > CreateBatchTransfersAsync(
        TransferenciaBatchCreateDto batchDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _transferService.CreateBatchTransfersAsync(
            batchDto,
            institucionId,
            userId,
            ipAddress,
            cancellationToken
        );
        return ApiResponse<IEnumerable<TransferenciaInventarioDto>>.Success(
            result.Data ?? new List<TransferenciaInventarioDto>()
        );
    }

    public Task<ApiResponse<PagedResult<TransferenciaInventarioDto>>> GetTransfersAsync(
        int institucionId,
        TransferenciaInventarioFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        return _transferService.GetTransfersAsync(institucionId, filter, cancellationToken);
    }

    public Task<ApiResponse<TransferenciaInventarioDto>> ApproveTransferAsync(
        int transferId,
        TransferenciaApprovalDto approvalDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return _transferService.ApproveTransferAsync(
            transferId,
            approvalDto,
            institucionId,
            userId,
            cancellationToken
        );
    }

    public Task<ApiResponse<TransferenciaInventarioDto>> GetTransferByIdAsync(
        int transferId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        return _transferService.GetTransferByIdAsync(transferId, institucionId, cancellationToken);
    }

    #endregion

    #region Missing Interface Methods

    public async Task<ApiResponse<InventoryDto>> UpdateInventoryAsync(
        int inventoryId,
        InventoryUpdateDto updateDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        return await UpdateInventoryQuantityAsync(
            inventoryId,
            updateDto,
            institucionId,
            "system",
            cancellationToken
        );
    }

    public async Task<ApiResponse<string>> BatchUpdateInventoryAsync(
        InventoryBatchUpdateDto batchUpdateDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            int updatedCount = 0;

            foreach (var update in batchUpdateDto.Items)
            {
                var result = await UpdateInventoryQuantityAsync(
                    update.InventoryId,
                    new InventoryUpdateDto
                    {
                        Cantidad = update.Cantidad,
                        Notes = "Actualización en lote",
                    },
                    institucionId,
                    "system",
                    cancellationToken
                );

                if (result.IsSuccess)
                {
                    updatedCount++;
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return ApiResponse<string>.Success(
                $"Successfully updated {updatedCount} inventory items"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error in batch update for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<string>.Failure("Error in batch update");
        }
    }

    public async Task<ApiResponse<MovimientoInventarioDto>> RegisterMovementAsync(
        MovimientoInventarioCreateDto movementDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // First, get and update the inventory
            var inventory = await _context.InventarioUnificado.FirstOrDefaultAsync(
                i => i.InventarioId == movementDto.InventarioId && i.InstitucionID == institucionId,
                cancellationToken
            );

            if (inventory == null)
            {
                return ApiResponse<MovimientoInventarioDto>.Failure("Inventory not found");
            }

            // Update inventory quantity to the new quantity specified in the movement
            inventory.Cantidad = movementDto.CantidadNueva;
            inventory.FechaUltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Then record the movement
            var movementResult = await _movementService.CreateMovementAsync(
                movementDto,
                institucionId,
                userId,
                ipAddress,
                cancellationToken
            );

            if (!movementResult.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                return movementResult;
            }

            // Check and generate alerts after inventory update
            await _alertService.CheckAndGenerateAlertsAsync(
                movementDto.InventarioId,
                institucionId,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Inventory {InventoryId} updated from {OldQuantity} to {NewQuantity} and movement recorded",
                movementDto.InventarioId,
                movementDto.CantidadAnterior,
                movementDto.CantidadNueva
            );

            return movementResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error registering movement for inventory {InventoryId} in institution {InstitucionId}",
                movementDto.InventarioId,
                institucionId
            );
            return ApiResponse<MovimientoInventarioDto>.Failure("Error registering movement and updating inventory");
        }
    }

    public async Task<ApiResponse<MovimientoInventarioResumenDto>> GetInventoryMovementsAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Get recent movements using the existing movement service
            var filter = new MovimientoInventarioFilterDto
            {
                Pagina = 1,
                TamanoPagina = 5, // Get last 5 movements
                OrdenarPor = "fecha",
                Descendente = true,
            };

            var movementsResult = await _movementService.GetMovementsAsync(
                inventoryId,
                institucionId,
                filter,
                cancellationToken
            );

            if (!movementsResult.IsSuccess)
            {
                return ApiResponse<MovimientoInventarioResumenDto>.Failure(
                    movementsResult.Errors,
                    movementsResult.Message
                );
            }

            // Get inventory info
            var inventory = await _context
                .InventarioUnificado.AsNoTracking()
                .Include(i => i.Articulo)
                .FirstOrDefaultAsync(
                    i => i.InventarioId == inventoryId && i.InstitucionID == institucionId,
                    cancellationToken
                );

            if (inventory == null)
            {
                return ApiResponse<MovimientoInventarioResumenDto>.Failure("Inventory not found");
            }

            // Calculate statistics from all movements
            var allMovements = await _context
                .MovimientosInventario.AsNoTracking()
                .Where(m => m.InventarioId == inventoryId && m.InstitucionID == institucionId)
                .ToListAsync(cancellationToken);

            var totalMovements = allMovements.Count;
            var totalEntradas = allMovements.Count(m => m.CantidadCambiada > 0);
            var totalSalidas = allMovements.Count(m => m.CantidadCambiada < 0);
            var lastMovement = allMovements
                .OrderByDescending(m => m.FechaMovimiento)
                .FirstOrDefault();

            var resumen = new MovimientoInventarioResumenDto
            {
                InventarioId = inventoryId,
                ArticuloId = inventory.ArticuloId,
                ArticuloNombre = inventory.Articulo?.NombreArticulo ?? "Desconocido",
                CantidadActual = inventory.Cantidad,
                TotalMovimientos = totalMovements,
                TotalEntradas = totalEntradas,
                TotalSalidas = totalSalidas,
                UltimoMovimiento = lastMovement?.FechaMovimiento,
                TipoUltimoMovimiento = lastMovement?.TipoMovimiento,
                MovimientosRecientes = movementsResult.Data!.Items.ToList(),
            };

            return ApiResponse<MovimientoInventarioResumenDto>.Success(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting inventory movements for {InventoryId}",
                inventoryId
            );
            return ApiResponse<MovimientoInventarioResumenDto>.Failure(
                "Error retrieving inventory movements"
            );
        }
    }

    public async Task<ApiResponse<MovimientoAuditoriaResponseDto>> GetMovementAuditAsync(
        MovimientoAuditoriaRequestDto request,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Convert request to movement filter
            var filter = new MovimientoInventarioFilterDto
            {
                TipoMovimiento = request.TipoMovimiento,
                FechaDesde = request.FechaInicio,
                FechaHasta = request.FechaFin,
                UsuarioId = request.UsuarioId,
                Pagina = request.Pagina,
                TamanoPagina = request.TamañoPagina,
                OrdenarPor = "fecha",
                Descendente = true,
            };

            // Get statistics for the institution
            var statsFilter = new MovimientoEstadisticasFilterDto
            {
                FechaDesde = request.FechaInicio,
                FechaHasta = request.FechaFin,
            };

            var statisticsResult = await _movementService.GetMovementStatisticsAsync(
                institucionId,
                statsFilter,
                cancellationToken
            );

            // Get movements for all inventory items matching criteria
            var query = _context
                .MovimientosInventario.AsNoTracking()
                .Include(m => m.Inventario)
                .ThenInclude(i => i!.Articulo)
                .Include(m => m.Usuario)
                .Where(m => m.InstitucionID == institucionId);

            // Apply filters
            if (!string.IsNullOrEmpty(request.TipoMovimiento))
            {
                query = query.Where(m => m.TipoMovimiento == request.TipoMovimiento);
            }

            if (request.FechaInicio.HasValue)
            {
                query = query.Where(m => m.FechaMovimiento >= request.FechaInicio.Value);
            }

            if (request.FechaFin.HasValue)
            {
                query = query.Where(m => m.FechaMovimiento <= request.FechaFin.Value);
            }

            if (request.ArticuloId.HasValue)
            {
                query = query.Where(m => m.Inventario!.ArticuloId == request.ArticuloId.Value);
            }

            if (request.TipoUbicacion.HasValue)
            {
                query = query.Where(m =>
                    m.Inventario!.TipoUbicacion == request.TipoUbicacion.Value
                );
            }

            if (request.UbicacionId.HasValue)
            {
                query = query.Where(m => m.Inventario!.UbicacionId == request.UbicacionId.Value);
            }

            if (!string.IsNullOrEmpty(request.UsuarioId))
            {
                query = query.Where(m => m.UsuarioId == request.UsuarioId);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting and pagination
            var movements = await query
                .OrderByDescending(m => m.FechaMovimiento)
                .Skip((request.Pagina - 1) * request.TamañoPagina)
                .Take(request.TamañoPagina)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var movementDtos = movements
                .Select(m => new MovimientoInventarioDto
                {
                    MovimientoId = m.MovimientoId,
                    InventarioId = m.InventarioId,
                    InstitucionId = m.InstitucionID,
                    TipoMovimiento = m.TipoMovimiento,
                    CantidadAnterior = m.CantidadAnterior,
                    CantidadNueva = m.CantidadNueva,
                    CantidadCambiada = m.CantidadCambiada,
                    Motivo = m.Motivo,
                    NumeroDocumento = m.NumeroDocumento,
                    TransferenciaId = m.TransferenciaId,
                    FechaMovimiento = m.FechaMovimiento,
                    UsuarioId = m.UsuarioId,
                    NombreUsuario = m.Usuario?.UserName ?? "Usuario Desconocido",
                    DireccionIP = m.DireccionIP,
                    NombreArticulo =
                        m.Inventario?.Articulo?.NombreArticulo ?? "Artículo Desconocido",
                    CodigoArticulo = m.Inventario?.Articulo?.ArticuloId.ToString() ?? "",
                })
                .ToList();

            var response = new MovimientoAuditoriaResponseDto
            {
                Movimientos = movementDtos,
                TotalRegistros = totalCount,
                PaginaActual = request.Pagina,
                TotalPaginas = (int)Math.Ceiling((double)totalCount / request.TamañoPagina),
                TienePaginaAnterior = request.Pagina > 1,
                TienePaginaSiguiente =
                    request.Pagina < (int)Math.Ceiling((double)totalCount / request.TamañoPagina),
                Estadisticas = statisticsResult.IsSuccess
                    ? statisticsResult.Data!
                    : new MovimientoEstadisticasDto(),
            };

            return ApiResponse<MovimientoAuditoriaResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting movement audit for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<MovimientoAuditoriaResponseDto>.Failure(
                "Error retrieving movement audit"
            );
        }
    }

    public async Task<ApiResponse<AlertasActivasResumenDto>> GetActiveAlertsAsync(
        AlertaFiltroRequestDto request,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Convert request to alert filter
            var filter = new AlertaInventarioFilterDto
            {
                TipoAlerta = request.TipoAlerta,
                Severidad = request.Severidad,
                Pagina = request.Pagina,
                InventarioId = request.ArticuloId,
                OrdenarPor = request.OrdenarPor,
                Descendente = request.OrdenDescendente,
            };

            // Get active alerts
            var alertsResult = await _alertService.GetActiveAlertsAsync(
                institucionId,
                filter,
                cancellationToken
            );

            if (!alertsResult.IsSuccess)
            {
                return ApiResponse<AlertasActivasResumenDto>.Failure(
                    alertsResult.Errors,
                    alertsResult.Message
                );
            }

            // Get alert statistics
            var statsFilter = new AlertaEstadisticasFilterDto
            {
                FechaDesde = request.FechaInicio,
                FechaHasta = request.FechaFin,
            };

            var statisticsResult = await _alertService.GetAlertStatisticsAsync(
                institucionId,
                statsFilter,
                cancellationToken
            );

            var resumen = new AlertasActivasResumenDto
            {
                TotalAlertas = alertsResult.Data!.TotalCount,
                AlertasCriticas = alertsResult.Data.Items.Count(a => a.Severidad == "Alta"),
                AlertasMedias = alertsResult.Data.Items.Count(a => a.Severidad == "Media"),
                AlertasBajas = alertsResult.Data.Items.Count(a => a.Severidad == "Baja"),
                AlertasRecientes = alertsResult.Data.Items.Take(5).ToList(),
            };

            return ApiResponse<AlertasActivasResumenDto>.Success(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting active alerts summary for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<AlertasActivasResumenDto>.Failure(
                "Error retrieving active alerts summary"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<TransferenciaInventarioDto>>> GetTransfersAsync(
        TransferenciaInventarioFilterDto request,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _transferService.GetTransfersAsync(
            institucionId,
            request,
            cancellationToken
        );
        return ApiResponse<IEnumerable<TransferenciaInventarioDto>>.Success(
            result.Data?.Items ?? new List<TransferenciaInventarioDto>()
        );
    }

    public async Task<ApiResponse<TransferenciaInventarioDto>> ApproveTransferAsync(
        int transferId,
        TransferenciaAprobacionDto approvalDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _transferService.ApproveTransferAsync(
            transferId,
            new TransferenciaApprovalDto { Approved = true, Comments = approvalDto.Comentarios },
            institucionId,
            userId,
            cancellationToken
        );
    }

    public async Task<ApiResponse<string>> TransferInventoryAsync(
        InventoryTransferDto transferDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // First, find the inventory item in the source location
            var sourceInventory = await _context
                .InventarioUnificado.AsNoTracking()
                .FirstOrDefaultAsync(
                    i =>
                        i.ArticuloId == transferDto.ArticuloId
                        && i.TipoUbicacion == (int)transferDto.FromLocationType
                        && i.UbicacionId == transferDto.FromLocationId
                        && i.InstitucionID == institucionId,
                    cancellationToken
                );

            if (sourceInventory == null)
            {
                return ApiResponse<string>.Failure(
                    "Source inventory not found for the specified article and location"
                );
            }

            if (sourceInventory.Cantidad < transferDto.Cantidad)
            {
                return ApiResponse<string>.Failure(
                    $"Insufficient inventory. Available: {sourceInventory.Cantidad}, Requested: {transferDto.Cantidad}"
                );
            }

            // Create the transfer with proper details
            var createDto = new TransferenciaCreateDto
            {
                TipoUbicacionOrigen = (int)transferDto.FromLocationType,
                UbicacionIdOrigen = transferDto.FromLocationId,
                TipoUbicacionDestino = (int)transferDto.ToLocationType,
                UbicacionIdDestino = transferDto.ToLocationId,
                Motivo = transferDto.Notes ?? "Transferencia de inventario",
                Notas = transferDto.Notes,
                Prioridad = "Media",
                RequiereAprobacion = false, // Set to false for immediate transfer
                Detalles = new List<DetalleTransferenciaCreateDto>
                {
                    new DetalleTransferenciaCreateDto
                    {
                        InventarioId = sourceInventory.InventarioId,
                        CantidadSolicitada = transferDto.Cantidad,
                        Notas = transferDto.Notes,
                    },
                },
            };

            var result = await _transferService.CreateTransferAsync(
                createDto,
                institucionId,
                "system",
                cancellationToken: cancellationToken
            );

            if (result.IsSuccess)
            {
                return ApiResponse<string>.Success("Transfer created successfully");
            }
            else
            {
                return ApiResponse<string>.Failure(result.Errors, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error in legacy transfer for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<string>.Failure("Error in transfer");
        }
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

    public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetCombinedInventoryAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var roomInventory = await GetRoomInventoryAsync(
                habitacionId,
                institucionId,
                cancellationToken
            );
            var generalInventory = await GetGeneralInventoryAsync(institucionId, cancellationToken);

            var combined = new List<InventoryDto>();

            if (roomInventory.IsSuccess && roomInventory.Data != null)
                combined.AddRange(roomInventory.Data);

            if (generalInventory.IsSuccess && generalInventory.Data != null)
                combined.AddRange(generalInventory.Data);

            return ApiResponse<IEnumerable<InventoryDto>>.Success(combined);
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

    public async Task<ApiResponse<StockValidationDto>> ValidateStockAsync(
        int articuloId,
        int requestedQuantity,
        InventoryLocationType locationType,
        int? locationId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .InventarioUnificado.AsNoTracking()
                .Where(i =>
                    i.ArticuloId == articuloId
                    && i.InstitucionID == institucionId
                    && i.TipoUbicacion == (int)locationType
                );

            if (locationId.HasValue)
            {
                query = query.Where(i => i.UbicacionId == locationId.Value);
            }

            var inventory = await query.FirstOrDefaultAsync(cancellationToken);

            var validation = new StockValidationDto
            {
                ArticuloId = articuloId,
                RequestedQuantity = requestedQuantity,
                AvailableQuantity = inventory?.Cantidad ?? 0,
                IsValid = (inventory?.Cantidad ?? 0) >= requestedQuantity,
                LocationType = locationType,
                LocationId = locationId,
            };

            return ApiResponse<StockValidationDto>.Success(validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating stock for article {ArticuloId} in institution {InstitucionId}",
                articuloId,
                institucionId
            );
            return ApiResponse<StockValidationDto>.Failure("Error validating stock");
        }
    }

    #endregion

    #region Private Helper Methods

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

    private async Task<ApiResponse<RoomInventoryDto>> CreateInventoryFromRoomDto(
        RoomInventoryCreateDto createDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken
    )
    {
        var inventoryCreateDto = new InventoryCreateDto
        {
            ArticuloId = createDto.ArticuloId,
            LocationType = InventoryLocationType.Room,
            LocationId = createDto.RoomId,
            Cantidad = createDto.CantidadInicial,
        };

        var createResult = await CreateInventoryAsync(
            inventoryCreateDto,
            institucionId,
            cancellationToken
        );

        if (!createResult.IsSuccess)
        {
            return ApiResponse<RoomInventoryDto>.Failure(createResult.Errors, createResult.Message);
        }

        // Convert to RoomInventoryDto
        var inventory = createResult.Data!;
        var roomInventoryDto = new RoomInventoryDto
        {
            InventoryId = inventory.InventoryId,
            ArticuloId = inventory.ArticuloId,
            NombreArticulo = inventory.ArticuloNombre,
            CodigoArticulo = inventory.ArticuloId.ToString(), // Generate from ID
            ImagenUrl = inventory.ArticuloImagenUrl,
            Cantidad = inventory.Cantidad,
            RoomId = inventory.LocationId ?? 0,
            NombreHabitacion = inventory.LocationName ?? "",
            FechaActualizacion = inventory.FechaRegistro, // Use the closest available
        };

        return ApiResponse<RoomInventoryDto>.Success(roomInventoryDto, createResult.Message);
    }

    #endregion
}
