using hotel.Data;
using hotel.DTOs.Caracteristicas;
using hotel.DTOs.Common;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services
{
    /// <summary>
    /// Service for managing Caracteristicas (room characteristics/features)
    /// </summary>
    public class CaracteristicasService : ICaracteristicasService
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<CaracteristicasService> _logger;
        private readonly IWebHostEnvironment _environment;

        public CaracteristicasService(
            HotelDbContext context,
            ILogger<CaracteristicasService> logger,
            IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<ApiResponse<IEnumerable<CaracteristicaDto>>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var caracteristicas = await _context.Caracteristicas
                    .AsNoTracking()
                    .OrderBy(c => c.Nombre)
                    .Select(c => new CaracteristicaDto
                    {
                        CaracteristicaId = c.CaracteristicaId,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        Icono = c.Icono
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} caracteristicas", caracteristicas.Count);

                return ApiResponse<IEnumerable<CaracteristicaDto>>.Success(caracteristicas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving caracteristicas");
                return ApiResponse<IEnumerable<CaracteristicaDto>>.Failure(
                    "Error al obtener las características",
                    "Ocurrió un error al obtener la lista de características");
            }
        }

        public async Task<ApiResponse<CaracteristicaDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var caracteristica = await _context.Caracteristicas
                    .AsNoTracking()
                    .Where(c => c.CaracteristicaId == id)
                    .Select(c => new CaracteristicaDto
                    {
                        CaracteristicaId = c.CaracteristicaId,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        Icono = c.Icono
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (caracteristica == null)
                {
                    _logger.LogWarning("Caracteristica with ID {Id} not found", id);
                    return ApiResponse<CaracteristicaDto>.Failure(
                        "Característica no encontrada",
                        $"No se encontró la característica con ID {id}");
                }

                _logger.LogInformation("Retrieved caracteristica {Id}: {Name}", id, caracteristica.Nombre);

                return ApiResponse<CaracteristicaDto>.Success(caracteristica);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving caracteristica with ID {Id}", id);
                return ApiResponse<CaracteristicaDto>.Failure(
                    "Error al obtener la característica",
                    "Ocurrió un error al obtener la característica solicitada");
            }
        }

        public async Task<ApiResponse<CaracteristicaDto>> CreateAsync(
            CaracteristicaCreateDto createDto,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Check if a caracteristica with the same name already exists
                var existingCaracteristica = await _context.Caracteristicas
                    .AsNoTracking()
                    .AnyAsync(c => c.Nombre.ToLower() == createDto.Nombre.ToLower(), cancellationToken);

                if (existingCaracteristica)
                {
                    _logger.LogWarning("Attempted to create caracteristica with duplicate name: {Name}", createDto.Nombre);
                    return ApiResponse<CaracteristicaDto>.Failure(
                        "Ya existe una característica con ese nombre",
                        "No se pueden crear características con nombres duplicados");
                }

                // Handle icon upload
                string? iconPath = null;
                if (createDto.Icono != null && createDto.Icono.Length > 0)
                {
                    var uploadResult = await SaveIconFileAsync(createDto.Icono);
                    if (!uploadResult.Success)
                    {
                        return ApiResponse<CaracteristicaDto>.Failure(uploadResult.ErrorMessage!);
                    }
                    iconPath = uploadResult.FilePath;
                }

                var caracteristica = new Caracteristica
                {
                    Nombre = createDto.Nombre.Trim(),
                    Descripcion = createDto.Descripcion?.Trim(),
                    Icono = iconPath
                };

                _context.Caracteristicas.Add(caracteristica);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var result = new CaracteristicaDto
                {
                    CaracteristicaId = caracteristica.CaracteristicaId,
                    Nombre = caracteristica.Nombre,
                    Descripcion = caracteristica.Descripcion,
                    Icono = caracteristica.Icono
                };

                _logger.LogInformation("Created caracteristica {Id}: {Name}", caracteristica.CaracteristicaId, caracteristica.Nombre);

                return ApiResponse<CaracteristicaDto>.Success(result, "Característica creada correctamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error creating caracteristica {Name}", createDto.Nombre);
                return ApiResponse<CaracteristicaDto>.Failure(
                    "Error al crear la característica",
                    "Ocurrió un error al crear la característica");
            }
        }

        public async Task<ApiResponse<CaracteristicaDto>> UpdateAsync(
            int id,
            CaracteristicaUpdateDto updateDto,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var caracteristica = await _context.Caracteristicas.FindAsync(new object[] { id }, cancellationToken);

                if (caracteristica == null)
                {
                    _logger.LogWarning("Attempted to update non-existent caracteristica with ID {Id}", id);
                    return ApiResponse<CaracteristicaDto>.Failure(
                        "Característica no encontrada",
                        $"No se encontró la característica con ID {id}");
                }

                // Check for duplicate name (excluding current caracteristica)
                var duplicateName = await _context.Caracteristicas
                    .AsNoTracking()
                    .AnyAsync(c => c.CaracteristicaId != id && c.Nombre.ToLower() == updateDto.Nombre.ToLower(), cancellationToken);

                if (duplicateName)
                {
                    _logger.LogWarning("Attempted to update caracteristica {Id} with duplicate name: {Name}", id, updateDto.Nombre);
                    return ApiResponse<CaracteristicaDto>.Failure(
                        "Ya existe una característica con ese nombre",
                        "No se pueden tener características con nombres duplicados");
                }

                // Update basic properties
                caracteristica.Nombre = updateDto.Nombre.Trim();
                caracteristica.Descripcion = updateDto.Descripcion?.Trim();

                // Handle icon update
                if (updateDto.RemoveIcon)
                {
                    // Remove existing icon
                    if (!string.IsNullOrEmpty(caracteristica.Icono))
                    {
                        DeleteIconFile(caracteristica.Icono);
                        caracteristica.Icono = null;
                    }
                }
                else if (updateDto.Icono != null && updateDto.Icono.Length > 0)
                {
                    // Delete old icon if exists
                    if (!string.IsNullOrEmpty(caracteristica.Icono))
                    {
                        DeleteIconFile(caracteristica.Icono);
                    }

                    // Save new icon
                    var uploadResult = await SaveIconFileAsync(updateDto.Icono);
                    if (!uploadResult.Success)
                    {
                        return ApiResponse<CaracteristicaDto>.Failure(uploadResult.ErrorMessage!);
                    }
                    caracteristica.Icono = uploadResult.FilePath;
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var result = new CaracteristicaDto
                {
                    CaracteristicaId = caracteristica.CaracteristicaId,
                    Nombre = caracteristica.Nombre,
                    Descripcion = caracteristica.Descripcion,
                    Icono = caracteristica.Icono
                };

                _logger.LogInformation("Updated caracteristica {Id}: {Name}", id, caracteristica.Nombre);

                return ApiResponse<CaracteristicaDto>.Success(result, "Característica actualizada correctamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error updating caracteristica with ID {Id}", id);
                return ApiResponse<CaracteristicaDto>.Failure(
                    "Error al actualizar la característica",
                    "Ocurrió un error al actualizar la característica");
            }
        }

        public async Task<ApiResponse> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var caracteristica = await _context.Caracteristicas.FindAsync(new object[] { id }, cancellationToken);

                if (caracteristica == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent caracteristica with ID {Id}", id);
                    return ApiResponse.Failure(
                        "Característica no encontrada",
                        $"No se encontró la característica con ID {id}");
                }

                // Check if caracteristica is being used by any room
                var isInUse = await _context.HabitacionCaracteristicas
                    .AsNoTracking()
                    .AnyAsync(hc => hc.CaracteristicaId == id, cancellationToken);

                if (isInUse)
                {
                    _logger.LogWarning("Attempted to delete caracteristica {Id} that is in use by rooms", id);
                    return ApiResponse.Failure(
                        "No se puede eliminar la característica",
                        "La característica está siendo utilizada por una o más habitaciones");
                }

                // Delete icon file if exists
                if (!string.IsNullOrEmpty(caracteristica.Icono))
                {
                    DeleteIconFile(caracteristica.Icono);
                }

                _context.Caracteristicas.Remove(caracteristica);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Deleted caracteristica {Id}: {Name}", id, caracteristica.Nombre);

                return ApiResponse.Success("Característica eliminada correctamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error deleting caracteristica with ID {Id}", id);
                return ApiResponse.Failure(
                    "Error al eliminar la característica",
                    "Ocurrió un error al eliminar la característica");
            }
        }

        public async Task<ApiResponse> AssignToRoomAsync(
            int habitacionId,
            IEnumerable<int> caracteristicaIds,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Validate room exists
                var roomExists = await _context.Habitaciones
                    .AsNoTracking()
                    .AnyAsync(h => h.HabitacionId == habitacionId, cancellationToken);

                if (!roomExists)
                {
                    _logger.LogWarning("Attempted to assign caracteristicas to non-existent room {RoomId}", habitacionId);
                    return ApiResponse.Failure(
                        "Habitación no encontrada",
                        $"No se encontró la habitación con ID {habitacionId}");
                }

                var caracteristicaIdList = caracteristicaIds?.Distinct().ToList() ?? new List<int>();

                // Validate caracteristicas exist (if any provided)
                if (caracteristicaIdList.Any())
                {
                    var existingCaracteristicas = await _context.Caracteristicas
                        .AsNoTracking()
                        .Where(c => caracteristicaIdList.Contains(c.CaracteristicaId))
                        .CountAsync(cancellationToken);

                    if (existingCaracteristicas != caracteristicaIdList.Count)
                    {
                        _logger.LogWarning("Some caracteristicas do not exist when assigning to room {RoomId}", habitacionId);
                        return ApiResponse.Failure(
                            "Algunas características no existen",
                            "Una o más características seleccionadas no existen en el sistema");
                    }
                }

                // Remove existing assignments
                await _context.HabitacionCaracteristicas
                    .Where(hc => hc.HabitacionId == habitacionId)
                    .ExecuteDeleteAsync(cancellationToken);

                // Add new assignments
                if (caracteristicaIdList.Any())
                {
                    var newAssignments = caracteristicaIdList
                        .Select(cId => new HabitacionCaracteristica
                        {
                            HabitacionId = habitacionId,
                            CaracteristicaId = cId
                        });

                    await _context.HabitacionCaracteristicas.AddRangeAsync(newAssignments, cancellationToken);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var message = caracteristicaIdList.Any()
                    ? $"Se asignaron {caracteristicaIdList.Count} características a la habitación"
                    : "Se removieron todas las características de la habitación";

                _logger.LogInformation("Updated caracteristicas for room {RoomId}: {Count} assigned", 
                    habitacionId, caracteristicaIdList.Count);

                return ApiResponse.Success(message);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error assigning caracteristicas to room {RoomId}", habitacionId);
                return ApiResponse.Failure(
                    "Error al asignar características",
                    "Ocurrió un error al asignar las características a la habitación");
            }
        }

        public async Task<ApiResponse<FileResult>> GetImageAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var caracteristica = await _context.Caracteristicas
                    .AsNoTracking()
                    .Where(c => c.CaracteristicaId == id)
                    .Select(c => new { c.CaracteristicaId, c.Icono, c.Nombre })
                    .FirstOrDefaultAsync(cancellationToken);

                if (caracteristica == null)
                {
                    _logger.LogWarning("Attempted to get image for non-existent caracteristica {Id}", id);
                    return ApiResponse<FileResult>.Failure(
                        "Característica no encontrada",
                        $"No se encontró la característica con ID {id}");
                }

                if (string.IsNullOrEmpty(caracteristica.Icono))
                {
                    _logger.LogWarning("Caracteristica {Id} has no icon", id);
                    return ApiResponse<FileResult>.Failure(
                        "Imagen no encontrada",
                        "La característica no tiene una imagen asociada");
                }

                var fullPath = Path.GetFullPath(caracteristica.Icono);

                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("Icon file not found for caracteristica {Id}: {Path}", id, fullPath);
                    return ApiResponse<FileResult>.Failure(
                        "Archivo de imagen no encontrado",
                        "El archivo de imagen no existe en el sistema");
                }

                var fileBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
                var contentType = GetContentType(fullPath);
                var fileName = Path.GetFileName(fullPath);

                var result = new FileResult
                {
                    FileBytes = fileBytes,
                    ContentType = contentType,
                    FileName = fileName
                };

                _logger.LogInformation("Retrieved image for caracteristica {Id}: {FileName}", id, fileName);

                return ApiResponse<FileResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image for caracteristica {Id}", id);
                return ApiResponse<FileResult>.Failure(
                    "Error al obtener la imagen",
                    "Ocurrió un error al obtener la imagen de la característica");
            }
        }

        #region Private Methods

        private async Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveIconFileAsync(IFormFile iconFile)
        {
            try
            {
                // Validate file
                if (iconFile.Length == 0)
                {
                    return (false, null, "El archivo está vacío");
                }

                if (iconFile.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    return (false, null, "El archivo es demasiado grande (máximo 5MB)");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(iconFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return (false, null, "Tipo de archivo no permitido. Solo se permiten imágenes (JPG, PNG, GIF, WebP)");
                }

                // Generate unique filename
                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "caracteristicas");
                
                // Ensure directory exists
                Directory.CreateDirectory(uploadsPath);
                
                var filePath = Path.Combine(uploadsPath, fileName);
                var relativePath = Path.Combine("wwwroot", "uploads", "caracteristicas", fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await iconFile.CopyToAsync(stream);
                }

                return (true, relativePath, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving icon file");
                return (false, null, "Error al guardar el archivo de imagen");
            }
        }

        private void DeleteIconFile(string iconPath)
        {
            try
            {
                var fullPath = Path.GetFullPath(iconPath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted icon file: {Path}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting icon file: {Path}", iconPath);
                // Don't throw - this is cleanup operation
            }
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        #endregion
    }
}