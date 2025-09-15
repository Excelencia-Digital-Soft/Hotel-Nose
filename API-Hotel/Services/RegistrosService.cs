using Microsoft.EntityFrameworkCore;
using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Registros;
using hotel.Interfaces;
using hotel.Models;

namespace hotel.Services;

public class RegistrosService : IRegistrosService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<RegistrosService> _logger;

    public RegistrosService(HotelDbContext context, ILogger<RegistrosService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<RegistrosPaginadosDto>> GetRegistrosAsync(
        int institucionId,
        TipoRegistro? tipoRegistro = null,
        ModuloSistema? modulo = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? usuarioId = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validaciones básicas
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var query = _context.Registros
                .AsNoTracking()
                .Where(r => r.InstitucionID == institucionId && r.Anulado != true);

            // Aplicar filtros opcionales
            if (tipoRegistro.HasValue)
                query = query.Where(r => r.TipoRegistro == tipoRegistro.Value);

            if (modulo.HasValue)
                query = query.Where(r => r.Modulo == modulo.Value);

            if (fechaDesde.HasValue)
                query = query.Where(r => r.FechaRegistro >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(r => r.FechaRegistro <= fechaHasta.Value);

            if (!string.IsNullOrEmpty(usuarioId))
                query = query.Where(r => r.UsuarioId == usuarioId);

            // Contar total de registros
            var totalRegistros = await query.CountAsync(cancellationToken);

            // Aplicar paginación y obtener datos
            var registros = await query
                .Include(r => r.Usuario)
                .Include(r => r.Institucion)
                .OrderByDescending(r => r.FechaRegistro)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RegistroDto
                {
                    RegistroID = r.RegistroID,
                    Contenido = r.Contenido,
                    TipoRegistro = r.TipoRegistro,
                    TipoRegistroTexto = r.TipoRegistro.ToString(),
                    Modulo = r.Modulo,
                    ModuloTexto = r.Modulo.ToString(),
                    ReservaId = r.ReservaId,
                    UsuarioId = r.UsuarioId,
                    NombreUsuario = r.Usuario != null ? r.Usuario.UserName : null,
                    InstitucionID = r.InstitucionID,
                    NombreInstitucion = r.Institucion != null ? r.Institucion.Nombre : null,
                    FechaRegistro = r.FechaRegistro,
                    DetallesAdicionales = r.DetallesAdicionales,
                    DireccionIP = r.DireccionIP,
                    Anulado = r.Anulado
                })
                .ToListAsync(cancellationToken);

            var totalPaginas = (int)Math.Ceiling((double)totalRegistros / pageSize);

            var resultado = new RegistrosPaginadosDto
            {
                Registros = registros,
                TotalRegistros = totalRegistros,
                PaginaActual = pageNumber,
                TamanoPagina = pageSize,
                TotalPaginas = totalPaginas,
                TienePaginaAnterior = pageNumber > 1,
                TienePaginaSiguiente = pageNumber < totalPaginas
            };

            _logger.LogInformation("Retrieved {Count} registros for institution {InstitucionId}, page {PageNumber} of {TotalPages}",
                registros.Count, institucionId, pageNumber, totalPaginas);

            return ApiResponse<RegistrosPaginadosDto>.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving registros for institution {InstitucionId}", institucionId);
            return ApiResponse<RegistrosPaginadosDto>.Failure(
                "Error retrieving registros",
                "An error occurred while retrieving the registros");
        }
    }

    public async Task<ApiResponse<RegistroDto>> GetRegistroByIdAsync(
        int registroId,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var registro = await _context.Registros
                .AsNoTracking()
                .Include(r => r.Usuario)
                .Include(r => r.Institucion)
                .Where(r => r.RegistroID == registroId && r.InstitucionID == institucionId && r.Anulado != true)
                .Select(r => new RegistroDto
                {
                    RegistroID = r.RegistroID,
                    Contenido = r.Contenido,
                    TipoRegistro = r.TipoRegistro,
                    TipoRegistroTexto = r.TipoRegistro.ToString(),
                    Modulo = r.Modulo,
                    ModuloTexto = r.Modulo.ToString(),
                    ReservaId = r.ReservaId,
                    UsuarioId = r.UsuarioId,
                    NombreUsuario = r.Usuario != null ? r.Usuario.UserName : null,
                    InstitucionID = r.InstitucionID,
                    NombreInstitucion = r.Institucion != null ? r.Institucion.Nombre : null,
                    FechaRegistro = r.FechaRegistro,
                    DetallesAdicionales = r.DetallesAdicionales,
                    DireccionIP = r.DireccionIP,
                    Anulado = r.Anulado
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (registro == null)
            {
                return ApiResponse<RegistroDto>.Failure("Registro not found");
            }

            return ApiResponse<RegistroDto>.Success(registro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving registro {RegistroId} for institution {InstitucionId}",
                registroId, institucionId);
            return ApiResponse<RegistroDto>.Failure(
                "Error retrieving registro",
                "An error occurred while retrieving the registro");
        }
    }

    public async Task<ApiResponse<RegistroDto>> CreateRegistroAsync(
        RegistroCreateDto createDto,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var registro = new Registros
            {
                Contenido = createDto.Contenido,
                TipoRegistro = createDto.TipoRegistro,
                Modulo = createDto.Modulo,
                ReservaId = createDto.ReservaId,
                UsuarioId = usuarioId,
                InstitucionID = institucionId,
                FechaRegistro = DateTime.UtcNow,
                DetallesAdicionales = createDto.DetallesAdicionales,
                DireccionIP = direccionIP,
                Anulado = null
            };

            _context.Registros.Add(registro);
            await _context.SaveChangesAsync(cancellationToken);

            var registroDto = new RegistroDto
            {
                RegistroID = registro.RegistroID,
                Contenido = registro.Contenido,
                TipoRegistro = registro.TipoRegistro,
                TipoRegistroTexto = registro.TipoRegistro.ToString(),
                Modulo = registro.Modulo,
                ModuloTexto = registro.Modulo.ToString(),
                ReservaId = registro.ReservaId,
                UsuarioId = registro.UsuarioId,
                InstitucionID = registro.InstitucionID,
                FechaRegistro = registro.FechaRegistro,
                DetallesAdicionales = registro.DetallesAdicionales,
                DireccionIP = registro.DireccionIP,
                Anulado = registro.Anulado
            };

            _logger.LogInformation("Created registro {RegistroId} for institution {InstitucionId} by user {UsuarioId}",
                registro.RegistroID, institucionId, usuarioId);

            return ApiResponse<RegistroDto>.Success(registroDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating registro for institution {InstitucionId}", institucionId);
            return ApiResponse<RegistroDto>.Failure(
                "Error creating registro",
                "An error occurred while creating the registro");
        }
    }

    #region Métodos convenientes para diferentes tipos de registro

    public async Task<ApiResponse<RegistroDto>> LogInfoAsync(
        string contenido,
        ModuloSistema modulo,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        string? detallesAdicionales = null,
        int? reservaId = null,
        CancellationToken cancellationToken = default)
    {
        var createDto = new RegistroCreateDto
        {
            Contenido = contenido,
            TipoRegistro = TipoRegistro.INFO,
            Modulo = modulo,
            ReservaId = reservaId,
            DetallesAdicionales = detallesAdicionales
        };

        return await CreateRegistroAsync(createDto, institucionId, usuarioId, direccionIP, cancellationToken);
    }

    public async Task<ApiResponse<RegistroDto>> LogWarningAsync(
        string contenido,
        ModuloSistema modulo,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        string? detallesAdicionales = null,
        int? reservaId = null,
        CancellationToken cancellationToken = default)
    {
        var createDto = new RegistroCreateDto
        {
            Contenido = contenido,
            TipoRegistro = TipoRegistro.WARNING,
            Modulo = modulo,
            ReservaId = reservaId,
            DetallesAdicionales = detallesAdicionales
        };

        return await CreateRegistroAsync(createDto, institucionId, usuarioId, direccionIP, cancellationToken);
    }

    public async Task<ApiResponse<RegistroDto>> LogErrorAsync(
        string contenido,
        ModuloSistema modulo,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        string? detallesAdicionales = null,
        int? reservaId = null,
        CancellationToken cancellationToken = default)
    {
        var createDto = new RegistroCreateDto
        {
            Contenido = contenido,
            TipoRegistro = TipoRegistro.ERROR,
            Modulo = modulo,
            ReservaId = reservaId,
            DetallesAdicionales = detallesAdicionales
        };

        return await CreateRegistroAsync(createDto, institucionId, usuarioId, direccionIP, cancellationToken);
    }

    public async Task<ApiResponse<RegistroDto>> LogAuditAsync(
        string contenido,
        ModuloSistema modulo,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        string? detallesAdicionales = null,
        int? reservaId = null,
        CancellationToken cancellationToken = default)
    {
        var createDto = new RegistroCreateDto
        {
            Contenido = contenido,
            TipoRegistro = TipoRegistro.AUDIT,
            Modulo = modulo,
            ReservaId = reservaId,
            DetallesAdicionales = detallesAdicionales
        };

        return await CreateRegistroAsync(createDto, institucionId, usuarioId, direccionIP, cancellationToken);
    }

    public async Task<ApiResponse<RegistroDto>> LogSecurityAsync(
        string contenido,
        ModuloSistema modulo,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        string? detallesAdicionales = null,
        int? reservaId = null,
        CancellationToken cancellationToken = default)
    {
        var createDto = new RegistroCreateDto
        {
            Contenido = contenido,
            TipoRegistro = TipoRegistro.SECURITY,
            Modulo = modulo,
            ReservaId = reservaId,
            DetallesAdicionales = detallesAdicionales
        };

        return await CreateRegistroAsync(createDto, institucionId, usuarioId, direccionIP, cancellationToken);
    }

    #endregion

    public async Task<ApiResponse<int>> LimpiarRegistrosAntiguosAsync(
        int institucionId,
        int diasRetencion = 90,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fechaLimite = DateTime.UtcNow.AddDays(-diasRetencion);

            var registrosEliminados = await _context.Registros
                .Where(r => r.InstitucionID == institucionId && 
                           r.FechaRegistro < fechaLimite &&
                           r.TipoRegistro != TipoRegistro.AUDIT && // No eliminar registros de auditoría
                           r.TipoRegistro != TipoRegistro.SECURITY) // No eliminar registros de seguridad
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation("Cleaned {Count} old registros for institution {InstitucionId} older than {FechaLimite}",
                registrosEliminados, institucionId, fechaLimite);

            return ApiResponse<int>.Success(registrosEliminados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning old registros for institution {InstitucionId}", institucionId);
            return ApiResponse<int>.Failure(
                "Error cleaning old registros",
                "An error occurred while cleaning old registros");
        }
    }

    public async Task<ApiResponse> AnularRegistroAsync(
        int registroId,
        int institucionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var registro = await _context.Registros
                .FirstOrDefaultAsync(r => r.RegistroID == registroId && 
                                         r.InstitucionID == institucionId &&
                                         r.Anulado != true, 
                                    cancellationToken);

            if (registro == null)
            {
                return ApiResponse.Failure("Registro not found");
            }

            registro.Anulado = true;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Anulado registro {RegistroId} for institution {InstitucionId}",
                registroId, institucionId);

            return ApiResponse.Success("Registro anulado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error anulando registro {RegistroId} for institution {InstitucionId}",
                registroId, institucionId);
            return ApiResponse.Failure(
                "Error anulando registro",
                "An error occurred while anulando the registro");
        }
    }
}