using hotel.Data;
using hotel.DTOs.Articulos;
using hotel.DTOs.Common;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

public class ArticulosService : IArticulosService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<ArticulosService> _logger;
    private readonly IWebHostEnvironment _environment;
    private const string UPLOADS_FOLDER = "uploads";

    public ArticulosService(
        HotelDbContext context,
        ILogger<ArticulosService> logger,
        IWebHostEnvironment environment
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task<ApiResponse<IEnumerable<ArticuloDto>>> GetAllAsync(
        int institucionId,
        int? categoriaId = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .Articulos.AsNoTracking()
                .Include(a => a.Imagen)
                .Include(a => a.CreadoPor)
                .Include(a => a.ModificadoPor)
                .Where(a =>
                    a.InstitucionID == institucionId && (a.Anulado == null || a.Anulado == false)
                );

            if (categoriaId.HasValue)
            {
                query = query.Where(a => a.CategoriaID == categoriaId.Value);
            }

            var articulos = await query
                .OrderByDescending(a => a.FechaCreacion ?? a.FechaRegistro)
                .ToListAsync(cancellationToken);

            var articulosDto = articulos.Select(a => MapToDto(a)).ToList();

            _logger.LogInformation(
                "Retrieved {Count} articles for institution {InstitucionId}",
                articulosDto.Count,
                institucionId
            );

            return ApiResponse<IEnumerable<ArticuloDto>>.Success(articulosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving articles for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<ArticuloDto>>.Failure(
                "Error retrieving articles",
                "An error occurred while retrieving the articles"
            );
        }
    }

    public async Task<ApiResponse<ArticuloDto>> GetByIdAsync(
        int id,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var articulo = await _context
                .Articulos.AsNoTracking()
                .Include(a => a.Imagen)
                .Include(a => a.CreadoPor)
                .Include(a => a.ModificadoPor)
                .FirstOrDefaultAsync(
                    a => a.ArticuloId == id && a.InstitucionID == institucionId,
                    cancellationToken
                );

            if (articulo == null)
            {
                return ApiResponse<ArticuloDto>.Failure("Article not found");
            }

            var articuloDto = MapToDto(articulo);

            _logger.LogInformation(
                "Retrieved article {ArticuloId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse<ArticuloDto>.Success(articuloDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving article {ArticuloId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse<ArticuloDto>.Failure(
                "Error retrieving article",
                "An error occurred while retrieving the article"
            );
        }
    }

    public async Task<ApiResponse<ArticuloDto>> CreateAsync(
        ArticuloCreateDto createDto,
        int institucionId,
        string? creadoPorId = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Validate category if provided
            if (createDto.CategoriaId.HasValue)
            {
                var categoryExists = await _context.CategoriasArticulos.AnyAsync(
                    c =>
                        c.CategoriaId == createDto.CategoriaId.Value
                        && c.InstitucionID == institucionId,
                    cancellationToken
                );

                if (!categoryExists)
                {
                    return ApiResponse<ArticuloDto>.Failure("Category not found");
                }
            }

            var articulo = new Articulos
            {
                NombreArticulo = createDto.NombreArticulo,
                Precio = createDto.Precio,
                CategoriaID = createDto.CategoriaId,
                InstitucionID = institucionId,
                Anulado = false,
                FechaRegistro = DateTime.Now,
                FechaCreacion = DateTime.Now,
                CreadoPorId = creadoPorId,
            };

            _context.Articulos.Add(articulo);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Retrieve with includes for response
            var createdArticulo = await _context
                .Articulos.AsNoTracking()
                .Include(a => a.Imagen)
                .Include(a => a.CreadoPor)
                .Include(a => a.ModificadoPor)
                .FirstOrDefaultAsync(a => a.ArticuloId == articulo.ArticuloId, cancellationToken);

            var articuloDto = MapToDto(createdArticulo!);

            _logger.LogInformation(
                "Created article {ArticuloId} for institution {InstitucionId}",
                articulo.ArticuloId,
                institucionId
            );

            return ApiResponse<ArticuloDto>.Success(articuloDto, "Article created successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error creating article for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<ArticuloDto>.Failure(
                "Error creating article",
                "An error occurred while creating the article"
            );
        }
    }

    public async Task<ApiResponse<ArticuloDto>> CreateWithImageAsync(
        ArticuloCreateWithImageDto createDto,
        int institucionId,
        string? creadoPorId = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Validate category if provided
            if (createDto.CategoriaId.HasValue)
            {
                var categoryExists = await _context.CategoriasArticulos.AnyAsync(
                    c =>
                        c.CategoriaId == createDto.CategoriaId.Value
                        && c.InstitucionID == institucionId,
                    cancellationToken
                );

                if (!categoryExists)
                {
                    return ApiResponse<ArticuloDto>.Failure("Category not found");
                }
            }

            // Create article
            var articulo = new Articulos
            {
                NombreArticulo = createDto.NombreArticulo,
                Precio = createDto.Precio,
                CategoriaID = createDto.CategoriaId,
                InstitucionID = institucionId,
                Anulado = false,
                FechaRegistro = DateTime.Now,
                FechaCreacion = DateTime.Now,
                CreadoPorId = creadoPorId,
            };

            _context.Articulos.Add(articulo);
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
                    articulo.imagenID = imageResult.Data!.ImagenId;
                    _context.Articulos.Update(articulo);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            await transaction.CommitAsync(cancellationToken);

            // Retrieve with includes for response
            var createdArticulo = await _context
                .Articulos.AsNoTracking()
                .Include(a => a.Imagen)
                .Include(a => a.CreadoPor)
                .Include(a => a.ModificadoPor)
                .FirstOrDefaultAsync(a => a.ArticuloId == articulo.ArticuloId, cancellationToken);

            var articuloDto = MapToDto(createdArticulo!);

            _logger.LogInformation(
                "Created article with image {ArticuloId} for institution {InstitucionId}",
                articulo.ArticuloId,
                institucionId
            );

            return ApiResponse<ArticuloDto>.Success(
                articuloDto,
                "Article created with image successfully"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error creating article with image for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<ArticuloDto>.Failure(
                "Error creating article with image",
                "An error occurred while creating the article with image"
            );
        }
    }

    public async Task<ApiResponse<ArticuloDto>> UpdateAsync(
        int id,
        ArticuloUpdateDto updateDto,
        int institucionId,
        string? modificadoPorId = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var articulo = await _context.Articulos.FirstOrDefaultAsync(
                a => a.ArticuloId == id && a.InstitucionID == institucionId,
                cancellationToken
            );

            if (articulo == null)
            {
                return ApiResponse<ArticuloDto>.Failure("Article not found");
            }

            // Validate category if provided
            if (updateDto.CategoriaId.HasValue)
            {
                var categoryExists = await _context.CategoriasArticulos.AnyAsync(
                    c =>
                        c.CategoriaId == updateDto.CategoriaId.Value
                        && c.InstitucionID == institucionId,
                    cancellationToken
                );

                if (!categoryExists)
                {
                    return ApiResponse<ArticuloDto>.Failure("Category not found");
                }
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateDto.NombreArticulo))
                articulo.NombreArticulo = updateDto.NombreArticulo;

            if (updateDto.Precio.HasValue)
                articulo.Precio = updateDto.Precio.Value;

            if (updateDto.CategoriaId.HasValue)
                articulo.CategoriaID = updateDto.CategoriaId.Value;

            articulo.FechaModificacion = DateTime.Now;
            articulo.ModificadoPorId = modificadoPorId;

            _context.Articulos.Update(articulo);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Retrieve with includes for response
            var updatedArticulo = await _context
                .Articulos.AsNoTracking()
                .Include(a => a.Imagen)
                .Include(a => a.CreadoPor)
                .Include(a => a.ModificadoPor)
                .FirstOrDefaultAsync(a => a.ArticuloId == id, cancellationToken);

            var articuloDto = MapToDto(updatedArticulo!);

            _logger.LogInformation(
                "Updated article {ArticuloId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse<ArticuloDto>.Success(articuloDto, "Article updated successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error updating article {ArticuloId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse<ArticuloDto>.Failure(
                "Error updating article",
                "An error occurred while updating the article"
            );
        }
    }

    public async Task<ApiResponse<ArticuloDto>> UpdateImageAsync(
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
            var articulo = await _context
                .Articulos.Include(a => a.Imagen)
                .FirstOrDefaultAsync(
                    a => a.ArticuloId == id && a.InstitucionID == institucionId,
                    cancellationToken
                );

            if (articulo == null)
            {
                return ApiResponse<ArticuloDto>.Failure("Article not found");
            }

            if (imagen == null || imagen.Length == 0)
            {
                return ApiResponse<ArticuloDto>.Failure("Invalid image file");
            }

            // Delete old image if exists
            if (articulo.imagenID.HasValue && articulo.Imagen != null)
            {
                await DeleteImageFileAsync(articulo.Imagen.NombreArchivo);
                _context.Imagenes.Remove(articulo.Imagen);
            }

            // Save new image
            var imageResult = await SaveImageAsync(imagen, institucionId, cancellationToken);
            if (!imageResult.IsSuccess)
            {
                return ApiResponse<ArticuloDto>.Failure("Error saving image");
            }

            articulo.imagenID = imageResult.Data!.ImagenId;
            articulo.FechaModificacion = DateTime.Now;
            articulo.ModificadoPorId = modificadoPorId;

            _context.Articulos.Update(articulo);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Retrieve with includes for response
            var updatedArticulo = await _context
                .Articulos.AsNoTracking()
                .Include(a => a.Imagen)
                .Include(a => a.CreadoPor)
                .Include(a => a.ModificadoPor)
                .FirstOrDefaultAsync(a => a.ArticuloId == id, cancellationToken);

            var articuloDto = MapToDto(updatedArticulo!);

            _logger.LogInformation(
                "Updated image for article {ArticuloId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse<ArticuloDto>.Success(
                articuloDto,
                "Article image updated successfully"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error updating image for article {ArticuloId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse<ArticuloDto>.Failure(
                "Error updating article image",
                "An error occurred while updating the article image"
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
            var articulo = await _context
                .Articulos.Include(a => a.Imagen)
                .FirstOrDefaultAsync(
                    a => a.ArticuloId == id && a.InstitucionID == institucionId,
                    cancellationToken
                );

            if (articulo == null)
            {
                return ApiResponse.Failure("Article not found");
            }

            // Check if article is used in any consumption
            var isUsedInConsumption = await _context.Consumo.AnyAsync(
                c => c.ArticuloId == id && (c.Anulado == null || c.Anulado == false),
                cancellationToken
            );

            if (isUsedInConsumption)
            {
                return ApiResponse.Failure(
                    "Cannot delete article as it has associated consumption records"
                );
            }

            // Delete image file if exists
            if (articulo.Imagen != null)
            {
                await DeleteImageFileAsync(articulo.Imagen.NombreArchivo);
                _context.Imagenes.Remove(articulo.Imagen);
            }

            _context.Articulos.Remove(articulo);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Deleted article {ArticuloId} for institution {InstitucionId}",
                id,
                institucionId
            );

            return ApiResponse.Success("Article deleted successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error deleting article {ArticuloId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse.Failure(
                "Error deleting article",
                "An error occurred while deleting the article"
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
            var articulo = await _context.Articulos.FirstOrDefaultAsync(
                a => a.ArticuloId == id && a.InstitucionID == institucionId,
                cancellationToken
            );

            if (articulo == null)
            {
                return ApiResponse.Failure("Article not found");
            }

            articulo.Anulado = anulado;
            articulo.FechaModificacion = DateTime.Now;
            articulo.ModificadoPorId = modificadoPorId;

            _context.Articulos.Update(articulo);
            await _context.SaveChangesAsync(cancellationToken);

            var statusText = anulado ? "disabled" : "enabled";
            _logger.LogInformation(
                "Article {ArticuloId} {Status} for institution {InstitucionId}",
                id,
                statusText,
                institucionId
            );

            return ApiResponse.Success($"Article {statusText} successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error toggling status for article {ArticuloId} for institution {InstitucionId}",
                id,
                institucionId
            );
            return ApiResponse.Failure(
                "Error toggling article status",
                "An error occurred while toggling the article status"
            );
        }
    }

    public async Task<ApiResponse<byte[]>> GetImageAsync(
        int articuloId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var articulo = await _context
                .Articulos.AsNoTracking()
                .Include(a => a.Imagen)
                .FirstOrDefaultAsync(
                    a => a.ArticuloId == articuloId && a.InstitucionID == institucionId,
                    cancellationToken
                );

            if (articulo?.Imagen == null)
            {
                return ApiResponse<byte[]>.Failure("Image not found");
            }

            var imagePath = Path.Combine(
                _environment.WebRootPath,
                UPLOADS_FOLDER,
                articulo.Imagen.NombreArchivo
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
            _logger.LogError(ex, "Error retrieving image for article {ArticuloId}", articuloId);
            return ApiResponse<byte[]>.Failure(
                "Error retrieving image",
                "An error occurred while retrieving the image"
            );
        }
    }

    public async Task<ApiResponse<string>> GetImageContentTypeAsync(
        int articuloId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var articulo = await _context
                .Articulos.AsNoTracking()
                .Include(a => a.Imagen)
                .FirstOrDefaultAsync(
                    a => a.ArticuloId == articuloId && a.InstitucionID == institucionId,
                    cancellationToken
                );

            if (articulo?.Imagen == null)
            {
                return ApiResponse<string>.Failure("Image not found");
            }

            var contentType = GetContentType(articulo.Imagen.NombreArchivo);
            return ApiResponse<string>.Success(contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving image content type for article {ArticuloId}",
                articuloId
            );
            return ApiResponse<string>.Failure(
                "Error retrieving image content type",
                "An error occurred while retrieving the image content type"
            );
        }
    }

    #region Private Methods

    private ArticuloDto MapToDto(Articulos articulo)
    {
        return new ArticuloDto
        {
            ArticuloId = articulo.ArticuloId,
            NombreArticulo = articulo.NombreArticulo ?? string.Empty,
            Precio = articulo.Precio,
            CategoriaId = articulo.CategoriaID,
            ImagenId = articulo.imagenID,
            ImagenUrl = articulo.Imagen?.NombreArchivo ?? null,
            ImagenAPI = articulo.imagenID.HasValue
                ? $"/api/v1/articulos/{articulo.ArticuloId}/image"
                : null,
            Anulado = articulo.Anulado ?? false,
            FechaRegistro = articulo.FechaCreacion ?? articulo.FechaRegistro ?? DateTime.Now,
            FechaModificacion = articulo.FechaModificacion,
            CreadoPorId = articulo.CreadoPorId,
            CreadoPorNombre =
                articulo.CreadoPor != null
                    ? $"{articulo.CreadoPor.FirstName} {articulo.CreadoPor.LastName}".Trim()
                    : null,
            ModificadoPorId = articulo.ModificadoPorId,
            ModificadoPorNombre =
                articulo.ModificadoPor != null
                    ? $"{articulo.ModificadoPor.FirstName} {articulo.ModificadoPor.LastName}".Trim()
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
                Origen = "Articulo",
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
