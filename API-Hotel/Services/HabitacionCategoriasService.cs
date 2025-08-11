using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.HabitacionCategorias;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services
{
    /// <summary>
    /// Service for managing room categories (CategoriasHabitaciones)
    /// </summary>
    public class HabitacionCategoriasService : IHabitacionCategoriasService
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<HabitacionCategoriasService> _logger;

        public HabitacionCategoriasService(
            HotelDbContext context,
            ILogger<HabitacionCategoriasService> logger
        )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<
            ApiResponse<IEnumerable<HabitacionCategoriaDto>>
        > GetAllByInstitutionAsync(int institucionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var categorias = await _context
                    .CategoriasHabitaciones.AsNoTracking()
                    .Where(c => c.InstitucionID == institucionId && c.Anulado != true)
                    .Select(c => new HabitacionCategoriaDto
                    {
                        CategoriaId = c.CategoriaId,
                        NombreCategoria = c.NombreCategoria ?? string.Empty,
                        CapacidadMaxima = c.CapacidadMaxima,
                        PrecioNormal = c.PrecioNormal,
                        PrecioEspecial = null, // No hay campo PrecioEspecial en el modelo actual
                        InstitucionId = c.InstitucionID,
                        Activo = c.Anulado != true,
                        PorcentajeXPersona = c.PorcentajeXPersona,
                        FechaCreacion = c.FechaRegistro,
                        FechaModificacion = null, // No hay campo de modificación en el modelo actual
                    })
                    .OrderBy(c => c.NombreCategoria)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "Retrieved {Count} room categories for institution {InstitucionId}",
                    categorias.Count,
                    institucionId
                );

                return ApiResponse<IEnumerable<HabitacionCategoriaDto>>.Success(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving room categories for institution {InstitucionId}",
                    institucionId
                );
                return ApiResponse<IEnumerable<HabitacionCategoriaDto>>.Failure(
                    "Error al obtener las categorías de habitaciones",
                    "Ocurrió un error al obtener las categorías de habitaciones"
                );
            }
        }

        public async Task<ApiResponse<HabitacionCategoriaDto>> GetByIdAsync(
            int id,
            int institucionId,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var categoria = await _context
                    .CategoriasHabitaciones.AsNoTracking()
                    .Where(c =>
                        c.CategoriaId == id && c.InstitucionID == institucionId && c.Anulado != true
                    )
                    .Select(c => new HabitacionCategoriaDto
                    {
                        CategoriaId = c.CategoriaId,
                        NombreCategoria = c.NombreCategoria ?? string.Empty,
                        CapacidadMaxima = c.CapacidadMaxima,
                        PrecioNormal = c.PrecioNormal,
                        PrecioEspecial = null,
                        InstitucionId = c.InstitucionID,
                        Activo = c.Anulado != true,
                        PorcentajeXPersona = c.PorcentajeXPersona,
                        FechaCreacion = c.FechaRegistro,
                        FechaModificacion = null,
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (categoria == null)
                {
                    _logger.LogWarning(
                        "Room category with ID {Id} not found for institution {InstitucionId}",
                        id,
                        institucionId
                    );
                    return ApiResponse<HabitacionCategoriaDto>.Failure(
                        "Categoría de habitación no encontrada",
                        $"No se encontró la categoría de habitación con ID {id}"
                    );
                }

                _logger.LogInformation(
                    "Retrieved room category {Id}: {Name} for institution {InstitucionId}",
                    id,
                    categoria.NombreCategoria,
                    institucionId
                );

                return ApiResponse<HabitacionCategoriaDto>.Success(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving room category with ID {Id} for institution {InstitucionId}",
                    id,
                    institucionId
                );
                return ApiResponse<HabitacionCategoriaDto>.Failure(
                    "Error al obtener la categoría de habitación",
                    "Ocurrió un error al obtener la categoría de habitación solicitada"
                );
            }
        }

        public async Task<ApiResponse<HabitacionCategoriaDto>> CreateAsync(
            HabitacionCategoriaCreateDto createDto,
            int institucionId,
            CancellationToken cancellationToken = default
        )
        {
            using var transaction = await _context.Database.BeginTransactionAsync(
                cancellationToken
            );
            try
            {
                // Check if a category with the same name already exists
                var existingCategoria = await _context
                    .CategoriasHabitaciones.AsNoTracking()
                    .AnyAsync(
                        c =>
                            c.InstitucionID == institucionId
                            && c.NombreCategoria != null
                            && c.NombreCategoria.ToLower() == createDto.NombreCategoria.ToLower()
                            && c.Anulado != true,
                        cancellationToken
                    );

                if (existingCategoria)
                {
                    _logger.LogWarning(
                        "Attempted to create room category with duplicate name: {Name} for institution {InstitucionId}",
                        createDto.NombreCategoria,
                        institucionId
                    );
                    return ApiResponse<HabitacionCategoriaDto>.Failure(
                        "Ya existe una categoría con ese nombre",
                        "No se pueden crear categorías con nombres duplicados"
                    );
                }

                var categoria = new CategoriasHabitaciones
                {
                    NombreCategoria = createDto.NombreCategoria.Trim(),
                    PrecioNormal = createDto.PrecioNormal,
                    InstitucionID = institucionId,
                    FechaRegistro = DateTime.UtcNow,
                    Anulado = false,
                };

                _context.CategoriasHabitaciones.Add(categoria);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var result = new HabitacionCategoriaDto
                {
                    CategoriaId = categoria.CategoriaId,
                    NombreCategoria = categoria.NombreCategoria ?? string.Empty,
                    CapacidadMaxima = categoria.CapacidadMaxima,
                    PrecioNormal = categoria.PrecioNormal,
                    PrecioEspecial = null,
                    InstitucionId = categoria.InstitucionID,
                    Activo = categoria.Anulado != true,
                    PorcentajeXPersona = categoria.PorcentajeXPersona,
                    FechaCreacion = categoria.FechaRegistro,
                    FechaModificacion = null,
                };

                _logger.LogInformation(
                    "Created room category {Id}: {Name} for institution {InstitucionId}",
                    categoria.CategoriaId,
                    categoria.NombreCategoria,
                    institucionId
                );

                return ApiResponse<HabitacionCategoriaDto>.Success(
                    result,
                    "Categoría de habitación creada correctamente"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(
                    ex,
                    "Error creating room category {Name} for institution {InstitucionId}",
                    createDto.NombreCategoria,
                    institucionId
                );
                return ApiResponse<HabitacionCategoriaDto>.Failure(
                    "Error al crear la categoría de habitación",
                    "Ocurrió un error al crear la categoría de habitación"
                );
            }
        }

        public async Task<ApiResponse<HabitacionCategoriaDto>> UpdateAsync(
            int id,
            HabitacionCategoriaUpdateDto updateDto,
            int institucionId,
            CancellationToken cancellationToken = default
        )
        {
            using var transaction = await _context.Database.BeginTransactionAsync(
                cancellationToken
            );
            try
            {
                var categoria = await _context.CategoriasHabitaciones.FirstOrDefaultAsync(
                    c =>
                        c.CategoriaId == id
                        && c.InstitucionID == institucionId
                        && c.Anulado != true,
                    cancellationToken
                );

                if (categoria == null)
                {
                    _logger.LogWarning(
                        "Attempted to update non-existent room category with ID {Id} for institution {InstitucionId}",
                        id,
                        institucionId
                    );
                    return ApiResponse<HabitacionCategoriaDto>.Failure(
                        "Categoría de habitación no encontrada",
                        $"No se encontró la categoría de habitación con ID {id}"
                    );
                }

                // Check for duplicate name (excluding current category)
                var duplicateName = await _context
                    .CategoriasHabitaciones.AsNoTracking()
                    .AnyAsync(
                        c =>
                            c.CategoriaId != id
                            && c.InstitucionID == institucionId
                            && c.NombreCategoria != null
                            && c.NombreCategoria.ToLower() == updateDto.NombreCategoria.ToLower()
                            && c.Anulado != true,
                        cancellationToken
                    );

                if (duplicateName)
                {
                    _logger.LogWarning(
                        "Attempted to update room category {Id} with duplicate name: {Name} for institution {InstitucionId}",
                        id,
                        updateDto.NombreCategoria,
                        institucionId
                    );
                    return ApiResponse<HabitacionCategoriaDto>.Failure(
                        "Ya existe una categoría con ese nombre",
                        "No se pueden tener categorías con nombres duplicados"
                    );
                }

                // Update properties
                categoria.NombreCategoria = updateDto.NombreCategoria.Trim();
                categoria.PrecioNormal = updateDto.PrecioNormal;
                categoria.PorcentajeXPersona = updateDto.PorcentajeXPersona;
                categoria.CapacidadMaxima = updateDto.CapacidadMaxima;

                if (updateDto.Activo.HasValue)
                {
                    categoria.Anulado = !updateDto.Activo.Value;
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var result = new HabitacionCategoriaDto
                {
                    CategoriaId = categoria.CategoriaId,
                    NombreCategoria = categoria.NombreCategoria ?? string.Empty,
                    CapacidadMaxima = categoria.CapacidadMaxima,
                    PrecioNormal = categoria.PrecioNormal,
                    PrecioEspecial = null,
                    InstitucionId = categoria.InstitucionID,
                    Activo = categoria.Anulado != true,
                    PorcentajeXPersona = categoria.PorcentajeXPersona,
                    FechaCreacion = categoria.FechaRegistro,
                    FechaModificacion = null,
                };

                _logger.LogInformation(
                    "Updated room category {Id}: {Name} for institution {InstitucionId}",
                    id,
                    categoria.NombreCategoria,
                    institucionId
                );

                return ApiResponse<HabitacionCategoriaDto>.Success(
                    result,
                    "Categoría de habitación actualizada correctamente"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(
                    ex,
                    "Error updating room category with ID {Id} for institution {InstitucionId}",
                    id,
                    institucionId
                );
                return ApiResponse<HabitacionCategoriaDto>.Failure(
                    "Error al actualizar la categoría de habitación",
                    "Ocurrió un error al actualizar la categoría de habitación"
                );
            }
        }

        public async Task<ApiResponse> DeleteAsync(
            int id,
            int institucionId,
            CancellationToken cancellationToken = default
        )
        {
            using var transaction = await _context.Database.BeginTransactionAsync(
                cancellationToken
            );
            try
            {
                var categoria = await _context.CategoriasHabitaciones.FirstOrDefaultAsync(
                    c =>
                        c.CategoriaId == id
                        && c.InstitucionID == institucionId
                        && c.Anulado != true,
                    cancellationToken
                );

                if (categoria == null)
                {
                    _logger.LogWarning(
                        "Attempted to delete non-existent room category with ID {Id} for institution {InstitucionId}",
                        id,
                        institucionId
                    );
                    return ApiResponse.Failure(
                        "Categoría de habitación no encontrada",
                        $"No se encontró la categoría de habitación con ID {id}"
                    );
                }

                // Check if category is being used by any rooms
                var isInUse = await _context
                    .Habitaciones.AsNoTracking()
                    .AnyAsync(h => h.CategoriaId == id && h.Anulado != true, cancellationToken);

                if (isInUse)
                {
                    _logger.LogWarning(
                        "Attempted to delete room category {Id} that is in use by rooms for institution {InstitucionId}",
                        id,
                        institucionId
                    );
                    return ApiResponse.Failure(
                        "No se puede eliminar la categoría",
                        "La categoría está siendo utilizada por una o más habitaciones"
                    );
                }

                // Soft delete
                categoria.Anulado = true;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Deleted room category {Id}: {Name} for institution {InstitucionId}",
                    id,
                    categoria.NombreCategoria,
                    institucionId
                );

                return ApiResponse.Success("Categoría de habitación eliminada correctamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(
                    ex,
                    "Error deleting room category with ID {Id} for institution {InstitucionId}",
                    id,
                    institucionId
                );
                return ApiResponse.Failure(
                    "Error al eliminar la categoría de habitación",
                    "Ocurrió un error al eliminar la categoría de habitación"
                );
            }
        }
    }
}

