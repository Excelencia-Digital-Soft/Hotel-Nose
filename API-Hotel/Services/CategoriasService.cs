using hotel.Data;
using hotel.DTOs.Categorias;
using hotel.DTOs.Common;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

public class CategoriasService : ICategoriasService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<CategoriasService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IRegistrosService _registrosService;
    private const string UPLOADS_FOLDER = "uploads";

    public CategoriasService(
        HotelDbContext context,
        ILogger<CategoriasService> logger,
        IWebHostEnvironment environment,
        IRegistrosService registrosService
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _registrosService = registrosService ?? throw new ArgumentNullException(nameof(registrosService));
    }

    public async Task<ApiResponse<IEnumerable<CategoriaDto>>> GetAllAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var categorias = await _context
                .CategoriasArticulos.AsNoTracking()
                .Include(c => c.Imagen)
                .Include(c => c.CreadoPor)
                .Include(c => c.ModificadoPor)
                .Where(c =>
                    c.InstitucionID == institucionId && (c.Anulado == null || c.Anulado == false)
                )
                .OrderByDescending(c => c.FechaCreacion ?? c.FechaRegistro ?? DateTime.MinValue)
                .ToListAsync(cancellationToken);

            var categoriasDto = categorias.Select(c => MapToDto(c)).ToList();

            _logger.LogInformation(
                "Retrieved {Count} categories for institution {InstitucionId}",
                categoriasDto.Count,
                institucionId
            );

            return ApiResponse<IEnumerable<CategoriaDto>>.Success(categoriasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving categories for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<CategoriaDto>>.Failure(
                "Error retrieving categories",
                "An error occurred while retrieving the categories"
            );
        }
    }

    public async Task<ApiResponse<CategoriaDto>> GetByIdAsync(
        int id,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var categoria = await _context
                .CategoriasArticulos.AsNoTracking()
                .Include(c => c.Imagen)
                .Include(c => c.CreadoPor)
                .Include(c => c.ModificadoPor)
                .FirstOrDefaultAsync(
                    c => c.CategoriaId == id && c.InstitucionID == institucionId,
                    cancellationToken
                );

            if (categoria == null)
            {
                return ApiResponse<CategoriaDto>.Failure("Category not found");
            }

            var categoriaDto = MapToDto(categoria);

            _logger.LogInformation(
                "Retrieved category {CategoriaId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse<CategoriaDto>.Success(categoriaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving category {CategoriaId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse<CategoriaDto>.Failure(
                "Error retrieving category",
                "An error occurred while retrieving the category"
            );
        }
    }

    public async Task<ApiResponse<CategoriaDto>> CreateAsync(
        CategoriaCreateDto createDto,
        int institucionId,
        string? creadoPorId = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Check if category name already exists (case-insensitive)
            var existsWithSameName = await _context.CategoriasArticulos.AnyAsync(
                c =>
                    c.NombreCategoria.ToLower() == createDto.NombreCategoria.ToLower()
                    && c.InstitucionID == institucionId
                    && (c.Anulado == null || c.Anulado == false),
                cancellationToken
            );

            if (existsWithSameName)
            {
                return ApiResponse<CategoriaDto>.Failure(
                    "A category with this name already exists"
                );
            }

            var categoria = new CategoriasArticulos
            {
                NombreCategoria = createDto.NombreCategoria.Trim(),
                InstitucionID = institucionId,
                Anulado = false,
                FechaRegistro = DateTime.Now,
                FechaCreacion = DateTime.Now,
                CreadoPorId = creadoPorId,
            };

            _context.CategoriasArticulos.Add(categoria);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Retrieve with includes for response
            var createdCategoria = await _context
                .CategoriasArticulos.AsNoTracking()
                .Include(c => c.Imagen)
                .Include(c => c.CreadoPor)
                .Include(c => c.ModificadoPor)
                .FirstOrDefaultAsync(
                    c => c.CategoriaId == categoria.CategoriaId,
                    cancellationToken
                );

            var categoriaDto = MapToDto(createdCategoria!);

            _logger.LogInformation(
                "Created category {CategoriaId} for institution {InstitucionId}",
                categoria.CategoriaId,
                institucionId
            );

            return ApiResponse<CategoriaDto>.Success(categoriaDto, "Category created successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error creating category for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<CategoriaDto>.Failure(
                "Error creating category",
                "An error occurred while creating the category"
            );
        }
    }

    public async Task<ApiResponse<CategoriaDto>> CreateWithImageAsync(
        CategoriaCreateWithImageDto createDto,
        int institucionId,
        string? creadoPorId = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Check if category name already exists (case-insensitive)
            var existsWithSameName = await _context.CategoriasArticulos.AnyAsync(
                c =>
                    c.NombreCategoria.ToLower() == createDto.NombreCategoria.ToLower()
                    && c.InstitucionID == institucionId
                    && (c.Anulado == null || c.Anulado == false),
                cancellationToken
            );

            if (existsWithSameName)
            {
                return ApiResponse<CategoriaDto>.Failure(
                    "A category with this name already exists"
                );
            }

            // Create category
            var categoria = new CategoriasArticulos
            {
                NombreCategoria = createDto.NombreCategoria.Trim(),
                InstitucionID = institucionId,
                Anulado = false,
                FechaRegistro = DateTime.Now,
                FechaCreacion = DateTime.Now,
                CreadoPorId = creadoPorId,
            };

            _context.CategoriasArticulos.Add(categoria);
            await _context.SaveChangesAsync(cancellationToken);

            // Handle image if provided
            if (createDto.Imagen != null && createDto.Imagen.Length > 0)
            {
                var imageResult = await SaveImageAsync(
                    createDto.Imagen,
                    institucionId,
                    cancellationToken
                );
                if (imageResult.IsSuccess)
                {
                    categoria.imagenID = imageResult.Data!.ImagenId;
                    _context.CategoriasArticulos.Update(categoria);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            await transaction.CommitAsync(cancellationToken);

            // Retrieve with includes for response
            var createdCategoria = await _context
                .CategoriasArticulos.AsNoTracking()
                .Include(c => c.Imagen)
                .Include(c => c.CreadoPor)
                .Include(c => c.ModificadoPor)
                .FirstOrDefaultAsync(
                    c => c.CategoriaId == categoria.CategoriaId,
                    cancellationToken
                );

            var categoriaDto = MapToDto(createdCategoria!);

            _logger.LogInformation(
                "Created category with image {CategoriaId} for institution {InstitucionId}",
                categoria.CategoriaId,
                institucionId
            );

            return ApiResponse<CategoriaDto>.Success(
                categoriaDto,
                "Category created with image successfully"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error creating category with image for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<CategoriaDto>.Failure(
                "Error creating category with image",
                "An error occurred while creating the category with image"
            );
        }
    }

    public async Task<ApiResponse<CategoriaDto>> UpdateAsync(
        int id,
        CategoriaUpdateDto updateDto,
        int institucionId,
        string? modificadoPorId = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var categoria = await _context.CategoriasArticulos.FirstOrDefaultAsync(
                c => c.CategoriaId == id && c.InstitucionID == institucionId,
                cancellationToken
            );

            if (categoria == null)
            {
                return ApiResponse<CategoriaDto>.Failure("Category not found");
            }

            // Check if new name already exists (excluding current category)
            if (!string.IsNullOrEmpty(updateDto.NombreCategoria))
            {
                var existsWithSameName = await _context.CategoriasArticulos.AnyAsync(
                    c =>
                        c.NombreCategoria.ToLower() == updateDto.NombreCategoria.ToLower()
                        && c.InstitucionID == institucionId
                        && c.CategoriaId != id
                        && (c.Anulado == null || c.Anulado == false),
                    cancellationToken
                );

                if (existsWithSameName)
                {
                    return ApiResponse<CategoriaDto>.Failure(
                        "A category with this name already exists"
                    );
                }

                categoria.NombreCategoria = updateDto.NombreCategoria.Trim();
            }

            categoria.FechaModificacion = DateTime.Now;
            categoria.ModificadoPorId = modificadoPorId;

            _context.CategoriasArticulos.Update(categoria);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Retrieve with includes for response
            var updatedCategoria = await _context
                .CategoriasArticulos.AsNoTracking()
                .Include(c => c.Imagen)
                .Include(c => c.CreadoPor)
                .Include(c => c.ModificadoPor)
                .FirstOrDefaultAsync(c => c.CategoriaId == id, cancellationToken);

            var categoriaDto = MapToDto(updatedCategoria!);

            _logger.LogInformation(
                "Updated category {CategoriaId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse<CategoriaDto>.Success(categoriaDto, "Category updated successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error updating category {CategoriaId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse<CategoriaDto>.Failure(
                "Error updating category",
                "An error occurred while updating the category"
            );
        }
    }

    public async Task<ApiResponse<CategoriaDto>> UpdateImageAsync(
        int id,
        IFormFile imagen,
        int institucionId,
        string? modificadoPorId = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var categoria = await _context
                .CategoriasArticulos.Include(c => c.Imagen)
                .FirstOrDefaultAsync(
                    c => c.CategoriaId == id && c.InstitucionID == institucionId,
                    cancellationToken
                );

            if (categoria == null)
            {
                return ApiResponse<CategoriaDto>.Failure("Category not found");
            }

            if (imagen == null || imagen.Length == 0)
            {
                return ApiResponse<CategoriaDto>.Failure("Invalid image file");
            }

            // Delete old image if exists
            if (categoria.imagenID.HasValue && categoria.Imagen != null)
            {
                await DeleteImageFileAsync(categoria.Imagen.NombreArchivo);
                _context.Imagenes.Remove(categoria.Imagen);
            }

            // Save new image
            var imageResult = await SaveImageAsync(imagen, institucionId, cancellationToken);
            if (!imageResult.IsSuccess)
            {
                return ApiResponse<CategoriaDto>.Failure("Error saving image");
            }

            categoria.imagenID = imageResult.Data!.ImagenId;
            categoria.FechaModificacion = DateTime.Now;
            categoria.ModificadoPorId = modificadoPorId;

            _context.CategoriasArticulos.Update(categoria);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Retrieve with includes for response
            var updatedCategoria = await _context
                .CategoriasArticulos.AsNoTracking()
                .Include(c => c.Imagen)
                .Include(c => c.CreadoPor)
                .Include(c => c.ModificadoPor)
                .FirstOrDefaultAsync(c => c.CategoriaId == id, cancellationToken);

            var categoriaDto = MapToDto(updatedCategoria!);

            _logger.LogInformation(
                "Updated image for category {CategoriaId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse<CategoriaDto>.Success(
                categoriaDto,
                "Category image updated successfully"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error updating image for category {CategoriaId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse<CategoriaDto>.Failure(
                "Error updating category image",
                "An error occurred while updating the category image"
            );
        }
    }

    public async Task<ApiResponse> DeleteAsync(
        int id,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var categoria = await _context
                .CategoriasArticulos.Include(c => c.Imagen)
                .FirstOrDefaultAsync(
                    c => c.CategoriaId == id && c.InstitucionID == institucionId,
                    cancellationToken
                );

            if (categoria == null)
            {
                return ApiResponse.Failure("Category not found");
            }

            // Check if category is used by any articles
            var isUsedByArticles = await _context.Articulos.AnyAsync(
                a => a.CategoriaID == id && (a.Anulado == null || a.Anulado == false),
                cancellationToken
            );

            if (isUsedByArticles)
            {
                return ApiResponse.Failure("Cannot delete category as it has associated articles");
            }

            // Delete image file if exists
            if (categoria.Imagen != null)
            {
                await DeleteImageFileAsync(categoria.Imagen.NombreArchivo);
                _context.Imagenes.Remove(categoria.Imagen);
            }

            _context.CategoriasArticulos.Remove(categoria);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Deleted category {CategoriaId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse.Success("Category deleted successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error deleting category {CategoriaId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse.Failure(
                "Error deleting category",
                "An error occurred while deleting the category"
            );
        }
    }

    public async Task<ApiResponse> ToggleStatusAsync(
        int id,
        bool anulado,
        int institucionId,
        string? modificadoPorId = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var categoria = await _context.CategoriasArticulos.FirstOrDefaultAsync(
                c => c.CategoriaId == id && c.InstitucionID == institucionId,
                cancellationToken
            );

            if (categoria == null)
            {
                return ApiResponse.Failure("Category not found");
            }

            categoria.Anulado = anulado;
            categoria.FechaModificacion = DateTime.Now;
            categoria.ModificadoPorId = modificadoPorId;

            _context.CategoriasArticulos.Update(categoria);
            await _context.SaveChangesAsync(cancellationToken);

            var statusText = anulado ? "disabled" : "enabled";
            _logger.LogInformation(
                "Category {CategoriaId} {Status} for institution {InstitucionId}",
                id,
                statusText,
                institucionId
            );

            return ApiResponse.Success($"Category {statusText} successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error toggling status for category {CategoriaId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse.Failure(
                "Error toggling category status",
                "An error occurred while toggling the category status"
            );
        }
    }

    public async Task<ApiResponse<byte[]>> GetImageAsync(
        int categoriaId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var categoria = await _context
                .CategoriasArticulos.AsNoTracking()
                .Include(c => c.Imagen)
                .FirstOrDefaultAsync(
                    c => c.CategoriaId == categoriaId && c.InstitucionID == institucionId,
                    cancellationToken
                );

            if (categoria?.Imagen == null)
            {
                return ApiResponse<byte[]>.Failure("Image not found");
            }

            var imagePath = Path.Combine(
                _environment.WebRootPath,
                UPLOADS_FOLDER,
                categoria.Imagen.NombreArchivo
            );

            if (!File.Exists(imagePath))
            {
                return ApiResponse<byte[]>.Failure("Image file not found");
            }

            var imageData = await File.ReadAllBytesAsync(imagePath, cancellationToken);

            return ApiResponse<byte[]>.Success(imageData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image for category {CategoriaId}", categoriaId);
            return ApiResponse<byte[]>.Failure(
                "Error retrieving image",
                "An error occurred while retrieving the image"
            );
        }
    }

    public async Task<ApiResponse<string>> GetImageContentTypeAsync(
        int categoriaId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var categoria = await _context
                .CategoriasArticulos.AsNoTracking()
                .Include(c => c.Imagen)
                .FirstOrDefaultAsync(
                    c => c.CategoriaId == categoriaId && c.InstitucionID == institucionId,
                    cancellationToken
                );

            if (categoria?.Imagen == null)
            {
                return ApiResponse<string>.Failure("Image not found");
            }

            var contentType = GetContentType(categoria.Imagen.NombreArchivo);
            return ApiResponse<string>.Success(contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving image content type for category {CategoriaId}",
                categoriaId
            );
            return ApiResponse<string>.Failure(
                "Error retrieving image content type",
                "An error occurred while retrieving the image content type"
            );
        }
    }

    #region Private Methods

    private CategoriaDto MapToDto(CategoriasArticulos categoria)
    {
        return new CategoriaDto
        {
            CategoriaId = categoria.CategoriaId,
            NombreCategoria = categoria.NombreCategoria ?? string.Empty,
            Anulado = categoria.Anulado ?? false,
            ImagenId = categoria.imagenID,
            ImagenUrl = categoria.imagenID.HasValue
                ? $"/api/v1/categorias/{categoria.CategoriaId}/image"
                : null,
            FechaRegistro = categoria.FechaCreacion ?? categoria.FechaRegistro ?? DateTime.Now,
            FechaModificacion = categoria.FechaModificacion,
            CreadoPorId = categoria.CreadoPorId,
            CreadoPorNombre =
                categoria.CreadoPor != null
                    ? $"{categoria.CreadoPor.FirstName} {categoria.CreadoPor.LastName}".Trim()
                    : null,
            ModificadoPorId = categoria.ModificadoPorId,
            ModificadoPorNombre =
                categoria.ModificadoPor != null
                    ? $"{categoria.ModificadoPor.FirstName} {categoria.ModificadoPor.LastName}".Trim()
                    : null,
        };
    }

    private async Task<ApiResponse<Imagenes>> SaveImageAsync(
        IFormFile image,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Validate image
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return ApiResponse<Imagenes>.Failure(
                    "Invalid image format. Allowed formats: jpg, jpeg, png, gif, bmp"
                );
            }

            if (image.Length > 5 * 1024 * 1024) // 5MB limit
            {
                return ApiResponse<Imagenes>.Failure("Image size cannot exceed 5MB");
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsPath = Path.Combine(_environment.WebRootPath, UPLOADS_FOLDER);

            // Ensure directory exists
            Directory.CreateDirectory(uploadsPath);

            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream, cancellationToken);

            // Create image record
            var imagen = new Imagenes
            {
                NombreArchivo = fileName,
                FechaSubida = DateTime.Now,
                InstitucionID = institucionId,
                Origen = "Categoria",
            };

            _context.Imagenes.Add(imagen);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<Imagenes>.Success(imagen);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error saving image for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<Imagenes>.Failure("Error saving image");
        }
    }

    private async Task DeleteImageFileAsync(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_environment.WebRootPath, UPLOADS_FOLDER, fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting image file {FileName}", fileName);
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream",
        };
    }

    #endregion
}

