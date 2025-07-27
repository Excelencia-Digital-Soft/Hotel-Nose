using System.Text.Json;
using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service for inventory movement operations and audit trail
/// </summary>
public class InventoryMovementService : IInventoryMovementService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<InventoryMovementService> _logger;

    public InventoryMovementService(
        HotelDbContext context,
        ILogger<InventoryMovementService> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<MovimientoInventarioDto>> CreateMovementAsync(
        MovimientoInventarioCreateDto movementDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Validate inventory exists and belongs to institution
            var inventario = await _context
                .InventarioUnificado.AsNoTracking()
                .FirstOrDefaultAsync(
                    i =>
                        i.InventarioId == movementDto.InventarioId
                        && i.InstitucionID == institucionId,
                    cancellationToken
                );

            if (inventario == null)
            {
                return ApiResponse<MovimientoInventarioDto>.Failure("Inventory not found");
            }

            // Create movement record
            var movimiento = new MovimientoInventario
            {
                InventarioId = movementDto.InventarioId,
                InstitucionID = institucionId,
                TipoMovimiento = movementDto.TipoMovimiento,
                CantidadAnterior = movementDto.CantidadAnterior,
                CantidadNueva = movementDto.CantidadNueva,
                CantidadCambiada = movementDto.CantidadCambiada,
                Motivo = movementDto.Motivo,
                NumeroDocumento = movementDto.NumeroDocumento,
                TransferenciaId = movementDto.TransferenciaId,
                FechaMovimiento = DateTime.UtcNow,
                UsuarioId = userId,
                DireccionIP = ipAddress,
                Metadata =
                    movementDto.Metadata != null
                        ? JsonSerializer.Serialize(movementDto.Metadata)
                        : null,
            };

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            // Map to DTO
            var movimientoDto = MapToMovimientoDto(movimiento);

            _logger.LogInformation(
                "Movement {TipoMovimiento} created for inventory {InventarioId} by user {UserId}",
                movementDto.TipoMovimiento,
                movementDto.InventarioId,
                userId
            );

            return ApiResponse<MovimientoInventarioDto>.Success(
                movimientoDto,
                "Movement recorded successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating movement for inventory {InventarioId} in institution {InstitucionId}",
                movementDto.InventarioId,
                institucionId
            );
            return ApiResponse<MovimientoInventarioDto>.Failure(
                "Error creating movement",
                "An error occurred while recording the inventory movement"
            );
        }
    }

    public async Task<ApiResponse<PagedResult<MovimientoInventarioDto>>> GetMovementsAsync(
        int inventoryId,
        int institucionId,
        MovimientoInventarioFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .MovimientosInventario.AsNoTracking()
                .Include(m => m.Inventario)
                .ThenInclude(i => i!.Articulo)
                .Include(m => m.Usuario)
                .Where(m => m.InventarioId == inventoryId && m.InstitucionID == institucionId);

            // Apply filters
            if (!string.IsNullOrEmpty(filter.TipoMovimiento))
            {
                query = query.Where(m => m.TipoMovimiento == filter.TipoMovimiento);
            }

            if (filter.FechaDesde.HasValue)
            {
                query = query.Where(m => m.FechaMovimiento >= filter.FechaDesde.Value);
            }

            if (filter.FechaHasta.HasValue)
            {
                query = query.Where(m => m.FechaMovimiento <= filter.FechaHasta.Value);
            }

            if (!string.IsNullOrEmpty(filter.UsuarioId))
            {
                query = query.Where(m => m.UsuarioId == filter.UsuarioId);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = filter.OrdenarPor?.ToLower() switch
            {
                "fecha" => filter.Descendente
                    ? query.OrderByDescending(m => m.FechaMovimiento)
                    : query.OrderBy(m => m.FechaMovimiento),
                "tipo" => filter.Descendente
                    ? query.OrderByDescending(m => m.TipoMovimiento)
                    : query.OrderBy(m => m.TipoMovimiento),
                "cantidad" => filter.Descendente
                    ? query.OrderByDescending(m => m.CantidadCambiada)
                    : query.OrderBy(m => m.CantidadCambiada),
                _ => query.OrderByDescending(m => m.FechaMovimiento),
            };

            // Apply pagination
            var movimientos = await query
                .Skip((filter.Pagina - 1) * filter.TamanoPagina)
                .Take(filter.TamanoPagina)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var movimientoDtos = new List<MovimientoInventarioDto>();
            foreach (var movimiento in movimientos)
            {
                movimientoDtos.Add(MapToMovimientoDto(movimiento));
            }

            var result = new PagedResult<MovimientoInventarioDto>
            {
                Items = movimientoDtos,
                TotalCount = totalCount,
                Page = filter.Pagina,
                PageSize = filter.TamanoPagina,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.TamanoPagina),
            };

            return ApiResponse<PagedResult<MovimientoInventarioDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving movements for inventory {InventoryId} in institution {InstitucionId}",
                inventoryId,
                institucionId
            );
            return ApiResponse<PagedResult<MovimientoInventarioDto>>.Failure(
                "Error retrieving movements",
                "An error occurred while retrieving the inventory movements"
            );
        }
    }

    public async Task<ApiResponse<MovimientoInventarioDto>> GetMovementByIdAsync(
        int movementId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var movimiento = await _context
                .MovimientosInventario.AsNoTracking()
                .Include(m => m.Inventario)
                .ThenInclude(i => i!.Articulo)
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(
                    m => m.MovimientoId == movementId && m.InstitucionID == institucionId,
                    cancellationToken
                );

            if (movimiento == null)
            {
                return ApiResponse<MovimientoInventarioDto>.Failure("Movement not found");
            }

            var movimientoDto = MapToMovimientoDto(movimiento);
            return ApiResponse<MovimientoInventarioDto>.Success(movimientoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving movement {MovementId} in institution {InstitucionId}",
                movementId,
                institucionId
            );
            return ApiResponse<MovimientoInventarioDto>.Failure(
                "Error retrieving movement",
                "An error occurred while retrieving the movement details"
            );
        }
    }

    public async Task<ApiResponse<MovimientoEstadisticasDto>> GetMovementStatisticsAsync(
        int institucionId,
        MovimientoEstadisticasFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .MovimientosInventario.AsNoTracking()
                .Where(m => m.InstitucionID == institucionId);

            // Apply date filters
            if (filter.FechaDesde.HasValue)
            {
                query = query.Where(m => m.FechaMovimiento >= filter.FechaDesde.Value);
            }

            if (filter.FechaHasta.HasValue)
            {
                query = query.Where(m => m.FechaMovimiento <= filter.FechaHasta.Value);
            }

            // Get statistics
            var totalMovimientos = await query.CountAsync(cancellationToken);

            var movimientosPorTipo = await query
                .GroupBy(m => m.TipoMovimiento)
                .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                .ToListAsync(cancellationToken);

            var movimientosPorDia = await query
                .Where(m => m.FechaMovimiento >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(m => m.FechaMovimiento.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
                .OrderBy(x => x.Fecha)
                .ToListAsync(cancellationToken);

            var usuariosMasActivos = await query
                .Where(m => m.FechaMovimiento >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(m => m.UsuarioId)
                .Select(g => new { UsuarioId = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .Take(10)
                .ToListAsync(cancellationToken);

            var estadisticas = new MovimientoEstadisticasDto
            {
                TotalMovimientos = totalMovimientos,
                MovimientosPorTipo = movimientosPorTipo.ToDictionary(x => x.Tipo, x => x.Cantidad),
                MovimientosPorDia = movimientosPorDia.ToDictionary(
                    x => x.Fecha.ToString("yyyy-MM-dd"),
                    x => x.Cantidad
                ),
                UsuariosMasActivos = usuariosMasActivos.ToDictionary(
                    x => x.UsuarioId,
                    x => x.Cantidad
                ),
                PeriodoAnalisis = new PeriodoAnalisisDto
                {
                    FechaInicio = filter.FechaDesde ?? DateTime.UtcNow.AddDays(-30),
                    FechaFin = filter.FechaHasta ?? DateTime.UtcNow,
                },
            };

            return ApiResponse<MovimientoEstadisticasDto>.Success(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving movement statistics for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<MovimientoEstadisticasDto>.Failure(
                "Error retrieving statistics",
                "An error occurred while retrieving movement statistics"
            );
        }
    }

    public async Task<ApiResponse<MovimientoInventarioDto>> RecordAdjustmentAsync(
        int inventoryId,
        int newQuantity,
        string reason,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get current inventory
            var inventario = await _context.InventarioUnificado.FirstOrDefaultAsync(
                i => i.InventarioId == inventoryId && i.InstitucionID == institucionId,
                cancellationToken
            );

            if (inventario == null)
            {
                return ApiResponse<MovimientoInventarioDto>.Failure("Inventory not found");
            }

            var cantidadAnterior = inventario.Cantidad;
            var cantidadCambiada = newQuantity - cantidadAnterior;

            // Update inventory quantity
            inventario.Cantidad = newQuantity;
            inventario.FechaUltimaActualizacion = DateTime.UtcNow;

            // Create movement record
            var movimiento = new MovimientoInventario
            {
                InventarioId = inventoryId,
                InstitucionID = institucionId,
                TipoMovimiento = "Ajuste",
                CantidadAnterior = cantidadAnterior,
                CantidadNueva = newQuantity,
                CantidadCambiada = cantidadCambiada,
                Motivo = reason,
                FechaMovimiento = DateTime.UtcNow,
                UsuarioId = userId,
                DireccionIP = ipAddress,
            };

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var movimientoDto = MapToMovimientoDto(movimiento);

            _logger.LogInformation(
                "Inventory adjustment recorded for {InventoryId}: {CantidadAnterior} -> {CantidadNueva}",
                inventoryId,
                cantidadAnterior,
                newQuantity
            );

            return ApiResponse<MovimientoInventarioDto>.Success(
                movimientoDto,
                "Adjustment recorded successfully"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error recording adjustment for inventory {InventoryId}",
                inventoryId
            );
            return ApiResponse<MovimientoInventarioDto>.Failure(
                "Error recording adjustment",
                "An error occurred while recording the inventory adjustment"
            );
        }
    }

    public async Task<ApiResponse<MovimientoInventarioDto>> RecordConsumptionAsync(
        int inventoryId,
        int quantity,
        int? consumoId,
        string? details,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get current inventory
            var inventario = await _context.InventarioUnificado.FirstOrDefaultAsync(
                i => i.InventarioId == inventoryId && i.InstitucionID == institucionId,
                cancellationToken
            );

            if (inventario == null)
            {
                return ApiResponse<MovimientoInventarioDto>.Failure("Inventory not found");
            }

            if (inventario.Cantidad < quantity)
            {
                return ApiResponse<MovimientoInventarioDto>.Failure(
                    "Insufficient inventory quantity"
                );
            }

            var cantidadAnterior = inventario.Cantidad;
            var cantidadNueva = cantidadAnterior - quantity;

            // Update inventory quantity
            inventario.Cantidad = cantidadNueva;
            inventario.FechaUltimaActualizacion = DateTime.UtcNow;

            // Create metadata for consumption details
            var metadata = new Dictionary<string, string>();
            if (consumoId.HasValue)
            {
                metadata["ConsumoId"] = consumoId.Value.ToString();
            }
            if (!string.IsNullOrEmpty(details))
            {
                metadata["Detalles"] = details;
            }

            // Create movement record
            var movimiento = new MovimientoInventario
            {
                InventarioId = inventoryId,
                InstitucionID = institucionId,
                TipoMovimiento = "Consumo",
                CantidadAnterior = cantidadAnterior,
                CantidadNueva = cantidadNueva,
                CantidadCambiada = -quantity,
                Motivo = details ?? "Consumo registrado",
                FechaMovimiento = DateTime.UtcNow,
                UsuarioId = userId,
                DireccionIP = ipAddress,
                Metadata = metadata.Count > 0 ? JsonSerializer.Serialize(metadata) : null,
            };

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var movimientoDto = MapToMovimientoDto(movimiento);

            _logger.LogInformation(
                "Consumption recorded for inventory {InventoryId}: {Quantity} units consumed",
                inventoryId,
                quantity
            );

            return ApiResponse<MovimientoInventarioDto>.Success(
                movimientoDto,
                "Consumption recorded successfully"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error recording consumption for inventory {InventoryId}",
                inventoryId
            );
            return ApiResponse<MovimientoInventarioDto>.Failure(
                "Error recording consumption",
                "An error occurred while recording the consumption"
            );
        }
    }

    #region Private Methods

    private MovimientoInventarioDto MapToMovimientoDto(MovimientoInventario movimiento)
    {
        return new MovimientoInventarioDto
        {
            MovimientoId = movimiento.MovimientoId,
            InventarioId = movimiento.InventarioId,
            TipoMovimiento = movimiento.TipoMovimiento,
            CantidadAnterior = movimiento.CantidadAnterior,
            CantidadNueva = movimiento.CantidadNueva,
            CantidadCambiada = movimiento.CantidadCambiada,
            Motivo = movimiento.Motivo,
            NumeroDocumento = movimiento.NumeroDocumento,
            TransferenciaId = movimiento.TransferenciaId,
            FechaMovimiento = movimiento.FechaMovimiento,
            UsuarioId = movimiento.UsuarioId,
            NombreUsuario = movimiento.Usuario?.UserName ?? "Usuario Desconocido",
            DireccionIP = movimiento.DireccionIP,
            Metadata = !string.IsNullOrEmpty(movimiento.Metadata)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(movimiento.Metadata)
                : null,
            NombreArticulo =
                movimiento.Inventario?.Articulo?.NombreArticulo ?? "Artículo Desconocido",
            CodigoArticulo = movimiento.Inventario?.Articulo?.ArticuloId.ToString() ?? "",
        };
    }

    #endregion
}
