using Microsoft.EntityFrameworkCore;
using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Reservas;
using hotel.Interfaces;
using hotel.Models;

namespace hotel.Services;

/// <summary>
/// Service implementation for managing reservas (reservations)
/// </summary>
public class ReservasService : IReservasService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<ReservasService> _logger;

    public ReservasService(HotelDbContext context, ILogger<ReservasService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> FinalizeReservationAsync(
        int habitacionId, 
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var habitacion = await _context.Habitaciones
                .FirstOrDefaultAsync(h => h.HabitacionId == habitacionId, cancellationToken);

            if (habitacion == null)
            {
                return ApiResponse.Failure("Room not found", $"No room found with ID {habitacionId}");
            }

            // Find active reservation for this room (reservation without end date)
            var reservaActiva = await _context.Reservas
                .Where(r => r.HabitacionId == habitacionId && r.FechaFin == null)
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
            _logger.LogError(ex, "Error finalizing reservation for room {HabitacionId}", habitacionId);
            return ApiResponse.Failure("Error finalizing reservation", "An error occurred while finalizing the reservation");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> PauseOccupationAsync(
        int visitaId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.VisitaId == visitaId && r.FechaFin == null, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse.Failure("Active reservation not found", $"No active reservation found for visit {visitaId}");
            }

            // Calculate current pause time (negative values indicate overtime in pause)
            var now = DateTime.Now;
            if (reserva.FechaReserva.HasValue && reserva.TotalHoras.HasValue && reserva.TotalMinutos.HasValue)
            {
                var endTime = reserva.FechaReserva.Value
                    .AddHours(reserva.TotalHoras.Value)
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
            return ApiResponse.Failure("Error pausing occupation", "An error occurred while pausing the occupation");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> ResumeOccupationAsync(
        int visitaId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.VisitaId == visitaId && r.FechaFin == null, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse.Failure("Active reservation not found", $"No active reservation found for visit {visitaId}");
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
            return ApiResponse.Failure("Error resuming occupation", "An error occurred while resuming the occupation");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReservaDto>> UpdateReservationPromotionAsync(
        int reservaId, 
        int? promocionId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reserva = await _context.Reservas
                .Include(r => r.Promocion)
                .Include(r => r.Habitacion)
                .ThenInclude(h => h!.Categoria)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Reservation not found", 
                    $"No reservation found with ID {reservaId}");
            }

            // Validate promotion if provided
            if (promocionId.HasValue)
            {
                var promocion = await _context.Promociones
                    .AsNoTracking()
                    .Include(p => p.Categoria)
                    .FirstOrDefaultAsync(p => p.PromocionID == promocionId.Value && p.Anulado != true, cancellationToken);

                if (promocion == null)
                {
                    return ApiResponse<ReservaDto>.Failure(
                        "Promotion not found", 
                        $"No active promotion found with ID {promocionId.Value}");
                }

                // Validate category compatibility
                if (reserva.Habitacion?.CategoriaId != promocion.CategoriaID)
                {
                    return ApiResponse<ReservaDto>.Failure(
                        "Incompatible promotion", 
                        "The promotion is not valid for this room category");
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
                UpdatedAt = DateTime.Now
            };

            _logger.LogInformation("Updated promotion for reservation {ReservaId} to {PromocionId}", 
                reservaId, promocionId);

            return ApiResponse<ReservaDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating promotion for reservation {ReservaId}", reservaId);
            return ApiResponse<ReservaDto>.Failure(
                "Error updating promotion", 
                "An error occurred while updating the reservation promotion");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> CancelReservationAsync(
        int reservaId, 
        string reason, 
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var reserva = await _context.Reservas
                .Include(r => r.Habitacion)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse.Failure("Reservation not found", $"No reservation found with ID {reservaId}");
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

            _logger.LogInformation("Cancelled reservation {ReservaId} with reason: {Reason}", 
                reservaId, reason);

            return ApiResponse.Success("Reservation cancelled successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error cancelling reservation {ReservaId}", reservaId);
            return ApiResponse.Failure("Error cancelling reservation", "An error occurred while cancelling the reservation");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReservaDto>> ExtendReservationAsync(
        int reservaId, 
        int additionalHours, 
        int additionalMinutes, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reserva = await _context.Reservas
                .Include(r => r.Promocion)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Reservation not found", 
                    $"No reservation found with ID {reservaId}");
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
                UpdatedAt = DateTime.Now
            };

            _logger.LogInformation("Extended reservation {ReservaId} by {Hours}h {Minutes}m", 
                reservaId, additionalHours, additionalMinutes);

            return ApiResponse<ReservaDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending reservation {ReservaId}", reservaId);
            return ApiResponse<ReservaDto>.Failure(
                "Error extending reservation", 
                "An error occurred while extending the reservation");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReservaDto>> GetReservationByIdAsync(
        int reservaId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reserva = await _context.Reservas
                .AsNoTracking()
                .Include(r => r.Promocion)
                .Include(r => r.Habitacion)
                .Include(r => r.Visita)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse<ReservaDto>.Failure(
                    "Reservation not found", 
                    $"No reservation found with ID {reservaId}");
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
                UpdatedAt = null
            };

            return ApiResponse<ReservaDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation {ReservaId}", reservaId);
            return ApiResponse<ReservaDto>.Failure(
                "Error retrieving reservation", 
                "An error occurred while retrieving the reservation");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<IEnumerable<ReservaDto>>> GetActiveReservationsAsync(
        int institucionId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reservas = await _context.Reservas
                .AsNoTracking()
                .Include(r => r.Promocion)
                .Include(r => r.Habitacion)
                .Include(r => r.Visita)
                .Where(r => r.FechaFin == null && r.FechaAnula == null && 
                           r.InstitucionID == institucionId)
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
                    UpdatedAt = null
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} active reservations for institution {InstitucionId}", 
                reservas.Count, institucionId);

            return ApiResponse<IEnumerable<ReservaDto>>.Success(reservas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active reservations for institution {InstitucionId}", institucionId);
            return ApiResponse<IEnumerable<ReservaDto>>.Failure(
                "Error retrieving reservations", 
                "An error occurred while retrieving the active reservations");
        }
    }
}