using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Visitas;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

public class VisitasService : IVisitasService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<VisitasService> _logger;
    private readonly IRegistrosService _registrosService;

    public VisitasService(
        HotelDbContext context,
        ILogger<VisitasService> logger,
        IRegistrosService registrosService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registrosService = registrosService ?? throw new ArgumentNullException(nameof(registrosService));
    }

    public async Task<ApiResponse<IEnumerable<VisitaDto>>> GetAllAsync(
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var visitas = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.Usuario)
                // .Include(v => v.Habitacion) // Ignored in EF configuration
                .Include(v => v.Reservas)
                .Where(v => v.InstitucionID == institucionId)
                .OrderByDescending(v => v.FechaRegistro)
                .ToListAsync(cancellationToken);

            var visitasDto = visitas.Select(v => MapToDto(v)).ToList();

            _logger.LogInformation(
                "Retrieved {Count} visits for institution {InstitucionId}",
                visitasDto.Count,
                institucionId);

            return ApiResponse<IEnumerable<VisitaDto>>.Success(visitasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving visits for institution {InstitucionId}",
                institucionId);
            return ApiResponse<IEnumerable<VisitaDto>>.Failure(
                "Error retrieving visits",
                "An error occurred while retrieving the visits");
        }
    }

    public async Task<ApiResponse<VisitaDto>> GetByIdAsync(
        int id,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var visita = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.Usuario)
                // .Include(v => v.Habitacion) // Ignored in EF configuration
                .Include(v => v.Reservas)
                .FirstOrDefaultAsync(v => 
                    v.VisitaId == id && 
                    v.InstitucionID == institucionId,
                    cancellationToken);

            if (visita == null)
            {
                return ApiResponse<VisitaDto>.Failure("Visit not found");
            }

            var visitaDto = MapToDto(visita);

            _logger.LogInformation(
                "Retrieved visit {VisitaId} for institution {InstitucionId}",
                id,
                institucionId);

            return ApiResponse<VisitaDto>.Success(visitaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving visit {VisitaId} for institution {InstitucionId}",
                id,
                institucionId);
            return ApiResponse<VisitaDto>.Failure(
                "Error retrieving visit",
                "An error occurred while retrieving the visit");
        }
    }

    public async Task<ApiResponse<Visitas>> CreateVisitaAsync(
        VisitaCreateDto createDto,
        int institucionId,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var visita = new Visitas
            {
                InstitucionID = institucionId,
                UserId = userId,
                PatenteVehiculo = createDto.PatenteVehiculo?.Trim(),
                NumeroTelefono = createDto.NumeroTelefono?.Trim(),
                Identificador = createDto.Identificador?.Trim(),
                FechaRegistro = DateTime.Now,
                FechaPrimerIngreso = createDto.EsReserva ? null : DateTime.Now,
                Anulado = false,
                HabitacionId = createDto.HabitacionId,
            };

            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync(cancellationToken);

            // Log audit
            await _registrosService.LogAuditAsync(
                $"Nueva Visita - Identificador: {visita.Identificador ?? "Sin identificación"}",
                ModuloSistema.RESERVAS,
                institucionId,
                userId,
                null,
                System.Text.Json.JsonSerializer.Serialize(new 
                {
                    visita.VisitaId,
                    visita.Identificador,
                    visita.HabitacionId
                }),
                visita.VisitaId,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Created visit {VisitaId} for institution {InstitucionId}",
                visita.VisitaId,
                institucionId);

            return ApiResponse<Visitas>.Success(visita, "Visit created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating visit for institution {InstitucionId}",
                institucionId);
            return ApiResponse<Visitas>.Failure(
                "Error creating visit",
                "An error occurred while creating the visit");
        }
    }

    public async Task<ApiResponse<VisitaDto>> UpdateAsync(
        int id,
        VisitaUpdateDto updateDto,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var visita = await _context.Visitas
                .FirstOrDefaultAsync(v => 
                    v.VisitaId == id && 
                    v.InstitucionID == institucionId,
                    cancellationToken);

            if (visita == null)
            {
                return ApiResponse<VisitaDto>.Failure("Visit not found");
            }

            if (!string.IsNullOrEmpty(updateDto.PatenteVehiculo))
                visita.PatenteVehiculo = updateDto.PatenteVehiculo.Trim();
            
            if (!string.IsNullOrEmpty(updateDto.Identificador))
                visita.Identificador = updateDto.Identificador.Trim();
            
            if (!string.IsNullOrEmpty(updateDto.NumeroTelefono))
                visita.NumeroTelefono = updateDto.NumeroTelefono.Trim();

            _context.Visitas.Update(visita);
            await _context.SaveChangesAsync(cancellationToken);

            // Retrieve with includes for response
            var updatedVisita = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.Usuario)
                // .Include(v => v.Habitacion) // Ignored in EF configuration
                .Include(v => v.Reservas)
                .FirstOrDefaultAsync(v => v.VisitaId == id, cancellationToken);

            var visitaDto = MapToDto(updatedVisita!);

            _logger.LogInformation(
                "Updated visit {VisitaId} for institution {InstitucionId}",
                id,
                institucionId);

            return ApiResponse<VisitaDto>.Success(visitaDto, "Visit updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating visit {VisitaId} for institution {InstitucionId}",
                id,
                institucionId);
            return ApiResponse<VisitaDto>.Failure(
                "Error updating visit",
                "An error occurred while updating the visit");
        }
    }

    public async Task<ApiResponse> AnularVisitaAsync(
        int id,
        int institucionId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var visita = await _context.Visitas
                .Include(v => v.Reservas)
                .Include(v => v.Movimientos)
                .FirstOrDefaultAsync(v => 
                    v.VisitaId == id && 
                    v.InstitucionID == institucionId,
                    cancellationToken);

            if (visita == null)
            {
                return ApiResponse.Failure("Visit not found");
            }

            if (visita.Anulado)
            {
                return ApiResponse.Failure("Visit is already cancelled");
            }

            // Check for active reservations
            var hasActiveReservations = visita.Reservas.Any(r => r.FechaAnula == null);
            if (hasActiveReservations)
            {
                return ApiResponse.Failure(
                    "Cannot cancel visit with active reservations",
                    "Please cancel all reservations before cancelling the visit");
            }

            visita.Anulado = true;

            // Also cancel all related movements
            foreach (var movimiento in visita.Movimientos)
            {
                movimiento.Anulado = true;
            }

            _context.Visitas.Update(visita);
            await _context.SaveChangesAsync(cancellationToken);

            // Log audit
            await _registrosService.LogAuditAsync(
                $"Visita Anulada - ID: {visita.VisitaId} - Motivo: {reason ?? "No especificado"}",
                ModuloSistema.RESERVAS,
                institucionId,
                visita.UserId,
                null,
                System.Text.Json.JsonSerializer.Serialize(new 
                {
                    VisitaId = visita.VisitaId,
                    Motivo = reason
                }),
                visita.VisitaId,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Cancelled visit {VisitaId} for institution {InstitucionId}",
                id,
                institucionId);

            return ApiResponse.Success("Visit cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error cancelling visit {VisitaId} for institution {InstitucionId}",
                id,
                institucionId);
            return ApiResponse.Failure(
                "Error cancelling visit",
                "An error occurred while cancelling the visit");
        }
    }

    public async Task<ApiResponse<IEnumerable<VisitaDto>>> GetActiveVisitasAsync(
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var visitas = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.Usuario)
                // .Include(v => v.Habitacion) // Ignored in EF configuration
                .Include(v => v.Reservas)
                .Where(v => 
                    v.InstitucionID == institucionId &&
                    !v.Anulado &&
                    v.Reservas.Any(r => r.FechaFin == null))
                .OrderByDescending(v => v.FechaRegistro)
                .ToListAsync(cancellationToken);

            var visitasDto = visitas.Select(v => MapToDto(v)).ToList();

            _logger.LogInformation(
                "Retrieved {Count} active visits for institution {InstitucionId}",
                visitasDto.Count,
                institucionId);

            return ApiResponse<IEnumerable<VisitaDto>>.Success(visitasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving active visits for institution {InstitucionId}",
                institucionId);
            return ApiResponse<IEnumerable<VisitaDto>>.Failure(
                "Error retrieving active visits",
                "An error occurred while retrieving active visits");
        }
    }

    public async Task<ApiResponse<VisitaDto>> GetByHabitacionAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var visita = await _context.Visitas
                .AsNoTracking()
                .Include(v => v.Usuario)
                // .Include(v => v.Habitacion) // Ignored in EF configuration
                .Include(v => v.Reservas)
                .FirstOrDefaultAsync(v => 
                    v.HabitacionId == habitacionId && 
                    v.InstitucionID == institucionId &&
                    !v.Anulado &&
                    v.Reservas.Any(r => r.FechaFin == null),
                    cancellationToken);

            if (visita == null)
            {
                return ApiResponse<VisitaDto>.Failure("No active visit found for this room");
            }

            var visitaDto = MapToDto(visita);

            _logger.LogInformation(
                "Retrieved visit for room {HabitacionId} in institution {InstitucionId}",
                habitacionId,
                institucionId);

            return ApiResponse<VisitaDto>.Success(visitaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving visit for room {HabitacionId} in institution {InstitucionId}",
                habitacionId,
                institucionId);
            return ApiResponse<VisitaDto>.Failure(
                "Error retrieving visit",
                "An error occurred while retrieving the visit");
        }
    }

    private VisitaDto MapToDto(Visitas visita)
    {
        var reservaActiva = visita.Reservas?.FirstOrDefault(r => r.FechaFin == null);
        
        return new VisitaDto
        {
            VisitaId = visita.VisitaId,
            PatenteVehiculo = visita.PatenteVehiculo,
            Identificador = visita.Identificador,
            NumeroTelefono = visita.NumeroTelefono,
            FechaPrimerIngreso = visita.FechaPrimerIngreso,
            UserId = visita.UserId,
            UserName = visita.Usuario?.UserName,
            FechaRegistro = visita.FechaRegistro,
            Anulado = visita.Anulado,
            InstitucionID = visita.InstitucionID,
            HabitacionId = visita.HabitacionId,
            HabitacionNombre = null, // TODO: Load from separate query if needed
            TieneReservaActiva = reservaActiva != null,
            ReservaActivaId = reservaActiva?.ReservaId
        };
    }
}