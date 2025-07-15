using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Consumos;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service implementation for managing consumos (consumptions)
/// </summary>
public class ConsumosService : IConsumosService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<ConsumosService> _logger;

    public ConsumosService(HotelDbContext context, ILogger<ConsumosService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<IEnumerable<ConsumoDto>>> GetConsumosByVisitaAsync(
        int visitaId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var consumos = await _context
                .Consumo
                .AsNoTracking()
                .Where(c => c.Movimientos!.VisitaId == visitaId && c.Anulado != true)
                .Include(c => c.Movimientos)
                .Include(c => c.Articulo)
                .Select(c => new ConsumoDto
                {
                    ConsumoId = c.ConsumoId,
                    ArticuloId = c.ArticuloId ?? 0,
                    ArticleName = c.Articulo!.NombreArticulo ?? string.Empty,
                    Cantidad = c.Cantidad ?? 0,
                    PrecioUnitario = c.PrecioUnitario ?? 0,
                    Total = (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0),
                    EsHabitacion = c.EsHabitacion ?? false,
                    FechaConsumo = c.Movimientos!.FechaRegistro ?? DateTime.Now,
                    VisitaId = c.Movimientos.VisitaId ?? 0,
                    HabitacionId = c.Movimientos.HabitacionId ?? 0,
                    Activo = !(c.Anulado ?? false),
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} consumos for visit {VisitaId}",
                consumos.Count,
                visitaId
            );

            return ApiResponse<IEnumerable<ConsumoDto>>.Success(consumos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consumos for visit {VisitaId}", visitaId);
            return ApiResponse<IEnumerable<ConsumoDto>>.Failure(
                "Error retrieving consumos",
                "An error occurred while retrieving the consumos"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> AddGeneralConsumosAsync(
        int habitacionId,
        int visitaId,
        IEnumerable<ConsumoCreateDto> items,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get or create movement
            var movimiento = await GetOrCreateMovementAsync(visitaId, habitacionId, cancellationToken);

            foreach (var item in items)
            {
                var consumo = new Consumo
                {
                    MovimientosId = movimiento.MovimientosId,
                    ArticuloId = item.ArticuloId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    EsHabitacion = false,
                    Anulado = false,
                };

                _context.Consumo.Add(consumo);

                // Update general inventory
                await UpdateInventoryAsync(item.ArticuloId, -item.Cantidad, null, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Added {Count} general consumos for visit {VisitaId}",
                items.Count(),
                visitaId
            );

            return ApiResponse.Success("Consumos added successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error adding general consumos for visit {VisitaId}", visitaId);
            return ApiResponse.Failure(
                "Error adding consumos",
                "An error occurred while adding the consumos"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> AddRoomConsumosAsync(
        int habitacionId,
        int visitaId,
        IEnumerable<ConsumoCreateDto> items,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get or create movement
            var movimiento = await GetOrCreateMovementAsync(visitaId, habitacionId, cancellationToken);

            foreach (var item in items)
            {
                var consumo = new Consumo
                {
                    MovimientosId = movimiento.MovimientosId,
                    ArticuloId = item.ArticuloId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    EsHabitacion = true,
                    Anulado = false,
                };

                _context.Consumo.Add(consumo);

                // Update room inventory
                await UpdateInventoryAsync(item.ArticuloId, -item.Cantidad, habitacionId, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Added {Count} room consumos for visit {VisitaId}",
                items.Count(),
                visitaId
            );

            return ApiResponse.Success("Room consumos added successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error adding room consumos for visit {VisitaId}", visitaId);
            return ApiResponse.Failure(
                "Error adding room consumos",
                "An error occurred while adding the room consumos"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> CancelConsumoAsync(
        int consumoId,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var consumo = await _context.Consumo
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.ConsumoId == consumoId, cancellationToken);

            if (consumo == null)
            {
                return ApiResponse.Failure(
                    "Consumo not found",
                    $"No consumo found with ID {consumoId}"
                );
            }

            consumo.Anulado = true;

            // Restore inventory
            var habitacionId = consumo.EsHabitacion == true ? consumo.Movimientos?.HabitacionId : null;
            await UpdateInventoryAsync(consumo.ArticuloId ?? 0, consumo.Cantidad ?? 0, habitacionId, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Cancelled consumo {ConsumoId}", consumoId);

            return ApiResponse.Success("Consumo cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling consumo {ConsumoId}", consumoId);
            return ApiResponse.Failure(
                "Error cancelling consumo",
                "An error occurred while cancelling the consumo"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ConsumoDto>> UpdateConsumoQuantityAsync(
        int consumoId,
        int quantity,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var consumo = await _context.Consumo
                .Include(c => c.Articulo)
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.ConsumoId == consumoId, cancellationToken);

            if (consumo == null)
            {
                return ApiResponse<ConsumoDto>.Failure(
                    "Consumo not found",
                    $"No consumo found with ID {consumoId}"
                );
            }

            var originalQuantity = consumo.Cantidad ?? 0;
            var quantityDifference = originalQuantity - quantity;
            
            consumo.Cantidad = quantity;

            // Update inventory with the difference
            if (quantityDifference != 0)
            {
                var habitacionId = consumo.EsHabitacion == true ? consumo.Movimientos?.HabitacionId : null;
                await UpdateInventoryAsync(consumo.ArticuloId ?? 0, quantityDifference, habitacionId, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var dto = new ConsumoDto
            {
                ConsumoId = consumo.ConsumoId,
                ArticuloId = consumo.ArticuloId ?? 0,
                ArticleName = consumo.Articulo?.NombreArticulo ?? string.Empty,
                Cantidad = consumo.Cantidad ?? 0,
                PrecioUnitario = consumo.PrecioUnitario ?? 0,
                Total = (consumo.Cantidad ?? 0) * (consumo.PrecioUnitario ?? 0),
                EsHabitacion = consumo.EsHabitacion ?? false,
                FechaConsumo = consumo.Movimientos?.FechaRegistro ?? DateTime.Now,
                VisitaId = consumo.Movimientos?.VisitaId ?? 0,
                HabitacionId = consumo.Movimientos?.HabitacionId ?? 0,
                Activo = !(consumo.Anulado ?? false),
            };

            _logger.LogInformation(
                "Updated consumo {ConsumoId} quantity to {Quantity}",
                consumoId,
                quantity
            );

            return ApiResponse<ConsumoDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consumo {ConsumoId}", consumoId);
            return ApiResponse<ConsumoDto>.Failure(
                "Error updating consumo",
                "An error occurred while updating the consumo"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ConsumoSummaryDto>> GetConsumosSummaryAsync(
        int visitaId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var consumos = await _context.Consumo
                .AsNoTracking()
                .Where(c => c.Movimientos!.VisitaId == visitaId && c.Anulado != true)
                .Include(c => c.Articulo)
                .Include(c => c.Movimientos)
                .ToListAsync(cancellationToken);

            var consumoDtos = consumos.Select(c => new ConsumoDto
            {
                ConsumoId = c.ConsumoId,
                ArticuloId = c.ArticuloId ?? 0,
                ArticleName = c.Articulo?.NombreArticulo ?? string.Empty,
                Cantidad = c.Cantidad ?? 0,
                PrecioUnitario = c.PrecioUnitario ?? 0,
                Total = (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0),
                EsHabitacion = c.EsHabitacion ?? false,
                FechaConsumo = c.Movimientos?.FechaRegistro ?? DateTime.Now,
                VisitaId = c.Movimientos?.VisitaId ?? 0,
                HabitacionId = c.Movimientos?.HabitacionId ?? 0,
                Activo = !(c.Anulado ?? false),
            }).ToList();

            var summary = new ConsumoSummaryDto
            {
                VisitaId = visitaId,
                TotalConsumos = consumoDtos.Count,
                TotalGeneral = consumoDtos.Where(c => !c.EsHabitacion).Sum(c => c.Total),
                TotalHabitacion = consumoDtos.Where(c => c.EsHabitacion).Sum(c => c.Total),
                TotalAmount = consumoDtos.Sum(c => c.Total),
                Consumos = consumoDtos,
                LastUpdated = DateTime.Now,
            };

            _logger.LogInformation("Generated consumos summary for visit {VisitaId}", visitaId);

            return ApiResponse<ConsumoSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error generating consumos summary for visit {VisitaId}",
                visitaId
            );
            return ApiResponse<ConsumoSummaryDto>.Failure(
                "Error generating summary",
                "An error occurred while generating the consumos summary"
            );
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Get or create a movement for the visit
    /// </summary>
    private async Task<Movimientos> GetOrCreateMovementAsync(
        int visitaId, 
        int habitacionId, 
        CancellationToken cancellationToken)
    {
        var existingMovement = await _context.Movimientos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.VisitaId == visitaId && m.Anulado != true, cancellationToken);

        if (existingMovement != null)
        {
            return existingMovement;
        }

        // Get institution ID from habitacion
        var habitacion = await _context.Habitaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.HabitacionId == habitacionId, cancellationToken);

        var newMovement = new Movimientos
        {
            VisitaId = visitaId,
            HabitacionId = habitacionId,
            FechaInicio = DateTime.Now,
            FechaRegistro = DateTime.Now,
            TotalFacturado = 0,
            Anulado = false,
            InstitucionID = habitacion?.InstitucionID ?? 0
        };

        _context.Movimientos.Add(newMovement);
        await _context.SaveChangesAsync(cancellationToken);

        return newMovement;
    }

    /// <summary>
    /// Update inventory (room or general) when consuming items
    /// </summary>
    private async Task UpdateInventoryAsync(
        int articuloId, 
        int quantityChange, 
        int? habitacionId, 
        CancellationToken cancellationToken)
    {
        if (habitacionId.HasValue)
        {
            // Update room inventory (No AsNoTracking because we need to update)
            var roomInventory = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.ArticuloId == articuloId && 
                                        i.HabitacionId == habitacionId.Value && 
                                        i.Anulado != true, cancellationToken);

            if (roomInventory != null)
            {
                roomInventory.Cantidad = (roomInventory.Cantidad ?? 0) + quantityChange;
                if (roomInventory.Cantidad < 0)
                {
                    roomInventory.Cantidad = 0;
                }
            }
        }
        else
        {
            // Update general inventory (No AsNoTracking because we need to update)
            var generalInventory = await _context.InventarioGeneral
                .FirstOrDefaultAsync(i => i.ArticuloId == articuloId && 
                                        i.Anulado != true, cancellationToken);

            if (generalInventory != null)
            {
                generalInventory.Cantidad = (generalInventory.Cantidad ?? 0) + quantityChange;
                if (generalInventory.Cantidad < 0)
                {
                    generalInventory.Cantidad = 0;
                }
            }
        }

        // Create stock movement record
        var stockMovement = new MovimientosStock
        {
            ArticuloId = articuloId,
            Cantidad = Math.Abs(quantityChange),
            TipoMovimientoId = quantityChange < 0 ? 2 : 1, // 2 = Egreso, 1 = Ingreso
            FechaMovimiento = DateTime.Now,
            FechaRegistro = DateTime.Now,
            Anulado = false
        };

        _context.MovimientosStock.Add(stockMovement);
    }

    #endregion
}

