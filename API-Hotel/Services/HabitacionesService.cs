using hotel.Data;
using hotel.DTOs;
using hotel.DTOs.Caracteristicas;
using hotel.DTOs.Common;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service for managing room (Habitaciones) operations
/// Implements single responsibility principle and dependency injection
/// </summary>
public class HabitacionesService(HotelDbContext context, ILogger<HabitacionesService> logger)
    : IHabitacionesService
{
    private readonly HotelDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<HabitacionesService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ApiResponse<IEnumerable<HabitacionDto>>> GetHabitacionesAsync(
        int institucionId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Getting habitaciones for institution {InstitucionId}, includeInactive: {IncludeInactive}",
                institucionId,
                includeInactive
            );

            var query = _context
                .Habitaciones.Include(h => h.Categoria)
                .Include(h => h.HabitacionCaracteristicas)
                .ThenInclude(hc => hc.Caracteristica)
                .Include(h => h.HabitacionImagenes)
                .ThenInclude(hi => hi.Imagen)
                .Where(h => h.InstitucionID == institucionId)
                .AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(h => h.Anulado != true);
            }

            var habitaciones = await query
                .OrderBy(h => h.NombreHabitacion)
                .ToListAsync(cancellationToken);

            var habitacionDtos = habitaciones.Select(h => MapToDto(h)).ToList();

            _logger.LogInformation(
                "Retrieved {Count} habitaciones for institution {InstitucionId}",
                habitacionDtos.Count,
                institucionId
            );

            return ApiResponse<IEnumerable<HabitacionDto>>.Success(habitacionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting habitaciones for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<HabitacionDto>>.Failure("Error retrieving rooms");
        }
    }

    public async Task<ApiResponse<HabitacionDto>> GetHabitacionByIdAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var habitacion = await _context
                .Habitaciones.Include(h => h.Categoria)
                .Include(h => h.HabitacionCaracteristicas)
                .ThenInclude(hc => hc.Caracteristica)
                .Include(h => h.HabitacionImagenes)
                .ThenInclude(hi => hi.Imagen)
                .FirstOrDefaultAsync(
                    h => h.HabitacionId == habitacionId && h.InstitucionID == institucionId,
                    cancellationToken
                );

            if (habitacion == null)
            {
                return ApiResponse<HabitacionDto>.Failure("Room not found");
            }

            var habitacionDto = MapToDto(habitacion);
            return ApiResponse<HabitacionDto>.Success(habitacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting habitacion {HabitacionId} for institution {InstitucionId}",
                habitacionId,
                institucionId
            );
            return ApiResponse<HabitacionDto>.Failure("Error retrieving room");
        }
    }

    public async Task<ApiResponse<IEnumerable<HabitacionDto>>> GetAvailableHabitacionesAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var habitaciones = await _context
                .Habitaciones.Include(h => h.Categoria)
                .Include(h => h.HabitacionCaracteristicas)
                .ThenInclude(hc => hc.Caracteristica)
                .Include(h => h.HabitacionImagenes)
                .ThenInclude(hi => hi.Imagen)
                .Where(h =>
                    h.InstitucionID == institucionId && h.Disponible == true && h.Anulado != true
                )
                .OrderBy(h => h.NombreHabitacion)
                .ToListAsync(cancellationToken);

            var habitacionDtos = habitaciones.Select(h => MapToDto(h)).ToList();
            return ApiResponse<IEnumerable<HabitacionDto>>.Success(habitacionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting available habitaciones for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<HabitacionDto>>.Failure(
                "Error retrieving available rooms"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<HabitacionDto>>> GetOccupiedHabitacionesAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var habitaciones = await _context
                .Habitaciones.Include(h => h.Categoria)
                .Include(h => h.HabitacionCaracteristicas)
                .ThenInclude(hc => hc.Caracteristica)
                .Include(h => h.HabitacionImagenes)
                .ThenInclude(hi => hi.Imagen)
                .Where(h =>
                    h.InstitucionID == institucionId && h.Disponible == false && h.Anulado != true
                )
                .OrderBy(h => h.NombreHabitacion)
                .ToListAsync(cancellationToken);

            var habitacionDtos = habitaciones.Select(h => MapToDto(h)).ToList();
            return ApiResponse<IEnumerable<HabitacionDto>>.Success(habitacionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting occupied habitaciones for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<HabitacionDto>>.Failure(
                "Error retrieving occupied rooms"
            );
        }
    }

    public async Task<ApiResponse<HabitacionDto>> CreateHabitacionAsync(
        HabitacionCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Validate category exists
            var categoria = await _context.CategoriasHabitaciones.FirstOrDefaultAsync(
                c => c.CategoriaId == createDto.CategoriaHabitacionId,
                cancellationToken
            );

            if (categoria == null)
            {
                return ApiResponse<HabitacionDto>.Failure("Room category not found");
            }

            // Check for duplicate room name
            var existingRoom = await _context.Habitaciones.FirstOrDefaultAsync(
                h =>
                    h.NombreHabitacion == createDto.NombreHabitacion
                    && h.InstitucionID == institucionId,
                cancellationToken
            );

            if (existingRoom != null)
            {
                return ApiResponse<HabitacionDto>.Failure("A room with this name already exists");
            }

            var habitacion = new Habitaciones
            {
                NombreHabitacion = createDto.NombreHabitacion,
                CategoriaId = createDto.CategoriaHabitacionId,
                InstitucionID = institucionId,
                Disponible = true,
                Anulado = false,
                FechaRegistro = DateTime.UtcNow,
            };

            _context.Habitaciones.Add(habitacion);
            await _context.SaveChangesAsync(cancellationToken);

            // Add characteristics if provided
            if (createDto.CaracteristicaIds?.Any() == true)
            {
                await AddCharacteristicsToRoom(
                    habitacion.HabitacionId,
                    createDto.CaracteristicaIds,
                    cancellationToken
                );
            }

            // Reload with includes
            var createdHabitacion = await _context
                .Habitaciones.Include(h => h.Categoria)
                .Include(h => h.HabitacionCaracteristicas)
                .ThenInclude(hc => hc.Caracteristica)
                .Include(h => h.HabitacionImagenes)
                .ThenInclude(hi => hi.Imagen)
                .FirstAsync(h => h.HabitacionId == habitacion.HabitacionId, cancellationToken);

            var habitacionDto = MapToDto(createdHabitacion);
            return ApiResponse<HabitacionDto>.Success(habitacionDto, "Room created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating habitacion for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<HabitacionDto>.Failure("Error creating room");
        }
    }

    public async Task<ApiResponse<HabitacionDto>> UpdateHabitacionAsync(
        int habitacionId,
        HabitacionUpdateDto updateDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var habitacion = await _context.Habitaciones.FirstOrDefaultAsync(
                h => h.HabitacionId == habitacionId && h.InstitucionID == institucionId,
                cancellationToken
            );

            if (habitacion == null)
            {
                return ApiResponse<HabitacionDto>.Failure("Room not found");
            }

            // Update properties if provided
            if (!string.IsNullOrEmpty(updateDto.NombreHabitacion))
            {
                habitacion.NombreHabitacion = updateDto.NombreHabitacion;
            }

            if (updateDto.CategoriaHabitacionId.HasValue)
            {
                var categoria = await _context.CategoriasHabitaciones.FirstOrDefaultAsync(
                    c => c.CategoriaId == updateDto.CategoriaHabitacionId.Value,
                    cancellationToken
                );

                if (categoria == null)
                {
                    return ApiResponse<HabitacionDto>.Failure("Room category not found");
                }

                habitacion.CategoriaId = updateDto.CategoriaHabitacionId.Value;
            }

            if (updateDto.Activo.HasValue)
            {
                habitacion.Anulado = !updateDto.Activo.Value;
            }

            // Update characteristics if provided
            if (updateDto.CaracteristicaIds != null)
            {
                await UpdateRoomCharacteristics(
                    habitacionId,
                    updateDto.CaracteristicaIds,
                    cancellationToken
                );
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Reload with includes
            var updatedHabitacion = await _context
                .Habitaciones.Include(h => h.Categoria)
                .Include(h => h.HabitacionCaracteristicas)
                .ThenInclude(hc => hc.Caracteristica)
                .Include(h => h.HabitacionImagenes)
                .ThenInclude(hi => hi.Imagen)
                .FirstAsync(h => h.HabitacionId == habitacionId, cancellationToken);

            var habitacionDto = MapToDto(updatedHabitacion);
            return ApiResponse<HabitacionDto>.Success(habitacionDto, "Room updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating habitacion {HabitacionId} for institution {InstitucionId}",
                habitacionId,
                institucionId
            );
            return ApiResponse<HabitacionDto>.Failure("Error updating room");
        }
    }

    public async Task<ApiResponse> DeleteHabitacionAsync(
        int habitacionId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var habitacion = await _context.Habitaciones.FirstOrDefaultAsync(
                h => h.HabitacionId == habitacionId && h.InstitucionID == institucionId,
                cancellationToken
            );

            if (habitacion == null)
            {
                return ApiResponse.Failure("Room not found");
            }

            // Check if room is currently occupied
            if (habitacion.Disponible == false)
            {
                return ApiResponse.Failure("Cannot delete an occupied room");
            }

            // Soft delete
            habitacion.Anulado = true;

            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse.Success("Room deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error deleting habitacion {HabitacionId} for institution {InstitucionId}",
                habitacionId,
                institucionId
            );
            return ApiResponse.Failure("Error deleting room");
        }
    }

    public async Task<ApiResponse> ChangeAvailabilityAsync(
        int habitacionId,
        bool available,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var habitacion = await _context.Habitaciones.FirstOrDefaultAsync(
                h => h.HabitacionId == habitacionId && h.InstitucionID == institucionId,
                cancellationToken
            );

            if (habitacion == null)
            {
                return ApiResponse.Failure("Room not found");
            }

            habitacion.Disponible = available;

            await _context.SaveChangesAsync(cancellationToken);

            var status = available ? "available" : "occupied";
            return ApiResponse.Success($"Room status changed to {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error changing availability for habitacion {HabitacionId} for institution {InstitucionId}",
                habitacionId,
                institucionId
            );
            return ApiResponse.Failure("Error changing room availability");
        }
    }

    public async Task<ApiResponse<HabitacionStatsDto>> GetHabitacionStatsAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var habitaciones = await _context
                .Habitaciones.Include(h => h.Categoria)
                .Where(h => h.InstitucionID == institucionId)
                .ToListAsync(cancellationToken);

            var activas = habitaciones.Where(h => h.Anulado != true).ToList();
            var total = activas.Count;
            var disponibles = activas.Count(h => h.Disponible == true);
            var ocupadas = activas.Count(h => h.Disponible == false);
            var inactivas = habitaciones.Count(h => h.Anulado == true);

            var ocupacionPorCategoria = activas
                .GroupBy(h => h.Categoria?.NombreCategoria ?? "Sin categoría")
                .ToDictionary(g => g.Key, g => g.Count(h => h.Disponible == false));

            var stats = new HabitacionStatsDto
            {
                TotalHabitaciones = total,
                HabitacionesDisponibles = disponibles,
                HabitacionesOcupadas = ocupadas,
                HabitacionesInactivas = inactivas,
                PorcentajeOcupacion = total > 0 ? (decimal)ocupadas / total * 100 : 0,
                OcupacionPorCategoria = ocupacionPorCategoria,
            };

            return ApiResponse<HabitacionStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting habitacion stats for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<HabitacionStatsDto>.Failure("Error retrieving room statistics");
        }
    }

    #region Private Helper Methods

    private HabitacionDto MapToDto(Habitaciones habitacion)
    {
        return new HabitacionDto
        {
            HabitacionId = habitacion.HabitacionId,
            NombreHabitacion = habitacion.NombreHabitacion ?? "",
            Disponible = habitacion.Disponible ?? false,
            VisitaId = habitacion.VisitaID,
            CategoriaHabitacionId = habitacion.CategoriaId ?? 0,
            NombreCategoria = habitacion.Categoria?.NombreCategoria,
            PrecioCategoria = habitacion.Categoria?.PrecioNormal,
            Activo = habitacion.Anulado != true,
            FechaCreacion = habitacion.FechaRegistro ?? DateTime.MinValue,
            FechaModificacion = null, // No hay campo en el modelo actual
            InstitucionId = habitacion.InstitucionID,
            Estado = habitacion.Disponible == true ? "Disponible" : "Ocupada",
            Observaciones = "", // No hay campo en el modelo actual
            Caracteristicas =
                habitacion
                    .HabitacionCaracteristicas?.Select(hc => new CaracteristicaDto
                    {
                        CaracteristicaId = hc.CaracteristicaId,
                        Nombre = hc.Caracteristica?.Nombre ?? "",
                        Descripcion = hc.Caracteristica?.Descripcion,
                        Icono = hc.Caracteristica?.Icono
                    })
                    .ToList() ?? [],
            Imagenes =
                habitacion
                    .HabitacionImagenes?.Where(hi => hi.Anulado != true)
                    .Select(hi => new ImagenDto
                    {
                        ImagenId = hi.ImagenId,
                        NombreArchivo = hi.Imagen?.NombreArchivo ?? "",
                        UrlCompleta = hi.Imagen?.Origen ?? "",
                    })
                    .ToList() ?? [],
        };
    }

    private async Task AddCharacteristicsToRoom(
        int habitacionId,
        List<int> caracteristicaIds,
        CancellationToken cancellationToken
    )
    {
        var caracteristicas = caracteristicaIds.Select(id => new HabitacionCaracteristica
        {
            HabitacionId = habitacionId,
            CaracteristicaId = id,
        });

        _context.HabitacionCaracteristicas.AddRange(caracteristicas);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateRoomCharacteristics(
        int habitacionId,
        List<int> caracteristicaIds,
        CancellationToken cancellationToken
    )
    {
        // Remove existing characteristics
        var existingCaracteristicas = await _context
            .HabitacionCaracteristicas.Where(hc => hc.HabitacionId == habitacionId)
            .ToListAsync(cancellationToken);

        _context.HabitacionCaracteristicas.RemoveRange(existingCaracteristicas);

        // Add new characteristics
        if (caracteristicaIds.Any())
        {
            await AddCharacteristicsToRoom(habitacionId, caracteristicaIds, cancellationToken);
        }
    }

    public async Task<ApiResponse<IEnumerable<HabitacionCompleteDto>>> GetHabitacionesCompleteAsync(
        int institucionId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Getting complete habitaciones for institution {InstitucionId}, includeInactive: {IncludeInactive}",
                institucionId,
                includeInactive
            );

            // First, get basic habitacion data without complex includes
            var habitacionesBasic = await _context
                .Habitaciones.AsNoTracking()
                .Where(h =>
                    h.InstitucionID == institucionId && (includeInactive || h.Anulado != true)
                )
                .Select(h => new
                {
                    h.HabitacionId,
                    h.NombreHabitacion,
                    h.CategoriaId,
                    h.Disponible,
                    h.ProximaReserva,
                    h.UsuarioId,
                    h.FechaRegistro,
                    h.Anulado,
                    h.VisitaID,
                    Precio = _context
                        .CategoriasHabitaciones.Where(c => c.CategoriaId == h.CategoriaId)
                        .Select(c => c.PrecioNormal)
                        .FirstOrDefault(),
                    PedidosPendientes = _context.Encargos.Any(e =>
                        e.VisitaId == h.VisitaID
                        && (e.Anulado ?? false) == false
                        && (e.Entregado ?? false) == false
                    ),
                })
                .OrderBy(h => h.NombreHabitacion)
                .ToListAsync(cancellationToken);

            var habitacionIds = habitacionesBasic.Select(h => h.HabitacionId).ToList();
            var visitaIds = habitacionesBasic
                .Where(h => h.VisitaID.HasValue)
                .Select(h => h.VisitaID!.Value)
                .ToList();

            // Get visitas data separately
            var visitasData = await _context
                .Visitas.AsNoTracking()
                .Where(v => visitaIds.Contains(v.VisitaId) && v.Anulado != true)
                .Select(v => new
                {
                    v.VisitaId,
                    v.FechaPrimerIngreso,
                    v.Identificador,
                    v.PatenteVehiculo,
                    v.NumeroTelefono,
                    v.FechaRegistro,
                })
                .ToListAsync(cancellationToken);

            // Get reservas data separately
            var reservasData = await _context
                .Reservas.Where(r => visitaIds.Contains(r.VisitaId ?? 0) && r.FechaFin == null)
                .Select(r => new
                {
                    r.VisitaId,
                    r.ReservaId,
                    r.FechaReserva,
                    r.FechaFin,
                    r.TotalHoras,
                    r.TotalMinutos,
                    r.MovimientoId,
                    r.PromocionId,
                    r.PausaHoras,
                    r.PausaMinutos,
                    r.FechaRegistro,
                })
                .ToListAsync(cancellationToken);

            // Get images data separately
            var imagenesData = await _context
                .HabitacionImagenes.Where(hi =>
                    habitacionIds.Contains(hi.HabitacionId) && hi.Anulado != true
                )
                .Select(hi => new { hi.HabitacionId, hi.ImagenId })
                .ToListAsync(cancellationToken);

            // Get caracteristicas data separately
            var caracteristicasData = await _context
                .HabitacionCaracteristicas.Where(hc => habitacionIds.Contains(hc.HabitacionId))
                .Select(hc => new
                {
                    hc.HabitacionId,
                    hc.CaracteristicaId,
                    Nombre = _context
                        .Caracteristicas.Where(c => c.CaracteristicaId == hc.CaracteristicaId)
                        .Select(c => c.Nombre)
                        .FirstOrDefault(),
                    Descripcion = _context
                        .Caracteristicas.Where(c => c.CaracteristicaId == hc.CaracteristicaId)
                        .Select(c => c.Descripcion)
                        .FirstOrDefault(),
                    Icono = _context
                        .Caracteristicas.Where(c => c.CaracteristicaId == hc.CaracteristicaId)
                        .Select(c => c.Icono)
                        .FirstOrDefault(),
                })
                .ToListAsync(cancellationToken);

            // Combine all data into DTOs
            var habitacionDtos = habitacionesBasic
                .Select(h =>
                {
                    var visita = visitasData.FirstOrDefault(v => v.VisitaId == h.VisitaID);
                    var reservaActiva = reservasData.FirstOrDefault(r => r.VisitaId == h.VisitaID);
                    var imagenes = imagenesData
                        .Where(i => i.HabitacionId == h.HabitacionId)
                        .Select(i => i.ImagenId)
                        .ToList();
                    var caracteristicas = caracteristicasData
                        .Where(c => c.HabitacionId == h.HabitacionId)
                        .ToList();

                    return new HabitacionCompleteDto
                    {
                        HabitacionId = h.HabitacionId,
                        NombreHabitacion = h.NombreHabitacion ?? "",
                        CategoriaId = h.CategoriaId,
                        Disponible = h.Disponible,
                        ProximaReserva = h.ProximaReserva,
                        UsuarioId = h.UsuarioId,
                        FechaRegistro = h.FechaRegistro,
                        Anulado = h.Anulado,
                        VisitaID = h.VisitaID,
                        Precio = h.Precio,
                        PedidosPendientes = h.PedidosPendientes,
                        ReservaActiva =
                            reservaActiva != null
                                ? new ReservaActivaDto
                                {
                                    ReservaId = reservaActiva.ReservaId,
                                    FechaInicio = reservaActiva.FechaReserva,
                                    FechaFin = reservaActiva.FechaFin,
                                    MontoTotal = null, // No hay campo MontoTotal en el modelo Reservas
                                    TotalHoras = reservaActiva.TotalHoras,
                                    TotalMinutos = reservaActiva.TotalMinutos,
                                    MovimientoId = reservaActiva.MovimientoId,
                                    PromocionId = reservaActiva.PromocionId,
                                    PausaHoras = reservaActiva.PausaHoras,
                                    PausaMinutos = reservaActiva.PausaMinutos,
                                    FechaRegistro = reservaActiva.FechaRegistro,
                                }
                                : null,
                        Reserva =
                            reservaActiva != null
                                ? new ReservaActivaDto
                                {
                                    ReservaId = reservaActiva.ReservaId,
                                    FechaInicio = reservaActiva.FechaReserva,
                                    FechaFin = reservaActiva.FechaFin,
                                    MontoTotal = null, // No hay campo MontoTotal en el modelo Reservas
                                    TotalHoras = reservaActiva.TotalHoras,
                                    TotalMinutos = reservaActiva.TotalMinutos,
                                    MovimientoId = reservaActiva.MovimientoId,
                                    PromocionId = reservaActiva.PromocionId,
                                    PausaHoras = reservaActiva.PausaHoras,
                                    PausaMinutos = reservaActiva.PausaMinutos,
                                    FechaRegistro = reservaActiva.FechaRegistro,
                                }
                                : null,
                        Visita =
                            visita != null
                                ? new VisitaBasicDto
                                {
                                    VisitaId = visita.VisitaId,
                                    FechaIngreso = visita.FechaPrimerIngreso,
                                    FechaSalida = null, // No hay campo FechaSalida en el modelo Visitas
                                    ClienteId = null, // No hay campo ClienteId en el modelo Visitas
                                    NombreCompleto = visita.Identificador,
                                    PatenteVehiculo = visita.PatenteVehiculo,
                                    NumeroTelefono = visita.NumeroTelefono,
                                    FechaRegistro = visita.FechaRegistro,
                                }
                                : null,
                        Imagenes = imagenes,
                        Caracteristicas = caracteristicas
                            .Select(c => new CaracteristicaDto
                            {
                                CaracteristicaId = c.CaracteristicaId,
                                Nombre = c.Nombre ?? "",
                                Descripcion = c.Descripcion,
                                Icono = c.Icono
                            })
                            .ToList(),
                    };
                })
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} complete habitaciones for institution {InstitucionId}",
                habitacionDtos.Count,
                institucionId
            );

            return ApiResponse<IEnumerable<HabitacionCompleteDto>>.Success(habitacionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting complete habitaciones for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<HabitacionCompleteDto>>.Failure(
                "Error retrieving complete rooms"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<HabitacionLibreDto>>> GetFreeHabitacionesOptimizedAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Getting optimized free habitaciones for institution {InstitucionId}",
                institucionId
            );

            var freeRooms = await _context
                .Habitaciones.AsNoTracking()
                .Where(h =>
                    h.InstitucionID == institucionId 
                    && h.Disponible == true 
                    && h.Anulado != true
                )
                .Select(h => new HabitacionLibreDto
                {
                    HabitacionId = h.HabitacionId,
                    NombreHabitacion = h.NombreHabitacion ?? "",
                    Disponible = h.Disponible,
                    CategoriaId = h.CategoriaId,
                    Precio = _context
                        .CategoriasHabitaciones.Where(c => c.CategoriaId == h.CategoriaId)
                        .Select(c => c.PrecioNormal)
                        .FirstOrDefault()
                })
                .OrderBy(h => h.NombreHabitacion)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} optimized free habitaciones for institution {InstitucionId}",
                freeRooms.Count,
                institucionId
            );

            return ApiResponse<IEnumerable<HabitacionLibreDto>>.Success(freeRooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting optimized free habitaciones for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<HabitacionLibreDto>>.Failure(
                "Error retrieving free rooms"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<HabitacionOptimizedDto>>> GetOccupiedHabitacionesOptimizedAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Getting optimized occupied habitaciones for institution {InstitucionId}",
                institucionId
            );

            // Get basic occupied room data
            var occupiedRoomsBasic = await _context
                .Habitaciones.AsNoTracking()
                .Where(h =>
                    h.InstitucionID == institucionId 
                    && h.Disponible == false 
                    && h.Anulado != true
                    && h.VisitaID.HasValue
                )
                .Select(h => new
                {
                    h.HabitacionId,
                    h.NombreHabitacion,
                    h.CategoriaId,
                    h.Disponible,
                    h.VisitaID,
                    Precio = _context
                        .CategoriasHabitaciones.Where(c => c.CategoriaId == h.CategoriaId)
                        .Select(c => c.PrecioNormal)
                        .FirstOrDefault(),
                    PedidosPendientes = _context.Encargos.Any(e =>
                        e.VisitaId == h.VisitaID
                        && (e.Anulado ?? false) == false
                        && (e.Entregado ?? false) == false
                    )
                })
                .OrderBy(h => h.NombreHabitacion)
                .ToListAsync(cancellationToken);

            var visitaIds = occupiedRoomsBasic.Select(h => h.VisitaID!.Value).ToList();

            // Get optimized visita data
            var visitasData = await _context
                .Visitas.AsNoTracking()
                .Where(v => visitaIds.Contains(v.VisitaId) && v.Anulado != true)
                .Select(v => new
                {
                    v.VisitaId,
                    v.Identificador,
                    v.PatenteVehiculo,
                    v.NumeroTelefono
                })
                .ToListAsync(cancellationToken);

            // Get optimized reserva data (only essential fields)
            var reservasData = await _context
                .Reservas.AsNoTracking()
                .Where(r => visitaIds.Contains(r.VisitaId ?? 0) && r.FechaFin == null)
                .Select(r => new
                {
                    r.VisitaId,
                    r.ReservaId,
                    r.FechaReserva,
                    r.FechaFin,
                    r.TotalHoras,
                    r.TotalMinutos,
                    r.PausaHoras,
                    r.PausaMinutos,
                    r.PromocionId
                })
                .ToListAsync(cancellationToken);

            // Combine data into optimized DTOs
            var occupiedRooms = occupiedRoomsBasic
                .Select(h =>
                {
                    var visita = visitasData.FirstOrDefault(v => v.VisitaId == h.VisitaID);
                    var reserva = reservasData.FirstOrDefault(r => r.VisitaId == h.VisitaID);

                    return new HabitacionOptimizedDto
                    {
                        HabitacionId = h.HabitacionId,
                        NombreHabitacion = h.NombreHabitacion ?? "",
                        Disponible = h.Disponible,
                        Precio = h.Precio,
                        CategoriaId = h.CategoriaId,
                        PedidosPendientes = h.PedidosPendientes,
                        VisitaID = h.VisitaID,
                        ReservaActiva = reserva != null
                            ? new ReservaOptimizedDto
                            {
                                ReservaId = reserva.ReservaId,
                                FechaInicio = reserva.FechaReserva,
                                FechaFin = reserva.FechaFin,
                                TotalHoras = reserva.TotalHoras,
                                TotalMinutos = reserva.TotalMinutos,
                                PausaHoras = reserva.PausaHoras,
                                PausaMinutos = reserva.PausaMinutos,
                                PromocionId = reserva.PromocionId
                            }
                            : null,
                        Visita = visita != null
                            ? new VisitaOptimizedDto
                            {
                                VisitaId = visita.VisitaId,
                                NombreCompleto = visita.Identificador,
                                NumeroTelefono = visita.NumeroTelefono,
                                PatenteVehiculo = visita.PatenteVehiculo
                            }
                            : null
                    };
                })
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} optimized occupied habitaciones for institution {InstitucionId}",
                occupiedRooms.Count,
                institucionId
            );

            return ApiResponse<IEnumerable<HabitacionOptimizedDto>>.Success(occupiedRooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting optimized occupied habitaciones for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<HabitacionOptimizedDto>>.Failure(
                "Error retrieving occupied rooms"
            );
        }
    }

    public async Task<ApiResponse<HabitacionBulkStatsDto>> GetHabitacionesOptimizedAsync(
        int institucionId,
        HabitacionFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Getting bulk optimized habitaciones for institution {InstitucionId}",
                institucionId
            );

            var result = new HabitacionBulkStatsDto();

            // Get stats in parallel
            var statsTask = GetHabitacionStatsAsync(institucionId, cancellationToken);

            // Conditionally load data based on filter
            Task<ApiResponse<IEnumerable<HabitacionLibreDto>>>? freeRoomsTask = null;
            Task<ApiResponse<IEnumerable<HabitacionOptimizedDto>>>? occupiedRoomsTask = null;

            if (filter.OnlyOccupied != true)
            {
                freeRoomsTask = GetFreeHabitacionesOptimizedAsync(institucionId, cancellationToken);
            }

            if (filter.OnlyAvailable != true)
            {
                occupiedRoomsTask = GetOccupiedHabitacionesOptimizedAsync(institucionId, cancellationToken);
            }

            // Await all tasks
            var statsResult = await statsTask;
            if (statsResult.IsSuccess)
            {
                result.Stats = statsResult.Data!;
            }

            if (freeRoomsTask != null)
            {
                var freeResult = await freeRoomsTask;
                if (freeResult.IsSuccess)
                {
                    result.HabitacionesLibres = freeResult.Data!;
                }
            }

            if (occupiedRoomsTask != null)
            {
                var occupiedResult = await occupiedRoomsTask;
                if (occupiedResult.IsSuccess)
                {
                    result.HabitacionesOcupadas = occupiedResult.Data!;
                }
            }

            return ApiResponse<HabitacionBulkStatsDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting bulk optimized habitaciones for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<HabitacionBulkStatsDto>.Failure(
                "Error retrieving optimized rooms data"
            );
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DTOs.Rooms.OccupiedRoomDto>> GetOccupiedRoomsWithTimingAsync(
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use SAME logic as GetOccupiedHabitacionesOptimizedAsync
            // Get basic occupied room data with exact same criteria
            var occupiedRoomsBasic = await _context
                .Habitaciones.AsNoTracking()
                .Where(h =>
                    h.InstitucionID == institucionId 
                    && h.Disponible == false  // Key criteria: must be marked as unavailable
                    && h.Anulado != true      // Key criteria: not cancelled
                    && h.VisitaID.HasValue    // Key criteria: must have active visit
                )
                .Select(h => new
                {
                    h.HabitacionId,
                    h.VisitaID
                })
                .ToListAsync(cancellationToken);

            var visitaIds = occupiedRoomsBasic.Select(h => h.VisitaID!.Value).ToList();
            
            if (!visitaIds.Any())
            {
                return new List<DTOs.Rooms.OccupiedRoomDto>();
            }

            // Get active visitas (same as optimized method)
            var visitasData = await _context
                .Visitas.AsNoTracking()
                .Where(v => visitaIds.Contains(v.VisitaId) && v.Anulado != true)  // Key criteria: visit not cancelled
                .Select(v => new
                {
                    v.VisitaId,
                    v.FechaRegistro
                })
                .ToListAsync(cancellationToken);

            // Get active reservas (same as optimized method)
            var reservasData = await _context
                .Reservas.AsNoTracking()
                .Where(r => visitaIds.Contains(r.VisitaId ?? 0) && r.FechaFin == null)  // Key criteria: no end date
                .Select(r => new
                {
                    r.VisitaId,
                    r.ReservaId,
                    r.FechaReserva,
                    r.TotalHoras,
                    r.TotalMinutos,
                    r.PromocionId
                })
                .ToListAsync(cancellationToken);

            var occupiedRooms = occupiedRoomsBasic
                .Where(h => visitasData.Any(v => v.VisitaId == h.VisitaID))  // Ensure visit exists and is not cancelled
                .Select(h => new
                {
                    h.HabitacionId,
                    Visita = visitasData.FirstOrDefault(v => v.VisitaId == h.VisitaID),
                    Reserva = reservasData.FirstOrDefault(r => r.VisitaId == h.VisitaID),
                    VisitaId = h.VisitaID
                })
                .ToList();


            // Get promotions if needed
            var promotionIds = occupiedRooms
                .Where(r => r.Reserva?.PromocionId.HasValue == true)
                .Select(r => r.Reserva!.PromocionId!.Value)
                .Distinct()
                .ToList();
            
            var promotions = await _context.Promociones
                .Where(p => promotionIds.Contains(p.PromocionID) && p.Anulado != true)
                .Select(p => new { p.PromocionID, p.CantidadHoras, CantidadMinutos = 0 }) // Promociones doesn't have minutes field
                .ToListAsync(cancellationToken);

            var result = new List<DTOs.Rooms.OccupiedRoomDto>();

            foreach (var room in occupiedRooms)
            {
                if (room.Visita != null)  // Must have valid visit
                {
                    var totalMinutes = 60; // Default 60 minutes
                    DateTime? startTime = null;
                    
                    // Try to get total minutes and start time from reservation or promotion
                    if (room.Reserva?.PromocionId.HasValue == true)
                    {
                        var promotion = promotions.FirstOrDefault(p => p.PromocionID == room.Reserva.PromocionId.Value);
                        if (promotion != null)
                        {
                            totalMinutes = promotion.CantidadHoras * 60 + promotion.CantidadMinutos;
                        }
                        startTime = room.Reserva.FechaReserva;
                    }
                    else if (room.Reserva?.TotalHoras.HasValue == true)
                    {
                        // Use reservation hours and minutes
                        totalMinutes = (room.Reserva.TotalHoras.Value * 60) + (room.Reserva.TotalMinutos ?? 0);
                        startTime = room.Reserva.FechaReserva;
                    }
                    else
                    {
                        // No active reservation, use visit registration time
                        startTime = room.Visita.FechaRegistro;
                    }

                    // Only add if we have a valid start time and visit ID
                    if (startTime.HasValue && room.VisitaId.HasValue)
                    {
                        result.Add(new DTOs.Rooms.OccupiedRoomDto
                        {
                            RoomId = room.HabitacionId,
                            VisitaId = room.VisitaId.Value,
                            ReservationStartTime = startTime,
                            TotalMinutes = totalMinutes,
                            InstitutionId = institucionId
                        });
                    }
                }
            }

            _logger.LogInformation(
                "Retrieved {Count} occupied rooms with timing for institution {InstitucionId}",
                result.Count,
                institucionId
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting occupied rooms with timing for institution {InstitucionId}",
                institucionId
            );
            return new List<DTOs.Rooms.OccupiedRoomDto>();
        }
    }

    #endregion
}
