using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.DTOs.UserConsumption;
using hotel.Interfaces;
using hotel.Models;
using hotel.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Refactored UserConsumptionService with reduced code duplication and better separation of concerns
/// Uses InventoryUnifiedService for all inventory operations
/// </summary>
public class UserConsumptionService : IUserConsumptionService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<UserConsumptionService> _logger;
    private readonly IInventoryUnifiedService _inventoryService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserConsumptionService(
        HotelDbContext context,
        ILogger<UserConsumptionService> logger,
        IInventoryUnifiedService inventoryService,
        UserManager<ApplicationUser> userManager
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _inventoryService =
            inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    #region Read Operations

    public async Task<ApiResponse<IEnumerable<UserConsumptionDto>>> GetUserConsumptionAsync(
        string userId,
        int institucionId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = BuildBaseConsumptionQuery(institucionId)
                .Where(c => c.Movimientos!.UserId == userId);

            query = ApplyDateFilters(query, startDate, endDate);

            var consumosData = await query
                .OrderByDescending(c => c.Movimientos!.FechaRegistro)
                .ToListAsync(cancellationToken);

            var consumos = consumosData
                .Select(c => MapToUserConsumptionDto(c, userId))
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} consumption records for user {UserId} in institution {InstitucionId}",
                consumos.Count(),
                userId,
                institucionId
            );

            return ApiResponse<IEnumerable<UserConsumptionDto>>.Success(consumos);
        }
        catch (Exception ex)
        {
            return LogAndReturnError<IEnumerable<UserConsumptionDto>>(
                ex,
                "Error retrieving user consumption for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );
        }
    }

    public async Task<ApiResponse<UserConsumptionSummaryDto>> GetUserConsumptionSummaryAsync(
        string userId,
        int institucionId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var periodStart = startDate ?? DateTime.Now.AddMonths(-1);
            var periodEnd = endDate ?? DateTime.Now;

            var query = BuildBaseConsumptionQuery(institucionId)
                .Where(c => c.Movimientos!.UserId == userId)
                .Where(c =>
                    c.Movimientos!.FechaRegistro >= periodStart
                    && c.Movimientos!.FechaRegistro <= periodEnd
                );

            var consumos = await query.ToListAsync(cancellationToken);

            var summary = BuildUserConsumptionSummary(consumos, userId, periodStart, periodEnd);

            _logger.LogInformation(
                "Generated consumption summary for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );

            return ApiResponse<UserConsumptionSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            return LogAndReturnError<UserConsumptionSummaryDto>(
                ex,
                "Error generating consumption summary for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<UserConsumptionDto>>> GetAllUsersConsumptionAsync(
        int institucionId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = BuildBaseConsumptionQueryWithUser(institucionId);
            query = ApplyDateFiltersWithUser(query, startDate, endDate);

            var consumosData = await query
                .OrderByDescending(c => c.Movimientos!.FechaRegistro)
                .ToListAsync(cancellationToken);

            var consumos = consumosData
                .Select(c => MapToUserConsumptionDtoWithUserInfo(c))
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} consumption records for all users in institution {InstitucionId}",
                consumos.Count(),
                institucionId
            );

            return ApiResponse<IEnumerable<UserConsumptionDto>>.Success(consumos);
        }
        catch (Exception ex)
        {
            return LogAndReturnError<IEnumerable<UserConsumptionDto>>(
                ex,
                "Error retrieving all users consumption for institution {InstitucionId}",
                institucionId
            );
        }
    }

    public async Task<
        ApiResponse<IEnumerable<UserConsumptionByServiceDto>>
    > GetConsumptionByServiceAsync(
        string userId,
        int institucionId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = BuildBaseConsumptionQuery(institucionId)
                .Where(c => c.Movimientos!.UserId == userId);

            query = ApplyDateFilters(query, startDate, endDate);
            var consumos = await query.ToListAsync(cancellationToken);

            var result = BuildConsumptionByServiceSummary(consumos);

            _logger.LogInformation(
                "Retrieved consumption by service for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );

            return ApiResponse<IEnumerable<UserConsumptionByServiceDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return LogAndReturnError<IEnumerable<UserConsumptionByServiceDto>>(
                ex,
                "Error retrieving consumption by service for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );
        }
    }

    #endregion

    #region Write Operations

    public async Task<ApiResponse<UserConsumptionDto>> RegisterConsumptionAsync(
        UserConsumptionCreateDto createDto,
        string userId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
         #region log RegisterConsumtionAsync
            
    _logger.LogInformation(
        "📥 RegisterConsumptionAsync called - UserId: {UserId}, InstitucionId: {InstitucionId}, " +
        "ArticuloId: {ArticuloId}, Cantidad: {Cantidad}, TipoConsumo: {TipoConsumo}, HabitacionId: {HabitacionId}",
        userId,
        institucionId,
        createDto.ArticuloId,
        createDto.Cantidad,
        createDto.TipoConsumo,
        createDto.HabitacionId
    );
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get article price
            var precioUnitario = await GetArticlePriceAsync(
                createDto.ArticuloId,
                createDto.PrecioUnitario,
                institucionId,
                cancellationToken
            );
              #endregion
            if (!precioUnitario.HasValue)
            {
                return ApiResponse<UserConsumptionDto>.Failure("Article not found");
            }

            // Validate and consume inventory
            var inventoryResult = await ValidateAndConsumeInventoryAsync(
                createDto.ArticuloId,
                createDto.Cantidad,
                createDto.TipoConsumo ?? "Servicio",
                createDto.HabitacionId,
                institucionId,
                userId,
                "Consumo de usuario",
                cancellationToken
            );

            if (!inventoryResult.IsSuccess)
            {
                return ApiResponse<UserConsumptionDto>.Failure(
                    inventoryResult.Errors,
                    inventoryResult.Message ?? "Error processing inventory"
                );
            }

            // Create movement and consumption
            var movimiento = CreateMovimiento(
                userId,
                institucionId,
                createDto,
                precioUnitario.Value
            );
            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            var consumo = CreateConsumo(movimiento.MovimientosId, createDto, precioUnitario.Value);
            _context.Consumo.Add(consumo);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Return created consumption
            var result = await GetCreatedConsumptionDto(
                consumo.ConsumoId,
                userId,
                cancellationToken
            );

            _logger.LogInformation(
                "Registered consumption {ConsumoId} for user {UserId} in institution {InstitucionId}",
                consumo.ConsumoId,
                userId,
                institucionId
            );

            return ApiResponse<UserConsumptionDto>.Success(
                result,
                "Consumption registered successfully"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return LogAndReturnError<UserConsumptionDto>(
                ex,
            "❌ ERROR in RegisterConsumptionAsync - UserId: {UserId}, ArticuloId: {ArticuloId}, " +
                "Message: {Message}, StackTrace: {StackTrace}",
                userId,
                createDto.ArticuloId,
                ex.Message,
                ex.StackTrace
            );
        }
    }

    public async Task<ApiResponse<UserConsumptionDto>> AdminCreateConsumptionForUserAsync(
        AdminUserConsumptionCreateDto createDto,
        int institucionId,
        string adminUserId,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Validate target user
            var targetUser = await _userManager.FindByIdAsync(createDto.UserId);
            if (targetUser == null)
            {
                return ApiResponse<UserConsumptionDto>.Failure("Target user not found");
            }

            // Get article price
            var precioUnitario = await GetArticlePriceAsync(
                createDto.ArticuloId,
                createDto.PrecioUnitario,
                institucionId,
                cancellationToken
            );
            if (!precioUnitario.HasValue)
            {
                return ApiResponse<UserConsumptionDto>.Failure("Article not found");
            }

            // Validate and consume inventory
            var inventoryResult = await ValidateAndConsumeInventoryAsync(
                createDto.ArticuloId,
                createDto.Cantidad,
                createDto.TipoConsumo ?? "Servicio",
                createDto.HabitacionId,
                institucionId,
                adminUserId,
                $"Consumo asignado por administrador {adminUserId}",
                cancellationToken
            );

            if (!inventoryResult.IsSuccess)
            {
                return ApiResponse<UserConsumptionDto>.Failure(
                    inventoryResult.Errors,
                    inventoryResult.Message ?? "Error processing inventory"
                );
            }

            // Create movement for target user
            var movimiento = CreateMovimientoForAdmin(
                createDto,
                adminUserId,
                institucionId,
                precioUnitario.Value
            );
            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            // Create consumption
            var consumo = CreateConsumoFromAdmin(
                movimiento.MovimientosId,
                createDto,
                precioUnitario.Value
            );
            _context.Consumo.Add(consumo);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Return created consumption with user info
            var result = await GetCreatedConsumptionDtoWithUserInfo(
                consumo.ConsumoId,
                createDto,
                targetUser,
                cancellationToken
            );

            _logger.LogInformation(
                "Admin {AdminUserId} created consumption {ConsumoId} for user {UserId} in institution {InstitucionId}",
                adminUserId,
                consumo.ConsumoId,
                createDto.UserId,
                institucionId
            );

            return ApiResponse<UserConsumptionDto>.Success(
                result,
                "Consumption created successfully for user"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return LogAndReturnError<UserConsumptionDto>(
                ex,
                "Error creating consumption for user {UserId} by admin {AdminUserId} in institution {InstitucionId}",
                createDto.UserId,
                adminUserId,
                institucionId
            );
        }
    }

    public async Task<ApiResponse> CancelConsumptionAsync(
        int consumptionId,
        string userId,
        int institucionId,
        string? reason = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Find and validate consumption
            var consumo = await FindConsumptionForCancellation(
                consumptionId,
                institucionId,
                cancellationToken
            );
            if (consumo == null)
            {
                return ApiResponse.Failure("Consumption not found");
            }

            if (consumo.Anulado == true)
            {
                return ApiResponse.Failure("Consumption is already cancelled");
            }

            // Mark as cancelled
            consumo.Anulado = true;
            await _context.SaveChangesAsync(cancellationToken);

            // Restore inventory
            if (consumo.ArticuloId.HasValue && consumo.Cantidad.HasValue)
            {
                var restoreResult = await RestoreInventoryForCancelledConsumptionAsync(
                    consumo.ArticuloId.Value,
                    consumo.Cantidad.Value,
                    consumo.EsHabitacion == true,
                    consumo.Movimientos?.HabitacionId,
                    reason ?? "Consumo anulado",
                    consumo.ConsumoId.ToString(),
                    institucionId,
                    userId,
                    cancellationToken
                );

                if (!restoreResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to restore inventory for cancelled consumption {ConsumoId}: {Error}",
                        consumo.ConsumoId,
                        restoreResult.Message
                    );
                }
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Consumption {ConsumoId} cancelled by user {UserId} in institution {InstitucionId}",
                consumptionId,
                userId,
                institucionId
            );

            return ApiResponse.Success("Consumption cancelled successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error cancelling consumption {ConsumoId} for institution {InstitucionId}",
                consumptionId,
                institucionId
            );
            return ApiResponse.Failure(
                "Error cancelling consumption",
                "An error occurred while cancelling the consumption"
            );
        }
    }

    #endregion

    #region Private Helper Methods - Query Building

    private IQueryable<Consumo> BuildBaseConsumptionQuery(int institucionId)
    {
        return _context
            .Consumo.AsNoTracking()
            .Include(c => c.Articulo)
            .Include(c => c.Movimientos!)
            .ThenInclude(m => m.Habitacion)
            .Where(c =>
                c.Movimientos != null
                && c.Movimientos.InstitucionID == institucionId
                && (c.Anulado == null || c.Anulado == false)
            );
    }

    private IQueryable<Consumo> BuildBaseConsumptionQueryWithUser(int institucionId)
    {
        return _context
            .Consumo.AsNoTracking()
            .Include(c => c.Articulo)
            .Include(c => c.Movimientos!)
            .ThenInclude(m => m.Habitacion)
            .Include(c => c.Movimientos!)
            .ThenInclude(m => m.User)
            .Where(c =>
                c.Movimientos != null
                && c.Movimientos.InstitucionID == institucionId
                && (c.Anulado == null || c.Anulado == false)
            );
    }

    private IQueryable<Consumo> ApplyDateFilters(
        IQueryable<Consumo> query,
        DateTime? startDate,
        DateTime? endDate
    )
    {
        if (startDate.HasValue)
            query = query.Where(c => c.Movimientos!.FechaRegistro >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.Movimientos!.FechaRegistro <= endDate.Value);

        return query;
    }

    private IQueryable<Consumo> ApplyDateFiltersWithUser(
        IQueryable<Consumo> query,
        DateTime? startDate,
        DateTime? endDate
    )
    {
        return ApplyDateFilters(query, startDate, endDate);
    }

    #endregion

    #region Private Helper Methods - Mapping

    private static UserConsumptionDto MapToUserConsumptionDto(Consumo c, string userId)
    {
        return new UserConsumptionDto
        {
            Id = c.ConsumoId,
            UserId = userId,
            UserName = userId,
            UserFullName = userId,
            ArticuloId = c.ArticuloId ?? 0,
            ArticuloNombre = c.Articulo?.NombreArticulo ?? string.Empty,
            ArticuloCodigo = string.Empty,
            Cantidad = c.Cantidad ?? 0,
            PrecioUnitario = c.PrecioUnitario ?? 0,
            Total = (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0),
            FechaConsumo = c.Movimientos?.FechaRegistro ?? DateTime.Now,
            HabitacionId = c.Movimientos?.HabitacionId,
            HabitacionNumero = c.Movimientos?.Habitacion?.NombreHabitacion,
            ReservaId = null,
            TipoConsumo = c.EsHabitacion == true ? "Habitacion" : "Servicio",
            Observaciones = c.Movimientos?.Descripcion,
            Anulado = c.Anulado ?? false,
        };
    }

    private static UserConsumptionDto MapToUserConsumptionDtoWithUserInfo(Consumo c)
    {
        var user = c.Movimientos?.User;
        var userId = c.Movimientos?.UserId ?? string.Empty;
        
        return new UserConsumptionDto
        {
            Id = c.ConsumoId,
            UserId = userId,
            UserName = user?.UserName ?? userId,
            UserFullName = !string.IsNullOrEmpty(user?.FirstName) || !string.IsNullOrEmpty(user?.LastName)
                ? $"{user?.FirstName} {user?.LastName}".Trim()
                : user?.UserName ?? userId,
            ArticuloId = c.ArticuloId ?? 0,
            ArticuloNombre = c.Articulo?.NombreArticulo ?? string.Empty,
            ArticuloCodigo = string.Empty,
            Cantidad = c.Cantidad ?? 0,
            PrecioUnitario = c.PrecioUnitario ?? 0,
            Total = (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0),
            FechaConsumo = c.Movimientos?.FechaRegistro ?? DateTime.Now,
            HabitacionId = c.Movimientos?.HabitacionId,
            HabitacionNumero = c.Movimientos?.Habitacion?.NombreHabitacion,
            ReservaId = null,
            TipoConsumo = c.EsHabitacion == true ? "Habitacion" : "Servicio",
            Observaciones = c.Movimientos?.Descripcion,
            Anulado = c.Anulado ?? false,
        };
    }

    private static UserConsumptionSummaryDto BuildUserConsumptionSummary(
        List<Consumo> consumos,
        string userId,
        DateTime periodStart,
        DateTime periodEnd
    )
    {
        var summary = new UserConsumptionSummaryDto
        {
            UserId = userId,
            UserName = userId,
            UserFullName = userId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalItems = consumos.Count(),
            TotalQuantity = consumos.Sum(c => c.Cantidad ?? 0),
            TotalAmount = consumos.Sum(c => (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0)),
        };

        // Group by type
        summary.AmountByType = consumos
            .GroupBy(c => c.EsHabitacion == true ? "Habitacion" : "Servicio")
            .ToDictionary(g => g.Key, g => g.Sum(c => (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0)));

        // Top consumed items
        summary.TopConsumedItems = consumos
            .Where(c => c.ArticuloId.HasValue)
            .GroupBy(c => new { c.ArticuloId, NombreArticulo = c.Articulo?.NombreArticulo })
            .Select(g => new TopConsumedItem
            {
                ArticuloId = g.Key.ArticuloId ?? 0,
                ArticuloNombre = g.Key.NombreArticulo ?? string.Empty,
                TotalQuantity = g.Sum(c => c.Cantidad ?? 0),
                TotalAmount = g.Sum(c => (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0)),
            })
            .OrderByDescending(i => i.TotalAmount)
            .Take(10)
            .ToList();

        return summary;
    }

    private static List<UserConsumptionByServiceDto> BuildConsumptionByServiceSummary(
        List<Consumo> consumos
    )
    {
        var totalAmount = consumos.Sum(c => (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0));

        return consumos
            .GroupBy(c => c.EsHabitacion == true ? "Habitacion" : "Servicio")
            .Select(g =>
            {
                var serviceAmount = g.Sum(c => (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0));
                return new UserConsumptionByServiceDto
                {
                    ServiceType = g.Key,
                    TotalItems = g.Count(),
                    TotalAmount = serviceAmount,
                    Percentage = totalAmount > 0 ? (serviceAmount / totalAmount) * 100 : 0,
                    Items = g.Where(c => c.ArticuloId.HasValue)
                        .GroupBy(c => new
                        {
                            c.ArticuloId,
                            NombreArticulo = c.Articulo?.NombreArticulo,
                        })
                        .Select(itemGroup => new ServiceItemDetail
                        {
                            ArticuloId = itemGroup.Key.ArticuloId ?? 0,
                            ArticuloNombre = itemGroup.Key.NombreArticulo ?? string.Empty,
                            Quantity = itemGroup.Sum(c => c.Cantidad ?? 0),
                            Amount = itemGroup.Sum(c =>
                                (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0)
                            ),
                            LastConsumedDate = itemGroup.Max(c =>
                                c.Movimientos?.FechaRegistro ?? DateTime.Now
                            ),
                        })
                        .OrderByDescending(i => i.Amount)
                        .Take(5)
                        .ToList(),
                };
            })
            .OrderByDescending(s => s.TotalAmount)
            .ToList();
    }

    #endregion

    #region Private Helper Methods - Business Logic

    private async Task<decimal?> GetArticlePriceAsync(
        int articuloId,
        decimal? providedPrice,
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        if (providedPrice.HasValue && providedPrice.Value > 0)
            return providedPrice.Value;

        var articulo = await _context
            .Articulos.AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.ArticuloId == articuloId && a.InstitucionID == institucionId,
                cancellationToken
            );

        return articulo?.Precio;
    }

    #region donde Claude me dijo que reescribiera
    private async Task<ApiResponse> ValidateAndConsumeInventoryAsync(
    int articuloId,
    int cantidad,
    string tipoConsumo,
    int? habitacionId,
    int institucionId,
    string userId,
    string motivo,
    CancellationToken cancellationToken
)
    {
        try
        {
            // Determine inventory location
            var locationType =
                tipoConsumo == "Habitacion"
                    ? InventoryLocationType.Room
                    : InventoryLocationType.General;

            var locationId = tipoConsumo == "Habitacion" ? habitacionId : null;

            _logger.LogInformation(
                "🔍 Validating inventory - Article: {ArticuloId}, Quantity: {Cantidad}, " +
                "Type: {TipoConsumo}, LocationType: {LocationType}, LocationId: {LocationId}",
                articuloId,
                cantidad,
                tipoConsumo,
                locationType,
                locationId
            );

            // Verify article exists
            var articulo = await _context.Articulos
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    a => a.ArticuloId == articuloId && a.InstitucionID == institucionId,
                    cancellationToken
                );

            if (articulo == null)
            {
                return ApiResponse.Failure($"Article {articuloId} not found in institution {institucionId}");
            }

            // 1️⃣ FIND OR CREATE IN InventarioGeneral (Legacy Table)
            var inventarioGeneral = await _context.InventarioGeneral
                .FirstOrDefaultAsync(
                    i => i.ArticuloId == articuloId && i.InstitucionID == institucionId,
                    cancellationToken
                );

            if (inventarioGeneral == null)
            {
                _logger.LogWarning(
                    "📦 InventarioGeneral not found for article {ArticuloId}. Creating new record.",
                    articuloId
                );

                inventarioGeneral = new InventarioGeneral
                {
                    ArticuloId = articuloId,
                    Cantidad = 0, // Start with 0
                    FechaRegistro = DateTime.Now,
                    Anulado = false,
                    InstitucionID = institucionId
                };

                _context.InventarioGeneral.Add(inventarioGeneral);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "✅ Created InventarioGeneral ID {InventarioId} for article {ArticuloId}",
                    inventarioGeneral.InventarioId,
                    articuloId
                );
            }

            // 2️⃣ FIND OR CREATE IN InventarioUnificado (New Unified Table)
            var inventarioUnificado = await _context.InventarioUnificado
                .FirstOrDefaultAsync(
                    i =>
                        i.ArticuloId == articuloId
                        && i.TipoUbicacion == (int)locationType
                        && i.UbicacionId == locationId
                        && i.InstitucionID == institucionId,
                    cancellationToken
                );

            if (inventarioUnificado == null)
            {
                _logger.LogWarning(
                    "📦 InventarioUnificado not found for article {ArticuloId} in location {LocationType}:{LocationId}. Creating new record.",
                    articuloId,
                    locationType,
                    locationId
                );

                inventarioUnificado = new InventarioUnificado
                {
                    ArticuloId = articuloId,
                    InstitucionID = institucionId,
                    TipoUbicacion = (int)locationType,
                    UbicacionId = locationId,
                    Cantidad = 0, // Start with 0
                    FechaRegistro = DateTime.UtcNow,
                    FechaUltimaActualizacion = DateTime.UtcNow,
                    UsuarioRegistro = userId,
                    UsuarioUltimaActualizacion = userId,
                    Anulado = false,
                    CantidadMinima = 0,
                    CantidadMaxima = null,
                    PuntoReorden = 0,
                    Notas = locationType == InventoryLocationType.Room
                        ? $"Auto-created for room {locationId} inventory consumption"
                        : "Auto-created for general inventory consumption"
                };

                _context.InventarioUnificado.Add(inventarioUnificado);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "✅ Created InventarioUnificado ID {InventarioId} for article {ArticuloId} in location {LocationType}:{LocationId}",
                    inventarioUnificado.InventarioId,
                    articuloId,
                    locationType,
                    locationId
                );
            }

            // 3️⃣ VALIDATE STOCK AVAILABILITY
            if (inventarioUnificado.Cantidad < cantidad)
            {
                _logger.LogWarning(
                    "⚠️ Insufficient stock for article {ArticuloId}. Available: {Available}, Requested: {Requested}",
                    articuloId,
                    inventarioUnificado.Cantidad,
                    cantidad
                );

                return ApiResponse.Failure(
                    $"Stock insuficiente. Disponible: {inventarioUnificado.Cantidad}, " +
                    $"Solicitado: {cantidad}. " +
                    $"Por favor, reabastezca el inventario para el artículo '{articulo.NombreArticulo}' " +
                    $"en la ubicación {(locationType == InventoryLocationType.Room ? $"Habitación {locationId}" : "General")}."
                );
            }

            // 4️⃣ UPDATE INVENTORY QUANTITY DIRECTLY (within existing transaction)
            _logger.LogInformation(
                "📝 Updating inventory - Reducing {Cantidad} units from article {ArticuloId}",
                cantidad,
                articuloId
            );

            var previousQuantity = inventarioUnificado.Cantidad;
            inventarioUnificado.Cantidad -= cantidad;
            inventarioUnificado.FechaUltimaActualizacion = DateTime.UtcNow;
            inventarioUnificado.UsuarioUltimaActualizacion = userId;

            // Update InventarioGeneral as well (keep legacy table in sync)
            if (locationType == InventoryLocationType.General && inventarioGeneral != null)
            {
                inventarioGeneral.Cantidad = inventarioUnificado.Cantidad;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // 5️⃣ REGISTER MOVEMENT (using CreateMovementAsync - no nested transaction)
            _logger.LogInformation(
                "📝 Registering movement for inventory {InventarioId}",
                inventarioUnificado.InventarioId
            );

            var movementResult = await _inventoryService.RegisterMovementAsync(
                new MovimientoInventarioCreateDto
                {
                    InventarioId = inventarioUnificado.InventarioId,
                    TipoMovimiento = "Salida",
                    CantidadAnterior = previousQuantity,
                    CantidadNueva = inventarioUnificado.Cantidad,
                    CantidadCambiada = -cantidad,
                    Motivo = $"{motivo} - {tipoConsumo}",
                    NumeroDocumento = $"CONSUMO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                },
                institucionId,
                userId,
                cancellationToken: cancellationToken
            );

            if (!movementResult.IsSuccess)
            {
                _logger.LogError(
                    "❌ Failed to register inventory movement for article {ArticuloId}",
                    articuloId
                );
                return ApiResponse.Failure(
                    "Error updating inventory",
                    movementResult.Message ?? "Failed to register inventory movement"
                );
            }

            _logger.LogInformation(
                "✅ Inventory successfully updated for article {ArticuloId}. " +
                "Previous: {PreviousQuantity}, New: {NewQuantity}, Changed: {Changed}",
                articuloId,
                previousQuantity,
                inventarioUnificado.Cantidad,
                -cantidad
            );

            return ApiResponse.Success("Inventory updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ ERROR in ValidateAndConsumeInventoryAsync - Article: {ArticuloId}, " +
                "Quantity: {Cantidad}, Type: {TipoConsumo}, Message: {Message}, " +
                "StackTrace: {StackTrace}",
                articuloId,
                cantidad,
                tipoConsumo,
                ex.Message,
                ex.StackTrace
            );

            return ApiResponse.Failure(
                "Error processing inventory",
                $"An unexpected error occurred: {ex.Message}"
            );
        }
    }
#endregion
// consume de HotelDbContext.DbSets.cs
    private async Task<ApiResponse> RestoreInventoryForCancelledConsumptionAsync(
        int articuloId,
        int cantidad,
        bool esHabitacion,
        int? habitacionId,
        string motivo,
        string numeroDocumento,
        int institucionId,
        string userId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var locationType = esHabitacion
                ? InventoryLocationType.Room
                : InventoryLocationType.General;
            var locationId = esHabitacion ? habitacionId : null;

            var inventory = await _context.InventarioUnificado.FirstOrDefaultAsync(
                i =>
                    i.ArticuloId == articuloId
                    && i.TipoUbicacion == (int)locationType
                    && i.UbicacionId == locationId
                    && i.InstitucionID == institucionId,
                cancellationToken
            );

            if (inventory == null)
            {
                return ApiResponse.Failure(
                    "Inventory not found for article in specified location during restoration"
                );
            }

            var movementResult = await _inventoryService.RegisterMovementAsync(
                new MovimientoInventarioCreateDto
                {
                    InventarioId = inventory.InventarioId,
                    TipoMovimiento = "Entrada",
                    CantidadAnterior = inventory.Cantidad,
                    CantidadNueva = inventory.Cantidad + cantidad,
                    CantidadCambiada = cantidad,
                    Motivo = motivo,
                    NumeroDocumento = $"CANCEL-{numeroDocumento}",
                },
                institucionId,
                userId,
                cancellationToken: cancellationToken
            );

            return movementResult.IsSuccess
                ? ApiResponse.Success("Inventory restored successfully")
                : ApiResponse.Failure(
                    "Error restoring inventory",
                    movementResult.Message ?? "Failed to register inventory restoration movement"
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error restoring inventory for cancelled consumption of article {ArticuloId}",
                articuloId
            );
            return ApiResponse.Failure(
                "Error restoring inventory",
                "An unexpected error occurred while restoring inventory"
            );
        }
    }

    #endregion

    #region Private Helper Methods - Entity Creation

    private static Movimientos CreateMovimiento(
        string userId,
        int institucionId,
        UserConsumptionCreateDto createDto,
        decimal precioUnitario
    )
    {
        return new Movimientos
        {
            InstitucionID = institucionId,
            UserId = userId,
            FechaRegistro = DateTime.Now,
            TotalFacturado = createDto.Cantidad * precioUnitario,
            HabitacionId = createDto.HabitacionId,
            Descripcion = createDto.Observaciones,
        };
    }

    private static Movimientos CreateMovimientoForAdmin(
        AdminUserConsumptionCreateDto createDto,
        string adminUserId,
        int institucionId,
        decimal precioUnitario
    )
    {
        return new Movimientos
        {
            InstitucionID = institucionId,
            UserId = createDto.UserId,
            FechaRegistro = DateTime.Now,
            TotalFacturado = createDto.Cantidad * precioUnitario,
            HabitacionId = createDto.HabitacionId,
            Descripcion =
                createDto.Observaciones ?? $"Consumo asignado por administrador {adminUserId}",
        };
    }

    private static Consumo CreateConsumo(
        int movimientosId,
        UserConsumptionCreateDto createDto,
        decimal precioUnitario
    )
    {
        return new Consumo
        {
            MovimientosId = movimientosId,
            ArticuloId = createDto.ArticuloId,
            Cantidad = createDto.Cantidad,
            PrecioUnitario = precioUnitario,
            EsHabitacion = createDto.TipoConsumo == "Habitacion",
            Anulado = false,
        };
    }

    private static Consumo CreateConsumoFromAdmin(
        int movimientosId,
        AdminUserConsumptionCreateDto createDto,
        decimal precioUnitario
    )
    {
        return new Consumo
        {
            MovimientosId = movimientosId,
            ArticuloId = createDto.ArticuloId,
            Cantidad = createDto.Cantidad,
            PrecioUnitario = precioUnitario,
            EsHabitacion = createDto.TipoConsumo == "Habitacion",
            Anulado = false,
        };
    }

    #endregion

    #region Private Helper Methods - Data Retrieval

    private async Task<UserConsumptionDto> GetCreatedConsumptionDto(
        int consumoId,
        string userId,
        CancellationToken cancellationToken
    )
    {
        var result = await _context
            .Consumo.AsNoTracking()
            .Include(c => c.Articulo)
            .Include(c => c.Movimientos!)
            .ThenInclude(m => m.Habitacion)
            .FirstOrDefaultAsync(c => c.ConsumoId == consumoId, cancellationToken);

        return MapToUserConsumptionDto(result!, userId);
    }

    private async Task<UserConsumptionDto> GetCreatedConsumptionDtoWithUserInfo(
        int consumoId,
        AdminUserConsumptionCreateDto createDto,
        ApplicationUser targetUser,
        CancellationToken cancellationToken
    )
    {
        var result = await _context
            .Consumo.AsNoTracking()
            .Include(c => c.Articulo)
            .Include(c => c.Movimientos!)
            .ThenInclude(m => m.Habitacion)
            .FirstOrDefaultAsync(c => c.ConsumoId == consumoId, cancellationToken);

        var dto = MapToUserConsumptionDto(result!, createDto.UserId);
        dto.UserName = targetUser.UserName ?? createDto.UserId;
        dto.UserFullName = $"{targetUser.FirstName} {targetUser.LastName}".Trim();
        dto.ReservaId = createDto.ReservaId;

        return dto;
    }

    private async Task<Consumo?> FindConsumptionForCancellation(
        int consumptionId,
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .Consumo.Include(c => c.Articulo)
            .Include(c => c.Movimientos)
            .FirstOrDefaultAsync(
                c =>
                    c.ConsumoId == consumptionId
                    && c.Movimientos != null
                    && c.Movimientos.InstitucionID == institucionId,
                cancellationToken
            );
    }

    #endregion

    #region Private Helper Methods - Error Handling

    private ApiResponse<T> LogAndReturnError<T>(
        Exception ex,
        string messageTemplate,
        params object[] args
    )
    {
        _logger.LogError(ex, messageTemplate, args);
        return ApiResponse<T>.Failure(
            "Error processing request",
            "An error occurred while processing the request"
        );
    }

    #endregion
}
