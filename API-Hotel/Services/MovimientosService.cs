using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Movimientos;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

public class MovimientosService : IMovimientosService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<MovimientosService> _logger;
    private readonly IRegistrosService _registrosService;

    public MovimientosService(
        HotelDbContext context,
        ILogger<MovimientosService> logger,
        IRegistrosService registrosService
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registrosService =
            registrosService ?? throw new ArgumentNullException(nameof(registrosService));
    }

    public async Task<ApiResponse<IEnumerable<MovimientoDto>>> GetAllAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var movimientos = await _context
                .Movimientos.AsNoTracking()
                .Include(m => m.Visita)
                .Include(m => m.Habitacion)
                .Include(m => m.Pago)
                .Include(m => m.Consumo)
                .Where(m => m.InstitucionID == institucionId)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync(cancellationToken);

            var movimientosDto = movimientos.Select(m => MapToDto(m)).ToList();

            _logger.LogInformation(
                "Retrieved {Count} movements for institution {InstitucionId}",
                movimientosDto.Count,
                institucionId
            );

            return ApiResponse<IEnumerable<MovimientoDto>>.Success(movimientosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving movements for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<MovimientoDto>>.Failure(
                "Error retrieving movements",
                "An error occurred while retrieving the movements"
            );
        }
    }

    public async Task<ApiResponse<MovimientoDto>> GetByIdAsync(
        int id,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var movimiento = await _context
                .Movimientos.AsNoTracking()
                .Include(m => m.Visita)
                .Include(m => m.Habitacion)
                .Include(m => m.Pago)
                .Include(m => m.Consumo)
                .FirstOrDefaultAsync(
                    m => m.MovimientosId == id && m.InstitucionID == institucionId,
                    cancellationToken
                );

            if (movimiento == null)
            {
                return ApiResponse<MovimientoDto>.Failure("Movement not found");
            }

            var movimientoDto = MapToDto(movimiento);

            _logger.LogInformation(
                "Retrieved movement {MovimientoId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse<MovimientoDto>.Success(movimientoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving movement {MovimientoId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse<MovimientoDto>.Failure(
                "Error retrieving movement",
                "An error occurred while retrieving the movement"
            );
        }
    }

    public async Task<ApiResponse<Movimientos>> CreateMovimientoAsync(
        MovimientoCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var movimiento = new Movimientos
            {
                VisitaId = createDto.VisitaId,
                InstitucionID = institucionId,
                TotalFacturado = createDto.TotalFacturado,
                HabitacionId = createDto.HabitacionId,
                FechaRegistro = DateTime.Now,
                Anulado = false,
                Descripcion = createDto.Descripcion,
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            // Log audit
            await _registrosService.LogAuditAsync(
                $"Nuevo Movimiento - Total: {movimiento.TotalFacturado:C}",
                ModuloSistema.PAGOS,
                institucionId,
                null,
                null,
                System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        MovimientoId = movimiento.MovimientosId,
                        VisitaId = movimiento.VisitaId,
                        Total = movimiento.TotalFacturado,
                        HabitacionId = movimiento.HabitacionId,
                    }
                ),
                movimiento.MovimientosId,
                cancellationToken
            );

            _logger.LogInformation(
                "Created movement {MovimientoId} for institution {InstitucionId}",
                movimiento.MovimientosId,
                institucionId
            );

            return ApiResponse<Movimientos>.Success(movimiento, "Movement created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating movement for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<Movimientos>.Failure(
                "Error creating movement",
                "An error occurred while creating the movement"
            );
        }
    }

    public async Task<ApiResponse<Movimientos>> CreateMovimientoHabitacionAsync(
        int visitaId,
        int institucionId,
        decimal totalFacturado,
        int habitacionId,
        string? descripcion = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var movimiento = new Movimientos
            {
                VisitaId = visitaId,
                InstitucionID = institucionId,
                TotalFacturado = totalFacturado,
                HabitacionId = habitacionId,
                FechaRegistro = DateTime.Now,
                Anulado = false,
                Descripcion = descripcion ?? "Movimiento por habitación",
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            // Log audit
            await _registrosService.LogAuditAsync(
                $"Movimiento Habitación - Habitación: {habitacionId} - Total: {totalFacturado:C}",
                ModuloSistema.HABITACIONES,
                institucionId,
                null,
                null,
                System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        MovimientoId = movimiento.MovimientosId,
                        VisitaId = visitaId,
                        HabitacionId = habitacionId,
                        Total = totalFacturado,
                    }
                ),
                movimiento.MovimientosId,
                cancellationToken
            );

            _logger.LogInformation(
                "Created room movement {MovimientoId} for visit {VisitaId} in institution {InstitucionId}",
                movimiento.MovimientosId,
                visitaId,
                institucionId
            );

            return ApiResponse<Movimientos>.Success(
                movimiento,
                "Room movement created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating room movement for visit {VisitaId} in institution {InstitucionId}",
                visitaId,
                institucionId
            );
            return ApiResponse<Movimientos>.Failure(
                "Error creating room movement",
                "An error occurred while creating the room movement"
            );
        }
    }

    public async Task<ApiResponse<MovimientoDto>> UpdateAsync(
        int id,
        MovimientoUpdateDto updateDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var movimiento = await _context.Movimientos.FirstOrDefaultAsync(
                m => m.MovimientosId == id && m.InstitucionID == institucionId,
                cancellationToken
            );

            if (movimiento == null)
            {
                return ApiResponse<MovimientoDto>.Failure("Movement not found");
            }

            if (movimiento.Anulado == true)
            {
                return ApiResponse<MovimientoDto>.Failure("Cannot update cancelled movement");
            }

            if (updateDto.TotalFacturado.HasValue)
                movimiento.TotalFacturado = updateDto.TotalFacturado.Value;

            if (!string.IsNullOrEmpty(updateDto.Descripcion))
                movimiento.Descripcion = updateDto.Descripcion;

            _context.Movimientos.Update(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            // Retrieve with includes for response
            var updatedMovimiento = await _context
                .Movimientos.AsNoTracking()
                .Include(m => m.Visita)
                .Include(m => m.Habitacion)
                .Include(m => m.Pago)
                .Include(m => m.Consumo)
                .FirstOrDefaultAsync(m => m.MovimientosId == id, cancellationToken);

            var movimientoDto = MapToDto(updatedMovimiento!);

            _logger.LogInformation(
                "Updated movement {MovimientoId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse<MovimientoDto>.Success(
                movimientoDto,
                "Movement updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating movement {MovimientoId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse<MovimientoDto>.Failure(
                "Error updating movement",
                "An error occurred while updating the movement"
            );
        }
    }

    public async Task<ApiResponse> AnularMovimientoAsync(
        int id,
        int institucionId,
        string? reason = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var movimiento = await _context
                .Movimientos.Include(m => m.Consumo)
                .FirstOrDefaultAsync(
                    m => m.MovimientosId == id && m.InstitucionID == institucionId,
                    cancellationToken
                );

            if (movimiento == null)
            {
                return ApiResponse.Failure("Movement not found");
            }

            if (movimiento.Anulado == true)
            {
                return ApiResponse.Failure("Movement is already cancelled");
            }

            if (movimiento.PagoId.HasValue)
            {
                return ApiResponse.Failure(
                    "Cannot cancel paid movement",
                    "This movement has been paid and cannot be cancelled"
                );
            }

            movimiento.Anulado = true;

            // Also cancel all related consumptions
            foreach (var consumo in movimiento.Consumo)
            {
                consumo.Anulado = true;
            }

            _context.Movimientos.Update(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            // Log audit
            await _registrosService.LogAuditAsync(
                $"Movimiento Anulado - ID: {movimiento.MovimientosId} - Motivo: {reason ?? "No especificado"}",
                ModuloSistema.PAGOS,
                institucionId,
                null,
                null,
                System.Text.Json.JsonSerializer.Serialize(
                    new { MovimientoId = movimiento.MovimientosId, Motivo = reason }
                ),
                movimiento.MovimientosId,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Cancelled movement {MovimientoId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse.Success("Movement cancelled successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error cancelling movement {MovimientoId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse.Failure(
                "Error cancelling movement",
                "An error occurred while cancelling the movement"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<MovimientoDto>>> GetByVisitaAsync(
        int visitaId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var movimientos = await _context
                .Movimientos.AsNoTracking()
                .Include(m => m.Visita)
                .Include(m => m.Habitacion)
                .Include(m => m.Pago)
                .Include(m => m.Consumo)
                .Where(m => m.VisitaId == visitaId && m.InstitucionID == institucionId)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync(cancellationToken);

            var movimientosDto = movimientos.Select(m => MapToDto(m)).ToList();

            _logger.LogInformation(
                "Retrieved {Count} movements for visit {VisitaId} in institution {InstitucionId}",
                movimientosDto.Count,
                visitaId,
                institucionId
            );

            return ApiResponse<IEnumerable<MovimientoDto>>.Success(movimientosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving movements for visit {VisitaId} in institution {InstitucionId}",
                visitaId,
                institucionId
            );
            return ApiResponse<IEnumerable<MovimientoDto>>.Failure(
                "Error retrieving movements",
                "An error occurred while retrieving the movements"
            );
        }
    }

    public async Task<ApiResponse<decimal>> GetTotalByVisitaAsync(
        int visitaId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var total = await _context
                .Movimientos.Where(m =>
                    m.VisitaId == visitaId && m.InstitucionID == institucionId && m.Anulado != true
                )
                .SumAsync(m => m.TotalFacturado ?? 0, cancellationToken);

            _logger.LogInformation(
                "Calculated total {Total} for visit {VisitaId} in institution {InstitucionId}",
                total,
                visitaId,
                institucionId
            );

            return ApiResponse<decimal>.Success(total);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error calculating total for visit {VisitaId} in institution {InstitucionId}",
                visitaId,
                institucionId
            );
            return ApiResponse<decimal>.Failure(
                "Error calculating total",
                "An error occurred while calculating the total"
            );
        }
    }

    public async Task<ApiResponse> UpdatePagoIdAsync(
        int movimientoId,
        int pagoId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var movimiento = await _context.Movimientos.FirstOrDefaultAsync(
                m => m.MovimientosId == movimientoId && m.InstitucionID == institucionId,
                cancellationToken
            );

            if (movimiento == null)
            {
                return ApiResponse.Failure("Movement not found");
            }

            if (movimiento.Anulado == true)
            {
                return ApiResponse.Failure("Cannot update payment for cancelled movement");
            }

            movimiento.PagoId = pagoId;

            _context.Movimientos.Update(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated payment {PagoId} for movement {MovimientoId} in institution {InstitucionId}",
                pagoId,
                movimientoId,
                institucionId
            );

            return ApiResponse.Success("Payment updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating payment for movement {MovimientoId} in institution {InstitucionId}",
                movimientoId,
                institucionId
            );
            return ApiResponse.Failure(
                "Error updating payment",
                "An error occurred while updating the payment"
            );
        }
    }

    private MovimientoDto MapToDto(Movimientos movimiento)
    {
        return new MovimientoDto
        {
            MovimientosId = movimiento.MovimientosId,
            VisitaId = movimiento.VisitaId,
            VisitaIdentificador = movimiento.Visita?.Identificador,
            InstitucionID = movimiento.InstitucionID,
            PagoId = movimiento.PagoId,
            FechaPago = movimiento.Pago?.fechaHora,
            TotalFacturado = movimiento.TotalFacturado,
            HabitacionId = movimiento.HabitacionId,
            HabitacionNombre = movimiento.Habitacion?.NombreHabitacion,
            FechaRegistro = movimiento.FechaRegistro,
            UserId = movimiento.UserId,
            UsuarioNombre = null, // TODO: Add user navigation if needed
            Anulado = movimiento.Anulado,
            Descripcion = movimiento.Descripcion,
            TotalConsumos = movimiento.Consumo?.Count(c => c.Anulado != true) ?? 0,
            TotalConsumosAmount =
                movimiento
                    .Consumo?.Where(c => c.Anulado != true)
                    .Sum(c => (c.Cantidad ?? 0) * (c.PrecioUnitario ?? 0)) ?? 0,
        };
    }
}

