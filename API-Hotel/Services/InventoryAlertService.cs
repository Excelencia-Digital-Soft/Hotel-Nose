using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service for inventory alert operations and configuration
/// </summary>
public class InventoryAlertService : IInventoryAlertService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<InventoryAlertService> _logger;

    public InventoryAlertService(HotelDbContext context, ILogger<InventoryAlertService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<PagedResult<AlertaInventarioDto>>> GetActiveAlertsAsync(
        int institucionId,
        AlertaInventarioFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            IQueryable<AlertaInventario> query;
            try
            {
                query = _context
                    .AlertasInventario.AsNoTracking()
                    .Include(a => a.Inventario)
                    .ThenInclude(i => i!.Articulo)
                    .Include(a => a.UsuarioQueReconocio)
                    .Where(a => a.InstitucionID == institucionId);
            }
            catch (Exception dbEx) when (dbEx.Message.Contains("Invalid column name") || 
                                       dbEx.Message.Contains("Invalid object name") ||
                                       dbEx.Message.Contains("nombre de columna") ||
                                       dbEx.Message.Contains("no es válido"))
            {
                _logger.LogWarning(
                    "AlertasInventario table not found. Please run script 14-Create_Inventory_Alerts_Tables.sql. Error: {Error}",
                    dbEx.Message
                );
                
                return ApiResponse<PagedResult<AlertaInventarioDto>>.Success(
                    new PagedResult<AlertaInventarioDto>
                    {
                        Items = new List<AlertaInventarioDto>(),
                        TotalCount = 0,
                        Page = filter.Pagina,
                        PageSize = filter.TamanoPagina,
                        TotalPages = 0
                    }
                );
            }

            // Apply filters
            if (filter.SoloActivas)
            {
                query = query.Where(a => a.EsActiva);
            }

            if (filter.SoloNoReconocidas)
            {
                query = query.Where(a => !a.FueReconocida);
            }

            if (!string.IsNullOrEmpty(filter.TipoAlerta))
            {
                query = query.Where(a => a.TipoAlerta == filter.TipoAlerta);
            }

            if (!string.IsNullOrEmpty(filter.Severidad))
            {
                query = query.Where(a => a.Severidad == filter.Severidad);
            }

            if (filter.InventarioId.HasValue)
            {
                query = query.Where(a => a.InventarioId == filter.InventarioId.Value);
            }

            if (filter.FechaDesde.HasValue)
            {
                query = query.Where(a => a.FechaCreacion >= filter.FechaDesde.Value);
            }

            if (filter.FechaHasta.HasValue)
            {
                query = query.Where(a => a.FechaCreacion <= filter.FechaHasta.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = filter.OrdenarPor?.ToLower() switch
            {
                "fecha" => filter.Descendente
                    ? query.OrderByDescending(a => a.FechaCreacion)
                    : query.OrderBy(a => a.FechaCreacion),
                "severidad" => filter.Descendente
                    ? query.OrderByDescending(a => a.Severidad)
                    : query.OrderBy(a => a.Severidad),
                "tipo" => filter.Descendente
                    ? query.OrderByDescending(a => a.TipoAlerta)
                    : query.OrderBy(a => a.TipoAlerta),
                _ => query.OrderByDescending(a => a.FechaCreacion),
            };

            // Apply pagination
            var alertas = await query
                .Skip((filter.Pagina - 1) * filter.TamanoPagina)
                .Take(filter.TamanoPagina)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var alertaDtos = new List<AlertaInventarioDto>();
            foreach (var alerta in alertas)
            {
                alertaDtos.Add(MapToAlertaDto(alerta));
            }

            var result = new PagedResult<AlertaInventarioDto>
            {
                Items = alertaDtos,
                TotalCount = totalCount,
                Page = filter.Pagina,
                PageSize = filter.TamanoPagina,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.TamanoPagina),
            };

            // Add statistics
            var stats = await GetAlertStatisticsForCurrentQuery(query, cancellationToken);

            return ApiResponse<PagedResult<AlertaInventarioDto>>.Success(
                result,
                $"Retrieved {alertaDtos.Count} alerts. Active: {stats.TotalActivas}, Critical: {stats.TotalCriticas}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving alerts for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<PagedResult<AlertaInventarioDto>>.Failure(
                "Error retrieving alerts",
                "An error occurred while retrieving the inventory alerts"
            );
        }
    }

    public async Task<ApiResponse<ConfiguracionAlertaDto>> ConfigureAlertsAsync(
        ConfiguracionAlertaCreateUpdateDto configDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Validate inventory exists and belongs to institution
            var inventario = await _context
                .InventarioUnificado.AsNoTracking()
                .FirstOrDefaultAsync(
                    i =>
                        i.InventarioId == configDto.InventarioId
                        && i.InstitucionID == institucionId,
                    cancellationToken
                );

            if (inventario == null)
            {
                return ApiResponse<ConfiguracionAlertaDto>.Failure("Inventory not found");
            }

            // Check if configuration already exists
            var existingConfig = await _context.ConfiguracionAlertasInventario.FirstOrDefaultAsync(
                c => c.InventarioId == configDto.InventarioId,
                cancellationToken
            );

            ConfiguracionAlertaInventario config;

            if (existingConfig != null)
            {
                // Update existing configuration
                existingConfig.StockMinimo = configDto.StockMinimo;
                existingConfig.StockMaximo = configDto.StockMaximo;
                existingConfig.StockCritico = configDto.StockCritico;
                existingConfig.AlertasStockBajoActivas = configDto.AlertasStockBajoActivas;
                existingConfig.AlertasStockAltoActivas = configDto.AlertasStockAltoActivas;
                existingConfig.AlertasStockCriticoActivas = configDto.AlertasStockCriticoActivas;
                existingConfig.NotificacionEmailActiva = configDto.NotificacionEmailActiva;
                existingConfig.NotificacionSmsActiva = configDto.NotificacionSmsActiva;
                existingConfig.EmailsNotificacion = configDto.EmailsNotificacion;
                existingConfig.TelefonosNotificacion = configDto.TelefonosNotificacion;
                existingConfig.FrecuenciaRevisionMinutos = configDto.FrecuenciaRevisionMinutos ?? 0;
                existingConfig.EsActiva = configDto.EsActiva;
                existingConfig.FechaActualizacion = DateTime.UtcNow;
                existingConfig.UsuarioActualizacion = userId;

                config = existingConfig;
            }
            else
            {
                // Create new configuration
                config = new ConfiguracionAlertaInventario
                {
                    InventarioId = configDto.InventarioId,
                    InstitucionID = institucionId,
                    StockMinimo = configDto.StockMinimo,
                    StockMaximo = configDto.StockMaximo,
                    StockCritico = configDto.StockCritico,
                    AlertasStockBajoActivas = configDto.AlertasStockBajoActivas,
                    AlertasStockAltoActivas = configDto.AlertasStockAltoActivas,
                    AlertasStockCriticoActivas = configDto.AlertasStockCriticoActivas,
                    NotificacionEmailActiva = configDto.NotificacionEmailActiva,
                    NotificacionSmsActiva = configDto.NotificacionSmsActiva,
                    EmailsNotificacion = configDto.EmailsNotificacion,
                    TelefonosNotificacion = configDto.TelefonosNotificacion,
                    FrecuenciaRevisionMinutos = configDto.FrecuenciaRevisionMinutos ?? 60,
                    EsActiva = configDto.EsActiva,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = userId,
                };

                try
                {
                    _context.ConfiguracionAlertasInventario.Add(config);
                }
                catch (Exception dbEx) when (dbEx.Message.Contains("Invalid column name") || 
                                           dbEx.Message.Contains("Invalid object name") ||
                                           dbEx.Message.Contains("nombre de columna") ||
                                           dbEx.Message.Contains("no es válido"))
                {
                    _logger.LogError(
                        "Cannot configure alerts - ConfiguracionAlertasInventario table not found. Please run script 14-Create_Inventory_Alerts_Tables.sql. Error: {Error}",
                        dbEx.Message
                    );
                    
                    return ApiResponse<ConfiguracionAlertaDto>.Failure(
                        "Alert configuration not available. Please run database migration scripts to create alert tables."
                    );
                }
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception dbEx) when (dbEx.Message.Contains("Invalid column name") || 
                                       dbEx.Message.Contains("Invalid object name") ||
                                       dbEx.Message.Contains("nombre de columna") ||
                                       dbEx.Message.Contains("no es válido"))
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(
                    "Cannot save alert configuration - tables not found. Please run script 14-Create_Inventory_Alerts_Tables.sql. Error: {Error}",
                    dbEx.Message
                );
                
                return ApiResponse<ConfiguracionAlertaDto>.Failure(
                    "Alert configuration not available. Please run database migration scripts to create alert tables."
                );
            }

            // Map to DTO
            var configDto_result = new ConfiguracionAlertaDto
            {
                ConfiguracionId = config.ConfiguracionId,
                InventarioId = config.InventarioId,
                StockMinimo = config.StockMinimo,
                StockMaximo = config.StockMaximo,
                StockCritico = config.StockCritico,
                AlertasStockBajoActivas = config.AlertasStockBajoActivas,
                AlertasStockAltoActivas = config.AlertasStockAltoActivas,
                AlertasStockCriticoActivas = config.AlertasStockCriticoActivas,
                NotificacionEmailActiva = config.NotificacionEmailActiva,
                NotificacionSmsActiva = config.NotificacionSmsActiva,
                EmailsNotificacion = config.EmailsNotificacion,
                TelefonosNotificacion = config.TelefonosNotificacion,
                FrecuenciaRevisionMinutos = config.FrecuenciaRevisionMinutos,
                EsActiva = config.EsActiva,
                FechaCreacion = config.FechaCreacion,
                FechaActualizacion = config.FechaActualizacion,
                NombreUsuarioCreacion = userId,
                NombreUsuarioActualizacion = config.UsuarioActualizacion,
            };

            _logger.LogInformation(
                "Alert configuration {Action} for inventory {InventarioId} by user {UserId}",
                existingConfig != null ? "updated" : "created",
                configDto.InventarioId,
                userId
            );

            return ApiResponse<ConfiguracionAlertaDto>.Success(
                configDto_result,
                $"Alert configuration {(existingConfig != null ? "updated" : "created")} successfully"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error configuring alerts for inventory {InventarioId} in institution {InstitucionId}",
                configDto.InventarioId,
                institucionId
            );
            return ApiResponse<ConfiguracionAlertaDto>.Failure(
                "Error configuring alerts",
                "An error occurred while configuring the alert settings"
            );
        }
    }

    public async Task<ApiResponse<AlertaInventarioDto>> AcknowledgeAlertAsync(
        int alertId,
        AlertaReconocimientoDto acknowledgmentDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Find the alert
            var alerta = await _context
                .AlertasInventario.Include(a => a.Inventario)
                .ThenInclude(i => i!.Articulo)
                .FirstOrDefaultAsync(
                    a => a.AlertaId == alertId && a.InstitucionID == institucionId,
                    cancellationToken
                );

            if (alerta == null)
            {
                return ApiResponse<AlertaInventarioDto>.Failure("Alert not found");
            }

            if (alerta.FueReconocida)
            {
                return ApiResponse<AlertaInventarioDto>.Failure(
                    "Alert has already been acknowledged"
                );
            }

            if (!alerta.EsActiva)
            {
                return ApiResponse<AlertaInventarioDto>.Failure("Alert is not active");
            }

            // Update alert
            alerta.FueReconocida = true;
            alerta.FechaReconocimiento = DateTime.UtcNow;
            alerta.UsuarioReconocimiento = userId;
            alerta.NotasReconocimiento = acknowledgmentDto.Notas;

            // If marked as resolved, update resolution fields
            if (acknowledgmentDto.MarcarComoResuelto)
            {
                alerta.EsActiva = false;
                alerta.FechaResolucion = DateTime.UtcNow;
                alerta.UsuarioResolucion = userId;
                alerta.NotasResolucion = acknowledgmentDto.NotasResolucion;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Map to DTO
            var alertaDto = MapToAlertaDto(alerta);

            _logger.LogInformation(
                "Alert {AlertaId} acknowledged by user {UserId} with resolution: {Resolved}",
                alertId,
                userId,
                acknowledgmentDto.MarcarComoResuelto
            );

            return ApiResponse<AlertaInventarioDto>.Success(
                alertaDto,
                $"Alert {(acknowledgmentDto.MarcarComoResuelto ? "acknowledged and resolved" : "acknowledged")} successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error acknowledging alert {AlertaId} in institution {InstitucionId}",
                alertId,
                institucionId
            );
            return ApiResponse<AlertaInventarioDto>.Failure(
                "Error acknowledging alert",
                "An error occurred while acknowledging the alert"
            );
        }
    }

    public async Task<ApiResponse<ConfiguracionAlertaDto>> GetAlertConfigurationAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            ConfiguracionAlertaInventario? config;
            try
            {
                config = await _context
                    .ConfiguracionAlertasInventario.AsNoTracking()
                    .Include(c => c.CreadoPor)
                    .Include(c => c.ModificadoPor)
                    .FirstOrDefaultAsync(
                        c => c.InventarioId == inventoryId && c.InstitucionID == institucionId,
                        cancellationToken
                    );
            }
            catch (Exception dbEx) when (dbEx.Message.Contains("Invalid column name") || 
                                       dbEx.Message.Contains("Invalid object name") ||
                                       dbEx.Message.Contains("nombre de columna") ||
                                       dbEx.Message.Contains("no es válido"))
            {
                _logger.LogWarning(
                    "ConfiguracionAlertasInventario table not found. Please run script 14-Create_Inventory_Alerts_Tables.sql. Error: {Error}",
                    dbEx.Message
                );
                
                return ApiResponse<ConfiguracionAlertaDto>.Failure(
                    "Alert configuration not available. Please run database migration scripts to create alert tables."
                );
            }

            if (config == null)
            {
                return ApiResponse<ConfiguracionAlertaDto>.Failure("Alert configuration not found");
            }

            var configDto = new ConfiguracionAlertaDto
            {
                ConfiguracionId = config.ConfiguracionId,
                InventarioId = config.InventarioId,
                StockMinimo = config.StockMinimo,
                StockMaximo = config.StockMaximo,
                StockCritico = config.StockCritico,
                AlertasStockBajoActivas = config.AlertasStockBajoActivas,
                AlertasStockAltoActivas = config.AlertasStockAltoActivas,
                AlertasStockCriticoActivas = config.AlertasStockCriticoActivas,
                NotificacionEmailActiva = config.NotificacionEmailActiva,
                NotificacionSmsActiva = config.NotificacionSmsActiva,
                EmailsNotificacion = config.EmailsNotificacion,
                TelefonosNotificacion = config.TelefonosNotificacion,
                FrecuenciaRevisionMinutos = config.FrecuenciaRevisionMinutos,
                EsActiva = config.EsActiva,
                FechaCreacion = config.FechaCreacion,
                FechaActualizacion = config.FechaActualizacion,
                NombreUsuarioCreacion = config.CreadoPor?.UserName ?? "Usuario Desconocido",
                NombreUsuarioActualizacion = config.ModificadoPor?.UserName,
            };

            return ApiResponse<ConfiguracionAlertaDto>.Success(configDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving alert configuration for inventory {InventoryId} in institution {InstitucionId}",
                inventoryId,
                institucionId
            );
            return ApiResponse<ConfiguracionAlertaDto>.Failure(
                "Error retrieving configuration",
                "An error occurred while retrieving the alert configuration"
            );
        }
    }

    public async Task<ApiResponse<AlertaGenerationResultDto>> CheckAndGenerateAlertsAsync(
        int inventoryId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var result = new AlertaGenerationResultDto
            {
                InventoryId = inventoryId,
                AlertasGeneradas = new List<AlertaInventarioDto>(),
                AlertasDesactivadas = new List<int>(),
            };

            // Get inventory with configuration
            var inventario = await _context
                .InventarioUnificado.Include(i => i.Articulo)
                .FirstOrDefaultAsync(
                    i => i.InventarioId == inventoryId && i.InstitucionID == institucionId,
                    cancellationToken
                );

            if (inventario == null)
            {
                return ApiResponse<AlertaGenerationResultDto>.Failure("Inventory not found");
            }

            // Get alert configuration - handle case where tables don't exist yet
            ConfiguracionAlertaInventario? config = null;
            try
            {
                config = await _context.ConfiguracionAlertasInventario.FirstOrDefaultAsync(
                    c => c.InventarioId == inventoryId,
                    cancellationToken
                );
            }
            catch (Exception dbEx) when (dbEx.Message.Contains("Invalid column name") || 
                                       dbEx.Message.Contains("Invalid object name") ||
                                       dbEx.Message.Contains("nombre de columna") ||
                                       dbEx.Message.Contains("no es válido"))
            {
                _logger.LogWarning(
                    "Alert tables not found in database. Please run script 14-Create_Inventory_Alerts_Tables.sql. Error: {Error}",
                    dbEx.Message
                );
                
                result.Mensaje = "Alert system not configured (tables missing). Please run database migration scripts.";
                return ApiResponse<AlertaGenerationResultDto>.Success(result);
            }

            if (config == null || !config.EsActiva)
            {
                result.Mensaje = "No active alert configuration found";
                return ApiResponse<AlertaGenerationResultDto>.Success(result);
            }

            var currentStock = inventario.Cantidad;

            // Deactivate resolved alerts first - also handle missing tables
            List<AlertaInventario> alertsToDeactivate = new();
            try
            {
                alertsToDeactivate = await _context
                    .AlertasInventario.Where(a => a.InventarioId == inventoryId && a.EsActiva)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception dbEx) when (dbEx.Message.Contains("Invalid column name") || 
                                       dbEx.Message.Contains("Invalid object name") ||
                                       dbEx.Message.Contains("nombre de columna") ||
                                       dbEx.Message.Contains("no es válido"))
            {
                _logger.LogDebug("AlertasInventario table not found, skipping alert deactivation check");
                // Continue without deactivating alerts - table doesn't exist yet
            }

            foreach (var alert in alertsToDeactivate)
            {
                bool shouldDeactivate = alert.TipoAlerta switch
                {
                    "StockBajo" => currentStock > config.StockMinimo,
                    "StockCritico" => currentStock > config.StockCritico,
                    "StockAlto" => config.StockMaximo.HasValue
                        && currentStock <= config.StockMaximo,
                    "StockAgotado" => currentStock > 0,
                    _ => false,
                };

                if (shouldDeactivate)
                {
                    alert.EsActiva = false;
                    alert.FechaResolucion = DateTime.UtcNow;
                    alert.NotasResolucion = "Resuelto automáticamente - condición ya no aplica";
                    result.AlertasDesactivadas.Add(alert.AlertaId);
                }
            }

            // Generate new alerts
            var alertsToCreate = new List<AlertaInventario>();

            // Check for stock agotado (out of stock)
            if (currentStock == 0)
            {
                var existingAlert = await _context
                    .AlertasInventario.Where(a =>
                        a.InventarioId == inventoryId
                        && a.TipoAlerta == "StockAgotado"
                        && a.EsActiva
                    )
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingAlert == null)
                {
                    alertsToCreate.Add(
                        new AlertaInventario
                        {
                            InventarioId = inventoryId,
                            InstitucionID = institucionId,
                            TipoAlerta = "StockAgotado",
                            Severidad = "Critica",
                            Mensaje = $"Stock agotado para {inventario.Articulo!.NombreArticulo}",
                            CantidadActual = currentStock,
                            UmbralConfiguracion = 0,
                            EsActiva = true,
                            FueReconocida = false,
                            FechaCreacion = DateTime.UtcNow,
                        }
                    );
                }
            }
            // Check for critical stock
            else if (currentStock <= config.StockCritico && config.AlertasStockCriticoActivas)
            {
                var existingAlert = await _context
                    .AlertasInventario.Where(a =>
                        a.InventarioId == inventoryId
                        && a.TipoAlerta == "StockCritico"
                        && a.EsActiva
                    )
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingAlert == null)
                {
                    alertsToCreate.Add(
                        new AlertaInventario
                        {
                            InventarioId = inventoryId,
                            InstitucionID = institucionId,
                            TipoAlerta = "StockCritico",
                            Severidad = "Alta",
                            Mensaje =
                                $"Stock crítico para {inventario.Articulo!.NombreArticulo} - {currentStock} unidades restantes",
                            CantidadActual = currentStock,
                            UmbralConfiguracion = config.StockCritico,
                            EsActiva = true,
                            FueReconocida = false,
                            FechaCreacion = DateTime.UtcNow,
                        }
                    );
                }
            }
            // Check for low stock
            else if (currentStock <= config.StockMinimo && config.AlertasStockBajoActivas)
            {
                var existingAlert = await _context
                    .AlertasInventario.Where(a =>
                        a.InventarioId == inventoryId && a.TipoAlerta == "StockBajo" && a.EsActiva
                    )
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingAlert == null)
                {
                    alertsToCreate.Add(
                        new AlertaInventario
                        {
                            InventarioId = inventoryId,
                            InstitucionID = institucionId,
                            TipoAlerta = "StockBajo",
                            Severidad = "Media",
                            Mensaje =
                                $"Stock bajo para {inventario.Articulo!.NombreArticulo} - {currentStock} unidades restantes",
                            CantidadActual = currentStock,
                            UmbralConfiguracion = config.StockMinimo,
                            EsActiva = true,
                            FueReconocida = false,
                            FechaCreacion = DateTime.UtcNow,
                        }
                    );
                }
            }

            // Check for high stock
            if (
                config.StockMaximo.HasValue
                && currentStock > config.StockMaximo
                && config.AlertasStockAltoActivas
            )
            {
                var existingAlert = await _context
                    .AlertasInventario.Where(a =>
                        a.InventarioId == inventoryId && a.TipoAlerta == "StockAlto" && a.EsActiva
                    )
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingAlert == null)
                {
                    alertsToCreate.Add(
                        new AlertaInventario
                        {
                            InventarioId = inventoryId,
                            InstitucionID = institucionId,
                            TipoAlerta = "StockAlto",
                            Severidad = "Baja",
                            Mensaje =
                                $"Stock alto para {inventario.Articulo!.NombreArticulo} - {currentStock} unidades",
                            CantidadActual = currentStock,
                            UmbralConfiguracion = config.StockMaximo.Value,
                            EsActiva = true,
                            FueReconocida = false,
                            FechaCreacion = DateTime.UtcNow,
                        }
                    );
                }
            }

            // Add new alerts to context - handle missing tables
            try
            {
                if (alertsToCreate.Any())
                {
                    _context.AlertasInventario.AddRange(alertsToCreate);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception dbEx) when (dbEx.Message.Contains("Invalid column name") || 
                                       dbEx.Message.Contains("Invalid object name") ||
                                       dbEx.Message.Contains("nombre de columna") ||
                                       dbEx.Message.Contains("no es válido"))
            {
                _logger.LogWarning(
                    "Cannot save alerts - AlertasInventario table not found. Please run script 14-Create_Inventory_Alerts_Tables.sql. Error: {Error}",
                    dbEx.Message
                );
                
                result.Mensaje = "Alerts detected but cannot be saved (tables missing). Please run database migration scripts.";
                return ApiResponse<AlertaGenerationResultDto>.Success(result);
            }

            // Map created alerts to DTOs
            foreach (var alerta in alertsToCreate)
            {
                result.AlertasGeneradas.Add(MapToAlertaDto(alerta));
            }

            result.Mensaje =
                $"Generated {alertsToCreate.Count} new alerts, deactivated {result.AlertasDesactivadas.Count} resolved alerts";

            _logger.LogInformation(
                "Alert check completed for inventory {InventoryId}: {NewAlerts} generated, {DeactivatedAlerts} deactivated",
                inventoryId,
                alertsToCreate.Count,
                result.AlertasDesactivadas.Count
            );

            return ApiResponse<AlertaGenerationResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking and generating alerts for inventory {InventoryId}",
                inventoryId
            );
            return ApiResponse<AlertaGenerationResultDto>.Failure(
                "Error checking alerts",
                "An error occurred while checking and generating alerts"
            );
        }
    }

    public async Task<ApiResponse<AlertaEstadisticasDto>> GetAlertStatisticsAsync(
        int institucionId,
        AlertaEstadisticasFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .AlertasInventario.AsNoTracking()
                .Where(a => a.InstitucionID == institucionId);

            // Apply date filters
            if (filter.FechaDesde.HasValue)
            {
                query = query.Where(a => a.FechaCreacion >= filter.FechaDesde.Value);
            }

            if (filter.FechaHasta.HasValue)
            {
                query = query.Where(a => a.FechaCreacion <= filter.FechaHasta.Value);
            }

            // Get statistics
            var totalAlertas = await query.CountAsync(cancellationToken);
            var alertasActivas = await query.CountAsync(a => a.EsActiva, cancellationToken);
            var alertasReconocidas = await query.CountAsync(
                a => a.FueReconocida,
                cancellationToken
            );

            var alertasPorTipo = await query
                .GroupBy(a => a.TipoAlerta)
                .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                .ToListAsync(cancellationToken);

            var alertasPorSeveridad = await query
                .GroupBy(a => a.Severidad)
                .Select(g => new { Severidad = g.Key, Cantidad = g.Count() })
                .ToListAsync(cancellationToken);

            var alertasPorDia = await query
                .Where(a => a.FechaCreacion >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(a => a.FechaCreacion.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
                .OrderBy(x => x.Fecha)
                .ToListAsync(cancellationToken);

            var estadisticas = new AlertaEstadisticasDto
            {
                TotalAlertas = totalAlertas,
                TotalActivas = alertasActivas,
                TotalReconocidas = alertasReconocidas,
                TotalCriticas =
                    alertasPorSeveridad.FirstOrDefault(x => x.Severidad == "Critica")?.Cantidad
                    ?? 0,
                AlertasPorTipo = alertasPorTipo.ToDictionary(x => x.Tipo, x => x.Cantidad),
                AlertasPorSeveridad = alertasPorSeveridad.ToDictionary(
                    x => x.Severidad,
                    x => x.Cantidad
                ),
                AlertasPorDia = alertasPorDia.ToDictionary(x => x.Fecha, x => x.Cantidad),
                PorcentajeReconocimiento =
                    totalAlertas > 0 ? (double)alertasReconocidas / totalAlertas * 100 : 0,
                TiempoPromedioResolucion = await CalculateAverageResolutionTime(
                    query,
                    cancellationToken
                ),
            };

            return ApiResponse<AlertaEstadisticasDto>.Success(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving alert statistics for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<AlertaEstadisticasDto>.Failure(
                "Error retrieving statistics",
                "An error occurred while retrieving alert statistics"
            );
        }
    }

    public async Task<ApiResponse<int>> ResolveMultipleAlertsAsync(
        List<int> alertIds,
        string resolutionNotes,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var alertas = await _context
                .AlertasInventario.Where(a =>
                    alertIds.Contains(a.AlertaId) && a.InstitucionID == institucionId && a.EsActiva
                )
                .ToListAsync(cancellationToken);

            if (!alertas.Any())
            {
                return ApiResponse<int>.Failure("No active alerts found with the provided IDs");
            }

            var resolved = 0;
            var now = DateTime.UtcNow;

            foreach (var alerta in alertas)
            {
                alerta.EsActiva = false;
                alerta.FueReconocida = true;
                alerta.FechaReconocimiento = now;
                alerta.FechaResolucion = now;
                alerta.UsuarioReconocimiento = userId;
                alerta.UsuarioResolucion = userId;
                alerta.NotasResolucion = resolutionNotes;
                resolved++;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Resolved {Count} alerts in batch by user {UserId}",
                resolved,
                userId
            );

            return ApiResponse<int>.Success(resolved, $"Successfully resolved {resolved} alerts");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error resolving multiple alerts in institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<int>.Failure(
                "Error resolving alerts",
                "An error occurred while resolving the alerts"
            );
        }
    }

    #region Private Methods

    private AlertaInventarioDto MapToAlertaDto(AlertaInventario alerta)
    {
        return new AlertaInventarioDto
        {
            AlertaId = alerta.AlertaId,
            InventarioId = alerta.InventarioId,
            TipoAlerta = alerta.TipoAlerta,
            Severidad = alerta.Severidad,
            Mensaje = alerta.Mensaje,
            CantidadActual = alerta.CantidadActual,
            UmbralConfiguracion = alerta.UmbralConfiguracion,
            EsActiva = alerta.EsActiva,
            FueReconocida = alerta.FueReconocida,
            FechaCreacion = alerta.FechaCreacion,
            FechaReconocimiento = alerta.FechaReconocimiento,
            FechaResolucion = alerta.FechaResolucion,
            NombreUsuarioReconocimiento = alerta.UsuarioQueReconocio?.UserName,
            NombreUsuarioResolucion = alerta.UsuarioQueResolvio?.UserName,
            NotasReconocimiento = alerta.NotasReconocimiento,
            NotasResolucion = alerta.NotasResolucion,
            NombreArticulo = alerta.Inventario?.Articulo?.NombreArticulo ?? "Artículo Desconocido",
            CodigoArticulo = alerta.Inventario?.Articulo?.ArticuloId.ToString() ?? "",
            UbicacionNombre = GetUbicacionName(alerta.Inventario),
        };
    }

    private string GetUbicacionName(InventarioUnificado? inventario)
    {
        if (inventario == null)
            return "Ubicación Desconocida";

        return inventario.TipoUbicacion switch
        {
            0 => "Inventario General",
            1 => $"Habitación {inventario.UbicacionId}",
            2 => $"Almacén {inventario.UbicacionId}",
            _ => "Ubicación Desconocida",
        };
    }

    private async Task<TotalAlertas> GetAlertStatisticsForCurrentQuery(
        IQueryable<AlertaInventario> query,
        CancellationToken cancellationToken
    )
    {
        var totalActivas = await query.CountAsync(a => a.EsActiva, cancellationToken);
        var totalCriticas = await query.CountAsync(
            a => a.Severidad == "Critica",
            cancellationToken
        );
        var totalReconocidas = await query.CountAsync(a => a.FueReconocida, cancellationToken);

        return new TotalAlertas
        {
            TotalActivas = totalActivas,
            TotalCriticas = totalCriticas,
            TotalReconocidas = totalReconocidas,
        };
    }

    private async Task<double> CalculateAverageResolutionTime(
        IQueryable<AlertaInventario> query,
        CancellationToken cancellationToken
    )
    {
        var resolvedAlerts = await query
            .Where(a => a.FechaResolucion.HasValue)
            .Select(a => new { Created = a.FechaCreacion, Resolved = a.FechaResolucion!.Value })
            .ToListAsync(cancellationToken);

        if (!resolvedAlerts.Any())
            return 0;

        var totalHours = resolvedAlerts.Select(a => (a.Resolved - a.Created).TotalHours).Average();

        return totalHours;
    }

    #endregion
}
