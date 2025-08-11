using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Empenos;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services
{
    /// <summary>
    /// Service for managing Empeños (Pawn/Collateral) with improved payment logic
    /// </summary>
    public class EmpenosService : IEmpenosService
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<EmpenosService> _logger;

        public EmpenosService(HotelDbContext context, ILogger<EmpenosService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<IEnumerable<EmpenoDto>>> GetAllUnpaidByInstitutionAsync(
            int institucionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var empenos = await _context.Empeño
                    .AsNoTracking()
                    .Where(e => e.InstitucionID == institucionId && 
                               (e.Anulado == false || e.Anulado == null) && 
                               e.PagoID == null)
                    .Include(e => e.Visita)
                    .Select(e => new EmpenoDto
                    {
                        EmpenoId = e.EmpeñoID,
                        VisitaId = e.VisitaID,
                        Detalle = e.Detalle ?? string.Empty,
                        Monto = e.Monto,
                        PagoId = e.PagoID,
                        FechaRegistro = e.FechaRegistro,
                        Anulado = e.Anulado ?? false,
                        InstitucionId = e.InstitucionID,
                        NombreVisita = e.Visita.Identificador,
                        NumeroHabitacion = null
                    })
                    .OrderBy(e => e.FechaRegistro)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} unpaid empeños for institution {InstitucionId}", 
                    empenos.Count, institucionId);

                return ApiResponse<IEnumerable<EmpenoDto>>.Success(empenos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unpaid empeños for institution {InstitucionId}", institucionId);
                return ApiResponse<IEnumerable<EmpenoDto>>.Failure(
                    "Error al obtener los empeños pendientes", 
                    "Ocurrió un error al recuperar los empeños pendientes");
            }
        }

        public async Task<ApiResponse<IEnumerable<EmpenoDto>>> GetAllByInstitutionAsync(
            int institucionId,
            bool includeAnulados = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _context.Empeño
                    .AsNoTracking()
                    .Where(e => e.InstitucionID == institucionId);

                if (!includeAnulados)
                {
                    query = query.Where(e => e.Anulado == false || e.Anulado == null);
                }

                var empenos = await query
                    .Include(e => e.Visita)
                    .Include(e => e.Pago)
                    .Select(e => new EmpenoDto
                    {
                        EmpenoId = e.EmpeñoID,
                        VisitaId = e.VisitaID,
                        Detalle = e.Detalle ?? string.Empty,
                        Monto = e.Monto,
                        PagoId = e.PagoID,
                        FechaRegistro = e.FechaRegistro,
                        Anulado = e.Anulado ?? false,
                        InstitucionId = e.InstitucionID,
                        NombreVisita = e.Visita.Identificador,
                        NumeroHabitacion = null,
                        FechaPago = e.Pago != null ? e.Pago.fechaHora : null
                    })
                    .OrderByDescending(e => e.FechaRegistro)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} empeños for institution {InstitucionId} (includeAnulados: {IncludeAnulados})", 
                    empenos.Count, institucionId, includeAnulados);

                return ApiResponse<IEnumerable<EmpenoDto>>.Success(empenos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving empeños for institution {InstitucionId}", institucionId);
                return ApiResponse<IEnumerable<EmpenoDto>>.Failure(
                    "Error al obtener los empeños", 
                    "Ocurrió un error al recuperar los empeños");
            }
        }

        public async Task<ApiResponse<EmpenoDto>> GetByIdAsync(
            int id,
            int institucionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var empeno = await _context.Empeño
                    .AsNoTracking()
                    .Where(e => e.EmpeñoID == id && e.InstitucionID == institucionId)
                    .Include(e => e.Visita)
                    .Include(e => e.Pago)
                    .Select(e => new EmpenoDto
                    {
                        EmpenoId = e.EmpeñoID,
                        VisitaId = e.VisitaID,
                        Detalle = e.Detalle ?? string.Empty,
                        Monto = e.Monto,
                        PagoId = e.PagoID,
                        FechaRegistro = e.FechaRegistro,
                        Anulado = e.Anulado ?? false,
                        InstitucionId = e.InstitucionID,
                        NombreVisita = e.Visita.Identificador,
                        NumeroHabitacion = null,
                        FechaPago = e.Pago != null ? e.Pago.fechaHora : null
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (empeno == null)
                {
                    _logger.LogWarning("Empeño {EmpenoId} not found for institution {InstitucionId}", id, institucionId);
                    return ApiResponse<EmpenoDto>.Failure($"No se encontró el empeño con ID: {id}");
                }

                _logger.LogInformation("Retrieved empeño {EmpenoId} for institution {InstitucionId}", id, institucionId);
                return ApiResponse<EmpenoDto>.Success(empeno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving empeño {EmpenoId} for institution {InstitucionId}", id, institucionId);
                return ApiResponse<EmpenoDto>.Failure(
                    "Error al obtener el empeño", 
                    "Ocurrió un error al recuperar el empeño");
            }
        }

        public async Task<ApiResponse<IEnumerable<EmpenoDto>>> GetByVisitaIdAsync(
            int visitaId,
            int institucionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var empenos = await _context.Empeño
                    .AsNoTracking()
                    .Where(e => e.VisitaID == visitaId && e.InstitucionID == institucionId && (e.Anulado == false || e.Anulado == null))
                    .Include(e => e.Visita)
                    .Include(e => e.Pago)
                    .Select(e => new EmpenoDto
                    {
                        EmpenoId = e.EmpeñoID,
                        VisitaId = e.VisitaID,
                        Detalle = e.Detalle ?? string.Empty,
                        Monto = e.Monto,
                        PagoId = e.PagoID,
                        FechaRegistro = e.FechaRegistro,
                        Anulado = e.Anulado ?? false,
                        InstitucionId = e.InstitucionID,
                        NombreVisita = e.Visita.Identificador,
                        NumeroHabitacion = null,
                        FechaPago = e.Pago != null ? e.Pago.fechaHora : null
                    })
                    .OrderBy(e => e.FechaRegistro)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} empeños for visit {VisitaId} in institution {InstitucionId}", 
                    empenos.Count, visitaId, institucionId);

                return ApiResponse<IEnumerable<EmpenoDto>>.Success(empenos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving empeños for visit {VisitaId} in institution {InstitucionId}", visitaId, institucionId);
                return ApiResponse<IEnumerable<EmpenoDto>>.Failure(
                    "Error al obtener los empeños de la visita", 
                    "Ocurrió un error al recuperar los empeños de la visita");
            }
        }

        public async Task<ApiResponse<EmpenoDto>> CreateAsync(
            EmpenoCreateDto createDto,
            int institucionId,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Validate visit exists and belongs to institution
                var visitaExists = await _context.Visitas
                    .AnyAsync(v => v.VisitaId == createDto.VisitaId && v.InstitucionID == institucionId, cancellationToken);

                if (!visitaExists)
                {
                    _logger.LogWarning("Visit {VisitaId} not found for institution {InstitucionId}", createDto.VisitaId, institucionId);
                    return ApiResponse<EmpenoDto>.Failure("La visita especificada no existe o no pertenece a la institución");
                }

                var newEmpeno = new Empeño
                {
                    VisitaID = createDto.VisitaId,
                    Detalle = createDto.Detalle,
                    Monto = createDto.Monto,
                    FechaRegistro = DateTime.Now,
                    InstitucionID = institucionId,
                    Anulado = false,
                    UserId = userId // Track which user created the empeño
                };

                await _context.Empeño.AddAsync(newEmpeno, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Created empeño {EmpenoId} for visit {VisitaId} in institution {InstitucionId} by user {UserId}", 
                    newEmpeno.EmpeñoID, createDto.VisitaId, institucionId, userId);

                // Return the created empeño with related data
                return await GetByIdAsync(newEmpeno.EmpeñoID, institucionId, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error creating empeño for visit {VisitaId} in institution {InstitucionId}", createDto.VisitaId, institucionId);
                return ApiResponse<EmpenoDto>.Failure(
                    "Error al crear el empeño", 
                    "Ocurrió un error al crear el empeño");
            }
        }

        public async Task<ApiResponse<EmpenoDto>> UpdateAsync(
            int id,
            EmpenoUpdateDto updateDto,
            int institucionId,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var empeno = await _context.Empeño
                    .FirstOrDefaultAsync(e => e.EmpeñoID == id && e.InstitucionID == institucionId, cancellationToken);

                if (empeno == null)
                {
                    _logger.LogWarning("Empeño {EmpenoId} not found for institution {InstitucionId}", id, institucionId);
                    return ApiResponse<EmpenoDto>.Failure($"No se encontró el empeño con ID: {id}");
                }

                if (empeno.PagoID.HasValue)
                {
                    _logger.LogWarning("Attempted to update paid empeño {EmpenoId}", id);
                    return ApiResponse<EmpenoDto>.Failure("No se puede modificar un empeño que ya ha sido pagado");
                }

                if (empeno.Anulado == true)
                {
                    _logger.LogWarning("Attempted to update cancelled empeño {EmpenoId}", id);
                    return ApiResponse<EmpenoDto>.Failure("No se puede modificar un empeño que ha sido anulado");
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateDto.Detalle))
                {
                    empeno.Detalle = updateDto.Detalle;
                }

                if (updateDto.Monto.HasValue)
                {
                    empeno.Monto = updateDto.Monto.Value;
                }

                // Update modification timestamp
                empeno.FechaRegistro = DateTime.Now;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Updated empeño {EmpenoId} in institution {InstitucionId}", id, institucionId);

                return await GetByIdAsync(id, institucionId, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error updating empeño {EmpenoId} in institution {InstitucionId}", id, institucionId);
                return ApiResponse<EmpenoDto>.Failure(
                    "Error al actualizar el empeño", 
                    "Ocurrió un error al actualizar el empeño");
            }
        }

        public async Task<ApiResponse<EmpenoDto>> PayEmpenoAsync(
            int id,
            EmpernoPagoDto pagoDto,
            int institucionId,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Validate payment amount
                if (!pagoDto.IsValidPayment)
                {
                    return ApiResponse<EmpenoDto>.Failure("El monto total del pago debe ser mayor a 0");
                }

                // Get empeño with related data
                var empeno = await _context.Empeño
                    .Include(e => e.Visita)
                    .FirstOrDefaultAsync(e => e.EmpeñoID == id && e.InstitucionID == institucionId, cancellationToken);

                if (empeno == null)
                {
                    _logger.LogWarning("Empeño {EmpenoId} not found for institution {InstitucionId}", id, institucionId);
                    return ApiResponse<EmpenoDto>.Failure($"No se encontró el empeño con ID: {id}");
                }

                if (empeno.PagoID.HasValue)
                {
                    _logger.LogWarning("Attempted to pay already paid empeño {EmpenoId}", id);
                    return ApiResponse<EmpenoDto>.Failure("Este empeño ya ha sido pagado");
                }

                if (empeno.Anulado == true)
                {
                    _logger.LogWarning("Attempted to pay cancelled empeño {EmpenoId}", id);
                    return ApiResponse<EmpenoDto>.Failure("No se puede pagar un empeño anulado");
                }

                // FIXED LOGIC: Create single payment record (no duplicate Movimiento)
                var nuevoPago = new Pagos
                {
                    MontoDescuento = 0,
                    MontoEfectivo = pagoDto.MontoEfectivo,
                    MontoTarjeta = pagoDto.MontoTarjeta,
                    MedioPagoId = 1, // Default payment method
                    TarjetaId = pagoDto.TarjetaId,
                    fechaHora = DateTime.Now,
                    InstitucionID = institucionId,
                    Observacion = pagoDto.Observacion
                };

                // Add payment
                await _context.Pagos.AddAsync(nuevoPago, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Link payment to empeño
                empeno.PagoID = nuevoPago.PagoId;
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Processed payment for empeño {EmpenoId} with payment {PagoId} (Total: {MontoTotal}) in institution {InstitucionId}", 
                    id, nuevoPago.PagoId, pagoDto.MontoTotal, institucionId);

                return await GetByIdAsync(id, institucionId, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error processing payment for empeño {EmpenoId} in institution {InstitucionId}", id, institucionId);
                return ApiResponse<EmpenoDto>.Failure(
                    "Error al procesar el pago", 
                    "Ocurrió un error al procesar el pago del empeño");
            }
        }

        public async Task<ApiResponse> DeleteAsync(
            int id,
            int institucionId,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var empeno = await _context.Empeño
                    .FirstOrDefaultAsync(e => e.EmpeñoID == id && e.InstitucionID == institucionId, cancellationToken);

                if (empeno == null)
                {
                    _logger.LogWarning("Empeño {EmpenoId} not found for institution {InstitucionId}", id, institucionId);
                    return ApiResponse.Failure($"No se encontró el empeño con ID: {id}");
                }

                if (empeno.PagoID.HasValue)
                {
                    _logger.LogWarning("Attempted to delete paid empeño {EmpenoId}", id);
                    return ApiResponse.Failure("No se puede anular un empeño que ya ha sido pagado");
                }

                // Soft delete
                empeno.Anulado = true;
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Soft deleted empeño {EmpenoId} in institution {InstitucionId}", id, institucionId);

                return ApiResponse.Success("Empeño anulado correctamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error deleting empeño {EmpenoId} in institution {InstitucionId}", id, institucionId);
                return ApiResponse.Failure(
                    "Error al anular el empeño", 
                    "Ocurrió un error al anular el empeño");
            }
        }

        public async Task<ApiResponse<bool>> ValidateVisitaExistsAsync(
            int visitaId,
            int institucionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var exists = await _context.Visitas
                    .AsNoTracking()
                    .AnyAsync(v => v.VisitaId == visitaId && v.InstitucionID == institucionId, cancellationToken);

                return ApiResponse<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating visit {VisitaId} for institution {InstitucionId}", visitaId, institucionId);
                return ApiResponse<bool>.Failure(
                    "Error al validar la visita", 
                    "Ocurrió un error al validar la visita");
            }
        }
    }
}