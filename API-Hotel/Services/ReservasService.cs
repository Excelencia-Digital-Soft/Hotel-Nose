using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Reservas;
using hotel.DTOs.Visitas;
using hotel.DTOs.Movimientos;
using hotel.DTOs.Consumos;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service implementation for managing reservas (reservations)
/// </summary>
public class ReservasService : IReservasService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<ReservasService> _logger;
    private readonly IRegistrosService _registrosService;
    private readonly IVisitasService _visitasService;
    private readonly IMovimientosService _movimientosService;
    private readonly IPromocionesService _promocionesService;

    public ReservasService(
        HotelDbContext context,
        ILogger<ReservasService> logger,
        IRegistrosService registrosService,
        IVisitasService visitasService,
        IMovimientosService movimientosService,
        IPromocionesService promocionesService
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registrosService =
            registrosService ?? throw new ArgumentNullException(nameof(registrosService));
        _visitasService = visitasService ?? throw new ArgumentNullException(nameof(visitasService));
        _movimientosService = movimientosService ?? throw new ArgumentNullException(nameof(movimientosService));
        _promocionesService = promocionesService ?? throw new ArgumentNullException(nameof(promocionesService));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> FinalizeReservationAsync(
        int habitacionId,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var habitacion = await _context.Habitaciones.FirstOrDefaultAsync(
                h => h.HabitacionId == habitacionId,
                cancellationToken
            );

            if (habitacion == null)
            {
                return ApiResponse.Failure(
                    "Room not found",
                    $"No room found with ID {habitacionId}"
                );
            }

            // Find active reservation for this room (reservation without end date)
            var reservaActiva = await _context
                .Reservas.Where(r => r.HabitacionId == habitacionId && r.FechaFin == null)
                .FirstOrDefaultAsync(cancellationToken);

            if (reservaActiva != null)
            {
                reservaActiva.FechaFin = DateTime.Now;
                reservaActiva.FechaAnula = null; // Ensure it's not marked as cancelled
            }

            // Mark room as available
            habitacion.Disponible = true;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Finalized reservation for room {HabitacionId}", habitacionId);

            return ApiResponse.Success("Reservation finalized successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error finalizing reservation for room {HabitacionId}",
                habitacionId
            );
            return ApiResponse.Failure(
                "Error finalizing reservation",
                "An error occurred while finalizing the reservation"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> PauseOccupationAsync(
        int visitaId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var reserva = await _context.Reservas.FirstOrDefaultAsync(
                r => r.VisitaId == visitaId && r.FechaFin == null,
                cancellationToken
            );

            if (reserva == null)
            {
                return ApiResponse.Failure(
                    "Active reservation not found",
                    $"No active reservation found for visit {visitaId}"
                );
            }

            // Calculate current pause time (negative values indicate overtime in pause)
            var now = DateTime.Now;
            if (
                reserva.FechaReserva.HasValue
                && reserva.TotalHoras.HasValue
                && reserva.TotalMinutos.HasValue
            )
            {
                var endTime = reserva
                    .FechaReserva.Value.AddHours(reserva.TotalHoras.Value)
                    .AddMinutes(reserva.TotalMinutos.Value);
                var timeDiff = now - endTime;

                if (timeDiff.TotalMinutes > 0)
                {
                    // In overtime - set negative pause values
                    reserva.PausaHoras = -(int)timeDiff.Hours;
                    reserva.PausaMinutos = -(int)(timeDiff.TotalMinutes % 60);
                }
                else
                {
                    // Still in regular time - set positive pause values
                    var remainingTime = endTime - now;
                    reserva.PausaHoras = (int)remainingTime.Hours;
                    reserva.PausaMinutos = (int)(remainingTime.TotalMinutes % 60);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Paused occupation for visit {VisitaId}", visitaId);

            return ApiResponse.Success("Occupation paused successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing occupation for visit {VisitaId}", visitaId);
            return ApiResponse.Failure(
                "Error pausing occupation",
                "An error occurred while pausing the occupation"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> ResumeOccupationAsync(
        int visitaId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var reserva = await _context.Reservas.FirstOrDefaultAsync(
                r => r.VisitaId == visitaId && r.FechaFin == null,
                cancellationToken
            );

            if (reserva == null)
            {
                return ApiResponse.Failure(
                    "Active reservation not found",
                    $"No active reservation found for visit {visitaId}"
                );
            }

            // Reset pause values
            reserva.PausaHoras = 0;
            reserva.PausaMinutos = 0;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Resumed occupation for visit {VisitaId}", visitaId);

            return ApiResponse.Success("Occupation resumed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming occupation for visit {VisitaId}", visitaId);
            return ApiResponse.Failure(
                "Error resuming occupation",
                "An error occurred while resuming the occupation"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReservaDto>> UpdateReservationPromotionAsync(
        int reservaId,
        int? promocionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var reserva = await _context
                .Reservas.Include(r => r.Promocion)
                .Include(r => r.Habitacion)
                .ThenInclude(h => h!.Categoria)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Reservation not found",
                    $"No reservation found with ID {reservaId}"
                );
            }

            // Validate promotion if provided using PromocionesService
            if (promocionId.HasValue && promocionId.Value > 0)
            {
                var promocionValidation = await _promocionesService.ValidateAndGetPromocionAsync(
                    promocionId.Value,
                    reserva.InstitucionID,
                    reserva.Habitacion?.CategoriaId,
                    cancellationToken
                );

                if (!promocionValidation.IsSuccess)
                {
                    return ApiResponse<ReservaDto>.Failure(
                        "Error validating promotion",
                        "An error occurred while validating the promotion"
                    );
                }

                var validationResult = promocionValidation.Data!;
                if (!validationResult.IsValid)
                {
                    return ApiResponse<ReservaDto>.Failure(
                        "Invalid promotion",
                        validationResult.ErrorMessage ?? "The promotion is not valid"
                    );
                }
            }

            reserva.PromocionId = promocionId;

            await _context.SaveChangesAsync(cancellationToken);

            var dto = new ReservaDto
            {
                ReservaId = reserva.ReservaId,
                HabitacionId = reserva.HabitacionId ?? 0,
                VisitaId = reserva.VisitaId ?? 0,
                FechaInicio = reserva.FechaReserva ?? DateTime.MinValue,
                FechaFin = reserva.FechaFin,
                TotalHoras = reserva.TotalHoras ?? 0,
                TotalMinutos = reserva.TotalMinutos ?? 0,
                PromocionId = reserva.PromocionId,
                PromocionNombre = reserva.Promocion?.Detalle,
                PromocionTarifa = reserva.Promocion?.Tarifa,
                PausaHoras = reserva.PausaHoras ?? 0,
                PausaMinutos = reserva.PausaMinutos ?? 0,
                EsReserva = true, // Default value (field doesn't exist in model)
                Activo = reserva.FechaFin == null, // Active if no end date
                CreatedAt = reserva.FechaRegistro ?? DateTime.MinValue,
                UpdatedAt = DateTime.Now,
            };

            _logger.LogInformation(
                "Updated promotion for reservation {ReservaId} to {PromocionId}",
                reservaId,
                promocionId
            );

            return ApiResponse<ReservaDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating promotion for reservation {ReservaId}", reservaId);
            return ApiResponse<ReservaDto>.Failure(
                "Error updating promotion",
                "An error occurred while updating the reservation promotion"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> CancelReservationAsync(
        int reservaId,
        string reason,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var reserva = await _context
                .Reservas.Include(r => r.Habitacion)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse.Failure(
                    "Reservation not found",
                    $"No reservation found with ID {reservaId}"
                );
            }

            // Mark reservation as cancelled
            reserva.FechaAnula = DateTime.Now;
            reserva.FechaFin = DateTime.Now;

            // Free the room
            if (reserva.Habitacion != null)
            {
                reserva.Habitacion.Disponible = true;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Cancelled reservation {ReservaId} with reason: {Reason}",
                reservaId,
                reason
            );

            return ApiResponse.Success("Reservation cancelled successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error cancelling reservation {ReservaId}", reservaId);
            return ApiResponse.Failure(
                "Error cancelling reservation",
                "An error occurred while cancelling the reservation"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReservaDto>> ExtendReservationAsync(
        int reservaId,
        int additionalHours,
        int additionalMinutes,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var reserva = await _context
                .Reservas.Include(r => r.Promocion)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Reservation not found",
                    $"No reservation found with ID {reservaId}"
                );
            }

            reserva.TotalHoras = (reserva.TotalHoras ?? 0) + additionalHours;
            reserva.TotalMinutos = (reserva.TotalMinutos ?? 0) + additionalMinutes;

            // Normalize minutes
            if (reserva.TotalMinutos >= 60)
            {
                reserva.TotalHoras += reserva.TotalMinutos / 60;
                reserva.TotalMinutos %= 60;
            }

            await _context.SaveChangesAsync(cancellationToken);

            var dto = new ReservaDto
            {
                ReservaId = reserva.ReservaId,
                HabitacionId = reserva.HabitacionId ?? 0,
                VisitaId = reserva.VisitaId ?? 0,
                FechaInicio = reserva.FechaReserva ?? DateTime.MinValue,
                FechaFin = reserva.FechaFin,
                TotalHoras = reserva.TotalHoras ?? 0,
                TotalMinutos = reserva.TotalMinutos ?? 0,
                PromocionId = reserva.PromocionId,
                PromocionNombre = reserva.Promocion?.Detalle,
                PromocionTarifa = reserva.Promocion?.Tarifa,
                PausaHoras = reserva.PausaHoras ?? 0,
                PausaMinutos = reserva.PausaMinutos ?? 0,
                EsReserva = true,
                Activo = reserva.FechaFin == null,
                CreatedAt = reserva.FechaRegistro ?? DateTime.MinValue,
                UpdatedAt = DateTime.Now,
            };

            _logger.LogInformation(
                "Extended reservation {ReservaId} by {Hours}h {Minutes}m",
                reservaId,
                additionalHours,
                additionalMinutes
            );

            return ApiResponse<ReservaDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending reservation {ReservaId}", reservaId);
            return ApiResponse<ReservaDto>.Failure(
                "Error extending reservation",
                "An error occurred while extending the reservation"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReservaDto>> GetReservationByIdAsync(
        int reservaId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var reserva = await _context
                .Reservas.AsNoTracking()
                .Include(r => r.Promocion)
                .Include(r => r.Habitacion)
                .Include(r => r.Visita)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Reservation not found",
                    $"No reservation found with ID {reservaId}"
                );
            }

            var dto = new ReservaDto
            {
                ReservaId = reserva.ReservaId,
                HabitacionId = reserva.HabitacionId ?? 0,
                VisitaId = reserva.VisitaId ?? 0,
                FechaInicio = reserva.FechaReserva ?? DateTime.MinValue,
                FechaFin = reserva.FechaFin,
                TotalHoras = reserva.TotalHoras ?? 0,
                TotalMinutos = reserva.TotalMinutos ?? 0,
                PromocionId = reserva.PromocionId,
                PromocionNombre = reserva.Promocion?.Detalle,
                PromocionTarifa = reserva.Promocion?.Tarifa,
                PausaHoras = reserva.PausaHoras ?? 0,
                PausaMinutos = reserva.PausaMinutos ?? 0,
                EsReserva = true,
                Activo = reserva.FechaFin == null && reserva.FechaAnula == null,
                CreatedAt = reserva.FechaRegistro ?? DateTime.MinValue,
                UpdatedAt = null,
            };

            return ApiResponse<ReservaDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation {ReservaId}", reservaId);
            return ApiResponse<ReservaDto>.Failure(
                "Error retrieving reservation",
                "An error occurred while retrieving the reservation"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<IEnumerable<ReservaDto>>> GetActiveReservationsAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var reservas = await _context
                .Reservas.AsNoTracking()
                .Include(r => r.Promocion)
                .Include(r => r.Habitacion)
                .Include(r => r.Visita)
                .Where(r =>
                    r.FechaFin == null && r.FechaAnula == null && r.InstitucionID == institucionId
                )
                .Select(r => new ReservaDto
                {
                    ReservaId = r.ReservaId,
                    HabitacionId = r.HabitacionId ?? 0,
                    VisitaId = r.VisitaId ?? 0,
                    FechaInicio = r.FechaReserva ?? DateTime.MinValue,
                    FechaFin = r.FechaFin,
                    TotalHoras = r.TotalHoras ?? 0,
                    TotalMinutos = r.TotalMinutos ?? 0,
                    PromocionId = r.PromocionId,
                    PromocionNombre = r.Promocion!.Detalle,
                    PromocionTarifa = r.Promocion.Tarifa,
                    PausaHoras = r.PausaHoras ?? 0,
                    PausaMinutos = r.PausaMinutos ?? 0,
                    EsReserva = true,
                    Activo = true, // All returned reservations are active
                    CreatedAt = r.FechaRegistro ?? DateTime.MinValue,
                    UpdatedAt = null,
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} active reservations for institution {InstitucionId}",
                reservas.Count,
                institucionId
            );

            return ApiResponse<IEnumerable<ReservaDto>>.Success(reservas);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving active reservations for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<ReservaDto>>.Failure(
                "Error retrieving reservations",
                "An error occurred while retrieving the active reservations"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> ComprehensiveCancelOccupationAsync(
        int reservaId,
        string reason,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        // Validate reason length
        if (!string.IsNullOrEmpty(reason) && reason.Length > 150)
        {
            return ApiResponse.Failure(
                "Invalid reason",
                "Cancellation reason cannot exceed 150 characters"
            );
        }

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var cancellationTime = DateTime.Now;

            // Find the reservation with all related data
            var reserva = await _context
                .Reservas.Include(r => r.Habitacion)
                .Include(r => r.Visita)
                .AsSplitQuery() // Optimize for multiple includes
                .FirstOrDefaultAsync(
                    r => r.ReservaId == reservaId && r.InstitucionID == institucionId,
                    cancellationToken
                );

            if (reserva == null)
            {
                return ApiResponse.Failure(
                    "Reservation not found",
                    $"No reservation found with ID {reservaId}"
                );
            }

            if (reserva.Visita == null)
            {
                return ApiResponse.Failure(
                    "Visit not found",
                    "No visit associated with this reservation"
                );
            }

            // Check if already cancelled
            if (reserva.FechaAnula.HasValue)
            {
                return ApiResponse.Failure(
                    "Already cancelled",
                    "This reservation has already been cancelled"
                );
            }

            // 1. Get all movements and their consumptions in a single query
            var movimientosIds = await _context
                .Movimientos.Where(m => m.VisitaId == reserva.VisitaId && m.Anulado != true)
                .Select(m => m.MovimientosId)
                .ToListAsync(cancellationToken);

            if (movimientosIds.Any())
            {
                // Bulk update movements
                await _context
                    .Movimientos.Where(m => movimientosIds.Contains(m.MovimientosId))
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(m => m.Anulado, true),
                        cancellationToken
                    );

                // Get all consumptions with their article info for inventory restoration
                var consumosToRestore = await _context
                    .Consumo.Where(c =>
                        movimientosIds.Contains(c.MovimientosId ?? 0) && c.Anulado != true
                    )
                    .Select(c => new ConsumoInventoryRestoreDto
                    {
                        ConsumoId = c.ConsumoId,
                        ArticuloId = c.ArticuloId,
                        Cantidad = c.Cantidad,
                        EsHabitacion = c.EsHabitacion,
                    })
                    .ToListAsync(cancellationToken);

                if (consumosToRestore.Any())
                {
                    // Bulk update consumptions
                    var consumoIds = consumosToRestore.Select(c => c.ConsumoId).ToList();
                    await _context
                        .Consumo.Where(c => consumoIds.Contains(c.ConsumoId))
                        .ExecuteUpdateAsync(
                            setters => setters.SetProperty(c => c.Anulado, true),
                            cancellationToken
                        );

                    // Restore inventory
                    await RestoreInventoryAsync(
                        consumosToRestore,
                        reserva.HabitacionId,
                        institucionId,
                        cancellationToken
                    );
                }
            }

            // 4. Update reservation status
            reserva.FechaAnula = cancellationTime;

            // 5. Mark the room as available and clear VisitaID
            if (reserva.Habitacion != null)
            {
                reserva.Habitacion.Disponible = true;
                reserva.Habitacion.VisitaID = null;
            }

            // 6. Mark the visit as cancelled
            reserva.Visita.Anulado = true;

            // 7. Create audit log entry using RegistrosService
            await _registrosService.LogAuditAsync(
                $"Cancelación de Ocupación - Habitación: {reserva.HabitacionId} - Motivo: {reason ?? "Sin motivo"}",
                ModuloSistema.RESERVAS,
                institucionId,
                userId,
                null, // direccionIP se puede obtener del contexto HTTP si es necesario
                System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        HabitacionId = reserva.HabitacionId,
                        Motivo = reason,
                        FechaCancelacion = DateTime.UtcNow,
                    }
                ),
                reserva.ReservaId,
                cancellationToken
            );

            // Save all changes
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Comprehensive cancellation completed for reservation {ReservaId} in institution {InstitucionId} by user {UserId}",
                reservaId,
                institucionId,
                userId
            );

            return ApiResponse.Success("Occupation cancelled successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error during comprehensive cancellation of reservation {ReservaId} in institution {InstitucionId}",
                reservaId,
                institucionId
            );

            return ApiResponse.Failure(
                "Error cancelling occupation",
                "An error occurred while cancelling the occupation. Please try again."
            );
        }
    }

    private async Task RestoreInventoryAsync(
        IEnumerable<ConsumoInventoryRestoreDto> consumosToRestore,
        int? habitacionId,
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        // Group consumptions by article and inventory type
        var roomInventoryItems = consumosToRestore
            .Where(c => c.EsHabitacion == true && c.ArticuloId.HasValue)
            .GroupBy(c => c.ArticuloId!.Value)
            .Select(g => new
            {
                ArticuloId = g.Key,
                TotalCantidad = g.Sum(c => (decimal)(c.Cantidad ?? 0)),
            })
            .ToList();

        var generalInventoryItems = consumosToRestore
            .Where(c => c.EsHabitacion != true && c.ArticuloId.HasValue)
            .GroupBy(c => c.ArticuloId!.Value)
            .Select(g => new
            {
                ArticuloId = g.Key,
                TotalCantidad = g.Sum(c => (decimal)(c.Cantidad ?? 0)),
            })
            .ToList();

        // Restore room inventory
        if (roomInventoryItems.Any() && habitacionId.HasValue)
        {
            var articleIds = roomInventoryItems.Select(i => i.ArticuloId).ToList();
            var inventarios = await _context
                .Inventarios.Where(i =>
                    articleIds.Contains(i.ArticuloId ?? 0) && i.HabitacionId == habitacionId
                )
                .ToListAsync(cancellationToken);

            foreach (var item in roomInventoryItems)
            {
                var inventario = inventarios.FirstOrDefault(i => i.ArticuloId == item.ArticuloId);
                if (inventario != null)
                {
                    inventario.Cantidad = (inventario.Cantidad ?? 0) + (int)item.TotalCantidad;
                }
            }
        }

        // Restore general inventory
        if (generalInventoryItems.Any())
        {
            var articleIds = generalInventoryItems.Select(i => i.ArticuloId).ToList();
            var inventariosGenerales = await _context
                .InventarioGeneral.Where(i =>
                    articleIds.Contains(i.ArticuloId ?? 0) && i.InstitucionID == institucionId
                )
                .ToListAsync(cancellationToken);

            foreach (var item in generalInventoryItems)
            {
                var inventario = inventariosGenerales.FirstOrDefault(i =>
                    i.ArticuloId == item.ArticuloId
                );
                if (inventario != null)
                {
                    inventario.Cantidad = (inventario.Cantidad ?? 0) + (int)item.TotalCantidad;
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReservaDto>> CreateReservationAsync(
        ReservaCreateDto createDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation
        var (IsValid, ErrorMessage) = ValidateReservationInput(createDto);
        if (!IsValid)
        {
            return ApiResponse<ReservaDto>.Failure(
                "Validation failed",
                ErrorMessage
            );
        }

        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.RepeatableRead, // Prevent concurrent room bookings
            cancellationToken
        );

        try
        {
            var currentTime = DateTime.Now;

            // 1. Validate and lock room for update (prevents concurrent booking)
            var habitacion = await _context
                .Habitaciones.Include(h => h.Categoria)
                .Where(h =>
                    h.HabitacionId == createDto.HabitacionId && h.InstitucionID == institucionId
                )
                .FirstOrDefaultAsync(cancellationToken);

            if (habitacion == null)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Room not found",
                    $"No room found with ID {createDto.HabitacionId} in this institution"
                );
            }

            // Double-check availability with lock
            if (habitacion.Disponible != true)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Room unavailable",
                    "The selected room is not available. It may have been booked by another user."
                );
            }

            // Validate room category exists
            if (habitacion.Categoria == null)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Invalid room configuration",
                    "Room category not configured properly"
                );
            }

            // 2. Calculate pricing (with optional promotion validation)
            var pricingResult = await CalculateReservationPricingAsync(
                habitacion.Categoria.PrecioNormal ?? 0,
                createDto.PromocionId,
                createDto.TotalHoras,
                createDto.TotalMinutos,
                institucionId,
                habitacion.CategoriaId, // Pass category for promotion validation
                cancellationToken
            );

            if (!pricingResult.IsValid)
            {
                return ApiResponse<ReservaDto>.Failure("Pricing error", pricingResult.ErrorMessage);
            }

            // 3. Create Visita using VisitasService
            var visitaCreateDto = new VisitaCreateDto
            {
                PatenteVehiculo = createDto.Guest.PatenteVehiculo?.Trim(),
                NumeroTelefono = createDto.Guest.NumeroTelefono?.Trim(),
                Identificador = createDto.Guest.Identificador?.Trim(),
                HabitacionId = createDto.HabitacionId,
                EsReserva = createDto.EsReserva
            };

            var visitaResult = await _visitasService.CreateVisitaAsync(
                visitaCreateDto, 
                institucionId, 
                userId, 
                cancellationToken);
                
            if (!visitaResult.IsSuccess)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Error creating visit", 
                    visitaResult.Errors.FirstOrDefault() ?? "Failed to create visit");
            }

            var visita = visitaResult.Data!;

            // 4. Create Movimiento using MovimientosService
            var movimientoResult = await _movimientosService.CreateMovimientoHabitacionAsync(
                visita.VisitaId,
                institucionId,
                pricingResult.TotalAmount,
                createDto.HabitacionId,
                "Movimiento por reserva de habitación",
                cancellationToken);
                
            if (!movimientoResult.IsSuccess)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Error creating movement", 
                    movimientoResult.Errors.FirstOrDefault() ?? "Failed to create movement");
            }

            var movimiento = movimientoResult.Data!;

            // 5. Create Reserva entity directly (as this is the core responsibility)
            var reserva = new Reservas
            {
                VisitaId = visita.VisitaId,
                HabitacionId = createDto.HabitacionId,
                FechaReserva = createDto.FechaInicio,
                InstitucionID = institucionId,
                FechaFin = null, // Active reservation
                TotalHoras = createDto.TotalHoras,
                TotalMinutos = createDto.TotalMinutos,
                UserId = userId,
                FechaRegistro = currentTime,
                FechaAnula = null,
                PromocionId = createDto.PromocionId == 0 ? null : createDto.PromocionId,
                MovimientoId = movimiento.MovimientosId,
                PausaHoras = 0,
                PausaMinutos = 0,
            };

            // 6. Add reservation and update room status
            _context.Reservas.Add(reserva);
            habitacion.Disponible = false;
            habitacion.VisitaID = visita.VisitaId;

            // 7. Save changes
            await _context.SaveChangesAsync(cancellationToken);

            // 8. Create audit log entry using RegistrosService
            await _registrosService.LogAuditAsync(
                $"Nueva Reserva - Habitación: {habitacion.HabitacionId} - Huésped: {visita.Identificador ?? "Sin identificación"} - Usuario: {userId}",
                ModuloSistema.RESERVAS,
                institucionId,
                userId,
                null, // direccionIP se puede obtener del contexto HTTP si es necesario
                System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        habitacion.HabitacionId,
                        HuespedIdentificador = visita.Identificador,
                        TarifaId = createDto.PromocionId,
                        FechaCreacion = DateTime.UtcNow,
                    }
                ),
                reserva.ReservaId,
                cancellationToken
            );

            // 9. Commit transaction
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Reservation created successfully: ReservaId={ReservaId}, HabitacionId={HabitacionId}, VisitaId={VisitaId}, TotalAmount={TotalAmount}",
                reserva.ReservaId,
                createDto.HabitacionId,
                visita.VisitaId,
                pricingResult.TotalAmount
            );

            // 10. Return created reservation as DTO
            var reservaDto = new ReservaDto
            {
                ReservaId = reserva.ReservaId,
                HabitacionId = reserva.HabitacionId ?? 0,
                VisitaId = reserva.VisitaId ?? 0,
                FechaInicio = reserva.FechaReserva ?? DateTime.MinValue,
                FechaFin = reserva.FechaFin,
                TotalHoras = reserva.TotalHoras ?? 0,
                TotalMinutos = reserva.TotalMinutos ?? 0,
                PromocionId = reserva.PromocionId,
                PromocionNombre = pricingResult.PromocionNombre,
                PromocionTarifa = pricingResult.PromocionTarifa,
                PausaHoras = 0,
                PausaMinutos = 0,
                EsReserva = createDto.EsReserva,
                Activo = true,
                CreatedAt = reserva.FechaRegistro ?? DateTime.MinValue,
                UpdatedAt = null,
            };

            return ApiResponse<ReservaDto>.Success(reservaDto, "Reservation created successfully");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(
                ex,
                "Concurrency conflict while creating reservation for room {HabitacionId}",
                createDto.HabitacionId
            );

            return ApiResponse<ReservaDto>.Failure(
                "Room no longer available",
                "The room was booked by another user. Please select a different room."
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error creating reservation for room {HabitacionId} in institution {InstitucionId}",
                createDto.HabitacionId,
                institucionId
            );

            return ApiResponse<ReservaDto>.Failure(
                "Error creating reservation",
                "An unexpected error occurred. Please try again or contact support."
            );
        }
    }

    private (bool IsValid, string ErrorMessage) ValidateReservationInput(ReservaCreateDto createDto)
    {
        // Check minimum duration
        if (createDto.TotalHoras == 0 && createDto.TotalMinutos == 0)
        {
            return (false, "Reservation must have at least 1 minute duration");
        }

        // Check maximum duration
        var totalMinutes = (createDto.TotalHoras * 60) + createDto.TotalMinutos;
        if (totalMinutes > 10080) // 7 days max
        {
            return (false, "Reservation cannot exceed 7 days");
        }

        // Check date is not too far in the past
        if (createDto.FechaInicio < DateTime.Now.AddMinutes(-5))
        {
            return (false, "Reservation start date cannot be in the past");
        }

        // Check date is not too far in the future
        if (createDto.FechaInicio > DateTime.Now.AddDays(365))
        {
            return (false, "Reservation cannot be more than 1 year in the future");
        }

        // Validate guest information
        if (createDto.Guest == null)
        {
            return (false, "Guest information is required");
        }

        return (true, string.Empty);
    }

    private async Task<PricingResult> CalculateReservationPricingAsync(
        decimal basePrice,
        int? promocionId,
        int hours,
        int minutes,
        int institucionId,
        int? categoriaId = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = new PricingResult { IsValid = true };

        try
        {
            // Use base price by default
            result.TariffRate = basePrice;

            // Check for promotion if provided
            if (promocionId.HasValue && promocionId.Value > 0)
            {
                var promocionValidation = await _promocionesService.ValidateAndGetPromocionAsync(
                    promocionId.Value,
                    institucionId,
                    categoriaId,
                    cancellationToken
                );

                if (!promocionValidation.IsSuccess)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Error al validar la promoción. Inténtelo de nuevo.";
                    return result;
                }

                var validationResult = promocionValidation.Data!;
                if (!validationResult.IsValid)
                {
                    result.IsValid = false;
                    result.ErrorMessage = validationResult.ErrorMessage ?? "La promoción no es válida.";
                    return result;
                }

                // Use promotion pricing
                result.TariffRate = validationResult.PromocionTarifa ?? basePrice;
                result.PromocionNombre = validationResult.PromocionNombre;
                result.PromocionTarifa = validationResult.PromocionTarifa;

                _logger.LogInformation(
                    "Applied promotion {PromocionId} with rate {Rate} for institution {InstitucionId}",
                    promocionId.Value,
                    result.TariffRate,
                    institucionId
                );
            }
            else
            {
                _logger.LogInformation(
                    "No promotion applied, using base price {BasePrice} for institution {InstitucionId}",
                    basePrice,
                    institucionId
                );
            }

            // Validate tariff
            if (result.TariffRate <= 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "Invalid pricing configuration. Please contact support.";
                return result;
            }

            // Calculate total
            var totalHours = hours + (minutes / 60.0m);
            result.TotalAmount = Math.Round(result.TariffRate * totalHours, 2);

            _logger.LogDebug(
                "Calculated pricing: Rate={Rate}, Hours={Hours}, Total={Total}",
                result.TariffRate,
                totalHours,
                result.TotalAmount
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating pricing for reservation");
            result.IsValid = false;
            result.ErrorMessage = "Error calculating reservation price";
            return result;
        }
    }

    private class PricingResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public decimal TariffRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PromocionNombre { get; set; }
        public decimal? PromocionTarifa { get; set; }
    }
}
