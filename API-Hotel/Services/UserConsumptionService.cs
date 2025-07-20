using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.UserConsumption;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

public class UserConsumptionService : IUserConsumptionService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<UserConsumptionService> _logger;

    public UserConsumptionService(HotelDbContext context, ILogger<UserConsumptionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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
            var query = _context
                .Consumo.AsNoTracking()
                .Include(c => c.Articulo)
                .Include(c => c.Movimientos)
                .ThenInclude(m => m.Habitacion)
                .Where(c =>
                    c.Movimientos != null
                    && c.Movimientos.UsuarioId != null
                    && c.Movimientos.UsuarioId.ToString() == userId
                    && c.Movimientos.InstitucionID == institucionId
                    && (c.Anulado == null || c.Anulado == false)
                );

            if (startDate.HasValue)
                query = query.Where(c => c.Movimientos.FechaRegistro >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.Movimientos.FechaRegistro <= endDate.Value);

            var consumos = await query
                .Select(c => new UserConsumptionDto
                {
                    Id = c.ConsumoId,
                    UserId = userId,
                    UserName = userId,
                    UserFullName = userId,
                    ArticuloId = c.ArticuloId ?? 0,
                    ArticuloNombre =
                        c.Articulo != null
                            ? c.Articulo.NombreArticulo ?? string.Empty
                            : string.Empty,
                    ArticuloCodigo = string.Empty,
                    Cantidad = c.Cantidad ?? 0,
                    PrecioUnitario = c.PrecioUnitario ?? 0,
                    Total = (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0),
                    FechaConsumo = c.Movimientos.FechaRegistro ?? DateTime.Now,
                    HabitacionId = c.Movimientos.HabitacionId,
                    HabitacionNumero =
                        c.Movimientos.Habitacion != null
                            ? c.Movimientos.Habitacion.NombreHabitacion
                            : null,
                    ReservaId = null,
                    TipoConsumo = c.EsHabitacion == true ? "Habitacion" : "Servicio",
                    Observaciones = c.Movimientos.Descripcion,
                    Anulado = c.Anulado ?? false,
                })
                .OrderByDescending(c => c.FechaConsumo)
                .ToListAsync(cancellationToken);

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
            _logger.LogError(
                ex,
                "Error retrieving user consumption for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );
            return ApiResponse<IEnumerable<UserConsumptionDto>>.Failure(
                "Error retrieving user consumption",
                "An error occurred while retrieving consumption records"
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

            var query = _context
                .Consumo.AsNoTracking()
                .Include(c => c.Articulo)
                .Include(c => c.Movimientos)
                .Where(c =>
                    c.Movimientos != null
                    && c.Movimientos.UsuarioId != null
                    && c.Movimientos.UsuarioId.ToString() == userId
                    && c.Movimientos.InstitucionID == institucionId
                    && (c.Anulado == null || c.Anulado == false)
                    && c.Movimientos.FechaRegistro >= periodStart
                    && c.Movimientos.FechaRegistro <= periodEnd
                );

            var consumos = await query.ToListAsync(cancellationToken);

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
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(c => (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0))
                );

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

            _logger.LogInformation(
                "Generated consumption summary for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );

            return ApiResponse<UserConsumptionSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error generating consumption summary for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );
            return ApiResponse<UserConsumptionSummaryDto>.Failure(
                "Error generating consumption summary",
                "An error occurred while generating the consumption summary"
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
            var query = _context
                .Consumo.AsNoTracking()
                .Include(c => c.Articulo)
                .Include(c => c.Movimientos)
                .ThenInclude(m => m.Habitacion)
                .Where(c =>
                    c.Movimientos != null
                    && c.Movimientos.InstitucionID == institucionId
                    && (c.Anulado == null || c.Anulado == false)
                );

            if (startDate.HasValue)
                query = query.Where(c => c.Movimientos.FechaRegistro >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.Movimientos.FechaRegistro <= endDate.Value);

            var consumosData = await query.ToListAsync(cancellationToken);

            var consumos = consumosData
                .Select(c => new UserConsumptionDto
                {
                    Id = c.ConsumoId,
                    UserId = c.Movimientos?.UsuarioId?.ToString() ?? string.Empty,
                    UserName = c.Movimientos?.UsuarioId?.ToString() ?? string.Empty,
                    UserFullName = c.Movimientos?.UsuarioId?.ToString() ?? string.Empty,
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
                })
                .OrderByDescending(c => c.FechaConsumo)
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
            _logger.LogError(
                ex,
                "Error retrieving all users consumption for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<UserConsumptionDto>>.Failure(
                "Error retrieving consumption records",
                "An error occurred while retrieving consumption records"
            );
        }
    }

    public async Task<ApiResponse<UserConsumptionDto>> RegisterConsumptionAsync(
        UserConsumptionCreateDto createDto,
        string userId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get article price if not provided
            decimal precioUnitario = createDto.PrecioUnitario ?? 0;
            if (precioUnitario == 0)
            {
                var articulo = await _context
                    .Articulos.AsNoTracking()
                    .FirstOrDefaultAsync(
                        a =>
                            a.ArticuloId == createDto.ArticuloId
                            && a.InstitucionID == institucionId,
                        cancellationToken
                    );

                if (articulo == null)
                {
                    return ApiResponse<UserConsumptionDto>.Failure("Article not found");
                }

                precioUnitario = articulo.Precio;
            }

            // Create movement
            var movimiento = new Movimientos
            {
                InstitucionID = institucionId,
                UsuarioId = int.TryParse(userId, out var uid) ? uid : null,
                FechaRegistro = DateTime.Now,
                TotalFacturado = createDto.Cantidad * precioUnitario,
                HabitacionId = createDto.HabitacionId,
                Descripcion = createDto.Observaciones,
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            // Create consumption
            var consumo = new Consumo
            {
                MovimientosId = movimiento.MovimientosId,
                ArticuloId = createDto.ArticuloId,
                Cantidad = createDto.Cantidad,
                PrecioUnitario = precioUnitario,
                EsHabitacion = createDto.TipoConsumo == "Habitacion",
                Anulado = false,
            };

            _context.Consumo.Add(consumo);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Retrieve the created consumption with related data
            var result = await _context
                .Consumo.AsNoTracking()
                .Include(c => c.Articulo)
                .Include(c => c.Movimientos)
                .ThenInclude(m => m.Habitacion)
                .FirstOrDefaultAsync(c => c.ConsumoId == consumo.ConsumoId, cancellationToken);

            var dto = new UserConsumptionDto
            {
                Id = result.ConsumoId,
                UserId = userId,
                UserName = userId,
                UserFullName = userId,
                ArticuloId = result.ArticuloId ?? 0,
                ArticuloNombre = result.Articulo?.NombreArticulo ?? string.Empty,
                ArticuloCodigo = string.Empty,
                Cantidad = result.Cantidad ?? 0,
                PrecioUnitario = result.PrecioUnitario ?? 0,
                Total = (result.Cantidad ?? 0) * (result.PrecioUnitario ?? 0),
                FechaConsumo = result.Movimientos?.FechaRegistro ?? DateTime.Now,
                HabitacionId = result.Movimientos?.HabitacionId,
                HabitacionNumero = result.Movimientos?.Habitacion?.NombreHabitacion,
                ReservaId = null,
                TipoConsumo = result.EsHabitacion == true ? "Habitacion" : "Servicio",
                Observaciones = result.Movimientos?.Descripcion,
                Anulado = result.Anulado ?? false,
            };

            _logger.LogInformation(
                "Registered consumption {ConsumoId} for user {UserId} in institution {InstitucionId}",
                consumo.ConsumoId,
                userId,
                institucionId
            );

            return ApiResponse<UserConsumptionDto>.Success(
                dto,
                "Consumption registered successfully"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error registering consumption for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );
            return ApiResponse<UserConsumptionDto>.Failure(
                "Error registering consumption",
                "An error occurred while registering the consumption"
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
            // Validate that the target user exists and belongs to the institution
            var targetUser = await _context
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == createDto.UserId, cancellationToken);

            if (targetUser == null)
            {
                return ApiResponse<UserConsumptionDto>.Failure("Target user not found");
            }

            // Get article price if not provided
            decimal precioUnitario = createDto.PrecioUnitario ?? 0;
            if (precioUnitario == 0)
            {
                var articulo = await _context
                    .Articulos.AsNoTracking()
                    .FirstOrDefaultAsync(
                        a =>
                            a.ArticuloId == createDto.ArticuloId
                            && a.InstitucionID == institucionId,
                        cancellationToken
                    );

                if (articulo == null)
                {
                    return ApiResponse<UserConsumptionDto>.Failure("Article not found");
                }

                precioUnitario = articulo.Precio;
            }

            // Create movement for the target user
            var movimiento = new Movimientos
            {
                InstitucionID = institucionId,
                UsuarioId = int.TryParse(createDto.UserId, out var uid) ? uid : null,
                FechaRegistro = DateTime.Now,
                TotalFacturado = createDto.Cantidad * precioUnitario,
                HabitacionId = createDto.HabitacionId,
                Descripcion =
                    createDto.Observaciones ?? $"Consumo asignado por administrador {adminUserId}",
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            // Create consumption
            var consumo = new Consumo
            {
                MovimientosId = movimiento.MovimientosId,
                ArticuloId = createDto.ArticuloId,
                Cantidad = createDto.Cantidad,
                PrecioUnitario = precioUnitario,
                EsHabitacion = createDto.TipoConsumo == "Habitacion",
                Anulado = false,
            };

            _context.Consumo.Add(consumo);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Retrieve the created consumption with related data
            var result = await _context
                .Consumo.AsNoTracking()
                .Include(c => c.Articulo)
                .Include(c => c.Movimientos)
                .ThenInclude(m => m.Habitacion)
                .FirstOrDefaultAsync(c => c.ConsumoId == consumo.ConsumoId, cancellationToken);

            var dto = new UserConsumptionDto
            {
                Id = result.ConsumoId,
                UserId = createDto.UserId,
                UserName = targetUser.UserName ?? createDto.UserId,
                UserFullName = $"{targetUser.FirstName} {targetUser.LastName}".Trim(),
                ArticuloId = result.ArticuloId ?? 0,
                ArticuloNombre = result.Articulo?.NombreArticulo ?? string.Empty,
                ArticuloCodigo = string.Empty,
                Cantidad = result.Cantidad ?? 0,
                PrecioUnitario = result.PrecioUnitario ?? 0,
                Total = (result.Cantidad ?? 0) * (result.PrecioUnitario ?? 0),
                FechaConsumo = result.Movimientos?.FechaRegistro ?? DateTime.Now,
                HabitacionId = result.Movimientos?.HabitacionId,
                HabitacionNumero = result.Movimientos?.Habitacion?.NombreHabitacion,
                ReservaId = createDto.ReservaId,
                TipoConsumo = result.EsHabitacion == true ? "Habitacion" : "Servicio",
                Observaciones = result.Movimientos?.Descripcion,
                Anulado = result.Anulado ?? false,
            };

            _logger.LogInformation(
                "Admin {AdminUserId} created consumption {ConsumoId} for user {UserId} in institution {InstitucionId}",
                adminUserId,
                consumo.ConsumoId,
                createDto.UserId,
                institucionId
            );

            return ApiResponse<UserConsumptionDto>.Success(
                dto,
                "Consumption created successfully for user"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error creating consumption for user {UserId} by admin {AdminUserId} in institution {InstitucionId}",
                createDto.UserId,
                adminUserId,
                institucionId
            );
            return ApiResponse<UserConsumptionDto>.Failure(
                "Error creating consumption for user",
                "An error occurred while creating the consumption for the user"
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
            var query = _context
                .Consumo.AsNoTracking()
                .Include(c => c.Articulo)
                .Include(c => c.Movimientos)
                .Where(c =>
                    c.Movimientos != null
                    && c.Movimientos.UsuarioId != null
                    && c.Movimientos.UsuarioId.ToString() == userId
                    && c.Movimientos.InstitucionID == institucionId
                    && (c.Anulado == null || c.Anulado == false)
                );

            if (startDate.HasValue)
                query = query.Where(c => c.Movimientos.FechaRegistro >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.Movimientos.FechaRegistro <= endDate.Value);

            var consumos = await query.ToListAsync(cancellationToken);

            var totalAmount = consumos.Sum(c => (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0));

            var result = consumos
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

            _logger.LogInformation(
                "Retrieved consumption by service for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );

            return ApiResponse<IEnumerable<UserConsumptionByServiceDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving consumption by service for user {UserId} in institution {InstitucionId}",
                userId,
                institucionId
            );
            return ApiResponse<IEnumerable<UserConsumptionByServiceDto>>.Failure(
                "Error retrieving consumption by service",
                "An error occurred while retrieving consumption by service"
            );
        }
    }
}

