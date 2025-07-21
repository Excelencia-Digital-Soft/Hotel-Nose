using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Promociones;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service implementation for managing promociones (promotions)
/// </summary>
public class PromocionesService : IPromocionesService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<PromocionesService> _logger;
    private readonly IRegistrosService _registrosService;

    public PromocionesService(
        HotelDbContext context, 
        ILogger<PromocionesService> logger,
        IRegistrosService registrosService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registrosService = registrosService ?? throw new ArgumentNullException(nameof(registrosService));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<IEnumerable<PromocionDto>>> GetPromotionsByCategoryAsync(
        int categoriaId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var promociones = await _context
                .Promociones.AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.CategoriaID == categoriaId && p.Anulado != true)
                .Select(p => new PromocionDto
                {
                    PromocionId = p.PromocionID,
                    Nombre = p.Detalle ?? string.Empty,
                    Descripcion = p.Detalle,
                    Tarifa = p.Tarifa,
                    CategoriaId = p.CategoriaID,
                    CategoriaNombre = p.Categoria!.NombreCategoria ?? string.Empty,
                    FechaInicio = DateTime.MinValue, // No hay campos de fecha en la entidad actual
                    FechaFin = DateTime.MaxValue,
                    Activo = !(p.Anulado ?? false),
                    InstitucionId = p.InstitucionID,
                    CreatedAt = DateTime.MinValue, // No hay campos de auditoría en la entidad actual
                    UpdatedAt = null,
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} promotions for category {CategoriaId}",
                promociones.Count,
                categoriaId
            );

            return ApiResponse<IEnumerable<PromocionDto>>.Success(promociones);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving promotions for category {CategoriaId}",
                categoriaId
            );
            return ApiResponse<IEnumerable<PromocionDto>>.Failure(
                "Error retrieving promotions",
                "An error occurred while retrieving the promotions"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<IEnumerable<PromocionDto>>> GetActivePromotionsAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var promociones = await _context
                .Promociones.AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.InstitucionID == institucionId && p.Anulado != true)
                .Select(p => new PromocionDto
                {
                    PromocionId = p.PromocionID,
                    Nombre = p.Detalle ?? string.Empty,
                    Descripcion = p.Detalle,
                    Tarifa = p.Tarifa,
                    CategoriaId = p.CategoriaID,
                    CategoriaNombre = p.Categoria!.NombreCategoria ?? string.Empty,
                    FechaInicio = DateTime.MinValue,
                    FechaFin = DateTime.MaxValue,
                    Activo = !(p.Anulado ?? false),
                    InstitucionId = p.InstitucionID,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = null,
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} active promotions for institution {InstitucionId}",
                promociones.Count,
                institucionId
            );

            return ApiResponse<IEnumerable<PromocionDto>>.Success(promociones);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving active promotions for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<PromocionDto>>.Failure(
                "Error retrieving promotions",
                "An error occurred while retrieving the active promotions"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PromocionDto>> GetPromotionByIdAsync(
        int promocionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var promocion = await _context
                .Promociones.AsNoTracking()
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.PromocionID == promocionId, cancellationToken);

            if (promocion == null)
            {
                return ApiResponse<PromocionDto>.Failure(
                    "Promotion not found",
                    $"No promotion found with ID {promocionId}"
                );
            }

            var dto = new PromocionDto
            {
                PromocionId = promocion.PromocionID,
                Nombre = promocion.Detalle ?? string.Empty,
                Descripcion = promocion.Detalle,
                Tarifa = promocion.Tarifa,
                CategoriaId = promocion.CategoriaID,
                CategoriaNombre = promocion.Categoria?.NombreCategoria,
                FechaInicio = DateTime.MinValue,
                FechaFin = DateTime.MaxValue,
                Activo = !(promocion.Anulado ?? false),
                InstitucionId = promocion.InstitucionID,
                CreatedAt = DateTime.MinValue,
                UpdatedAt = null,
            };

            return ApiResponse<PromocionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving promotion {PromocionId}", promocionId);
            return ApiResponse<PromocionDto>.Failure(
                "Error retrieving promotion",
                "An error occurred while retrieving the promotion"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PromocionDto>> CreatePromotionAsync(
        PromocionCreateDto createDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Validate category exists
            var categoria = await _context
                .CategoriasHabitaciones.AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.CategoriaId == createDto.CategoriaId,
                    cancellationToken
                );

            if (categoria == null)
            {
                return ApiResponse<PromocionDto>.Failure(
                    "Category not found",
                    $"No category found with ID {createDto.CategoriaId}"
                );
            }

            var promocion = new Promociones
            {
                Detalle = createDto.Nombre,
                Tarifa = createDto.Tarifa,
                CategoriaID = createDto.CategoriaId,
                CantidadHoras = createDto.CantidadHoras,
                Anulado = !createDto.Activo,
                InstitucionID = institucionId,
            };

            _context.Promociones.Add(promocion);
            await _context.SaveChangesAsync(cancellationToken);

            // Reload with category info
            await _context
                .Entry(promocion)
                .Reference(p => p.Categoria)
                .LoadAsync(cancellationToken);

            var dto = new PromocionDto
            {
                PromocionId = promocion.PromocionID,
                Nombre = promocion.Detalle ?? string.Empty,
                Descripcion = promocion.Detalle,
                Tarifa = promocion.Tarifa,
                CategoriaId = promocion.CategoriaID,
                CategoriaNombre = promocion.Categoria?.NombreCategoria,
                FechaInicio = DateTime.MinValue,
                FechaFin = DateTime.MaxValue,
                Activo = !(promocion.Anulado ?? false),
                InstitucionId = promocion.InstitucionID,
                CreatedAt = DateTime.MinValue,
                UpdatedAt = null,
            };

            _logger.LogInformation(
                "Created promotion {PromocionId} for institution {InstitucionId}",
                promocion.PromocionID,
                institucionId
            );

            return ApiResponse<PromocionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating promotion for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<PromocionDto>.Failure(
                "Error creating promotion",
                "An error occurred while creating the promotion"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PromocionDto>> UpdatePromotionAsync(
        int promocionId,
        PromocionUpdateDto updateDto,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var promocion = await _context
                .Promociones.Include(p => p.Categoria)
                .FirstOrDefaultAsync(
                    p => p.PromocionID == promocionId && p.InstitucionID == institucionId,
                    cancellationToken
                );

            if (promocion == null)
            {
                return ApiResponse<PromocionDto>.Failure(
                    "Promotion not found",
                    $"No promotion found with ID {promocionId}"
                );
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateDto.Nombre))
                promocion.Detalle = updateDto.Nombre;

            if (updateDto.Tarifa.HasValue)
                promocion.Tarifa = updateDto.Tarifa.Value;

            if (updateDto.CategoriaId.HasValue)
            {
                // Validate category exists
                var categoria = await _context
                    .CategoriasHabitaciones.AsNoTracking()
                    .FirstOrDefaultAsync(
                        c => c.CategoriaId == updateDto.CategoriaId.Value,
                        cancellationToken
                    );

                if (categoria == null)
                {
                    return ApiResponse<PromocionDto>.Failure(
                        "Category not found",
                        $"No category found with ID {updateDto.CategoriaId.Value}"
                    );
                }

                promocion.CategoriaID = updateDto.CategoriaId.Value;
            }

            if (updateDto.Activo.HasValue)
                promocion.Anulado = !updateDto.Activo.Value;

            await _context.SaveChangesAsync(cancellationToken);

            var dto = new PromocionDto
            {
                PromocionId = promocion.PromocionID,
                Nombre = promocion.Detalle ?? string.Empty,
                Descripcion = promocion.Detalle,
                Tarifa = promocion.Tarifa,
                CategoriaId = promocion.CategoriaID,
                CategoriaNombre = promocion.Categoria?.NombreCategoria,
                FechaInicio = DateTime.MinValue,
                FechaFin = DateTime.MaxValue,
                Activo = !(promocion.Anulado ?? false),
                InstitucionId = promocion.InstitucionID,
                CreatedAt = DateTime.MinValue,
                UpdatedAt = DateTime.Now,
            };

            _logger.LogInformation("Updated promotion {PromocionId}", promocionId);

            return ApiResponse<PromocionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating promotion {PromocionId}", promocionId);
            return ApiResponse<PromocionDto>.Failure(
                "Error updating promotion",
                "An error occurred while updating the promotion"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse> DeletePromotionAsync(
        int promocionId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var promocion = await _context.Promociones.FirstOrDefaultAsync(
                p => p.PromocionID == promocionId && p.InstitucionID == institucionId,
                cancellationToken
            );

            if (promocion == null)
            {
                return ApiResponse.Failure(
                    "Promotion not found",
                    $"No promotion found with ID {promocionId}"
                );
            }

            // Soft delete
            promocion.Anulado = true;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted promotion {PromocionId}", promocionId);

            return ApiResponse.Success("Promotion deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting promotion {PromocionId}", promocionId);
            return ApiResponse.Failure(
                "Error deleting promotion",
                "An error occurred while deleting the promotion"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PromocionValidationDto>> ValidatePromotionAsync(
        int promocionId,
        int reservaId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var promocion = await _context
                .Promociones.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PromocionID == promocionId, cancellationToken);

            if (promocion == null)
            {
                return ApiResponse<PromocionValidationDto>.Success(
                    new PromocionValidationDto
                    {
                        IsValid = false,
                        ErrorMessage = "Promotion not found",
                    }
                );
            }

            var reserva = await _context
                .Reservas.AsNoTracking()
                .Include(r => r.Habitacion)
                .ThenInclude(h => h!.Categoria)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId, cancellationToken);

            if (reserva == null)
            {
                return ApiResponse<PromocionValidationDto>.Success(
                    new PromocionValidationDto
                    {
                        IsValid = false,
                        ErrorMessage = "Reservation not found",
                    }
                );
            }

            var validationResult = new PromocionValidationDto();

            // Check if promotion is active
            if (promocion.Anulado == true)
            {
                validationResult.IsValid = false;
                validationResult.ErrorMessage = "Promotion is not active";
                return ApiResponse<PromocionValidationDto>.Success(validationResult);
            }

            // Check category compatibility
            if (reserva.Habitacion?.Categoria?.CategoriaId != promocion.CategoriaID)
            {
                validationResult.IsValid = false;
                validationResult.ErrorMessage = "Promotion is not valid for this room category";
                return ApiResponse<PromocionValidationDto>.Success(validationResult);
            }

            // Promotion is valid
            validationResult.IsValid = true;
            validationResult.OriginalRate = reserva.Habitacion?.Categoria?.PrecioNormal;
            validationResult.PromotionRate = promocion.Tarifa;
            validationResult.Discount = (validationResult.OriginalRate ?? 0) - promocion.Tarifa;
            validationResult.ValidFrom = DateTime.MinValue; // No date fields in current entity
            validationResult.ValidUntil = DateTime.MaxValue;

            _logger.LogInformation(
                "Validated promotion {PromocionId} for reservation {ReservaId}: {IsValid}",
                promocionId,
                reservaId,
                validationResult.IsValid
            );

            return ApiResponse<PromocionValidationDto>.Success(validationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating promotion {PromocionId} for reservation {ReservaId}",
                promocionId,
                reservaId
            );
            return ApiResponse<PromocionValidationDto>.Failure(
                "Error validating promotion",
                "An error occurred while validating the promotion"
            );
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PromocionValidationResult>> ValidateAndGetPromocionAsync(
        int promocionId,
        int institucionId,
        int? categoriaId = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var promocion = await _context
                .Promociones.AsNoTracking()
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(
                    p => p.PromocionID == promocionId 
                         && p.InstitucionID == institucionId 
                         && p.Anulado != true,
                    cancellationToken
                );

            if (promocion == null)
            {
                var result = new PromocionValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "La promoción no es válida o no pertenece a esta institución."
                };

                _logger.LogWarning(
                    "Promotion {PromocionId} not found or invalid for institution {InstitucionId}",
                    promocionId,
                    institucionId
                );

                return ApiResponse<PromocionValidationResult>.Success(result);
            }

            // Check category compatibility if provided
            bool isCompatibleWithCategory = true;
            if (categoriaId.HasValue && promocion.CategoriaID != categoriaId.Value)
            {
                isCompatibleWithCategory = false;
                _logger.LogWarning(
                    "Promotion {PromocionId} category {PromocionCategoriaId} does not match room category {CategoriaId}",
                    promocionId,
                    promocion.CategoriaID,
                    categoriaId.Value
                );
            }

            var validationResult = new PromocionValidationResult
            {
                IsValid = isCompatibleWithCategory,
                ErrorMessage = isCompatibleWithCategory ? null : "La promoción no es válida para esta categoría de habitación.",
                PromocionId = promocion.PromocionID,
                PromocionNombre = promocion.Detalle,
                PromocionTarifa = promocion.Tarifa,
                CategoriaId = promocion.CategoriaID,
                IsCompatibleWithCategory = isCompatibleWithCategory
            };

            _logger.LogInformation(
                "Validated promotion {PromocionId} for institution {InstitucionId}: IsValid={IsValid}, Compatible={IsCompatible}",
                promocionId,
                institucionId,
                validationResult.IsValid,
                isCompatibleWithCategory
            );

            return ApiResponse<PromocionValidationResult>.Success(validationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating promotion {PromocionId} for institution {InstitucionId}",
                promocionId,
                institucionId
            );

            var errorResult = new PromocionValidationResult
            {
                IsValid = false,
                ErrorMessage = "Error al validar la promoción. Inténtelo de nuevo."
            };

            return ApiResponse<PromocionValidationResult>.Success(errorResult);
        }
    }
}

