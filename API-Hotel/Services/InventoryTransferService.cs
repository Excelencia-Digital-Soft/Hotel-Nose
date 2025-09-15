using System.Text.Json;
using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service for inventory transfer operations with approval workflow
/// </summary>
public class InventoryTransferService : IInventoryTransferService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<InventoryTransferService> _logger;
    private readonly IInventoryMovementService _movementService;

    public InventoryTransferService(
        HotelDbContext context,
        ILogger<InventoryTransferService> logger,
        IInventoryMovementService movementService
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _movementService =
            movementService ?? throw new ArgumentNullException(nameof(movementService));
    }

    public async Task<ApiResponse<TransferenciaInventarioDto>> CreateTransferAsync(
        TransferenciaCreateDto transferDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Generate transfer number
            var transferNumber = await GenerateTransferNumber(institucionId, cancellationToken);

            // Create transfer
            var transferencia = new TransferenciaInventario
            {
                InstitucionID = institucionId,
                NumeroTransferencia = transferNumber,
                TipoUbicacionOrigen = transferDto.TipoUbicacionOrigen,
                UbicacionIdOrigen = transferDto.UbicacionIdOrigen,
                TipoUbicacionDestino = transferDto.TipoUbicacionDestino,
                UbicacionIdDestino = transferDto.UbicacionIdDestino,
                Estado = EstadoTransferencia.Pendiente,
                Prioridad = transferDto.Prioridad ?? PrioridadTransferencia.Media,
                Motivo = transferDto.Motivo,
                Notas = transferDto.Notas,
                RequiereAprobacion = transferDto.RequiereAprobacion,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacion = userId,
                DireccionIP = ipAddress,
                Detalles = new List<DetalleTransferenciaInventario>(),
            };

            // Validate and create transfer details
            foreach (var detalle in transferDto.Detalles)
            {
                // Find source inventory using the specific inventory ID from the detail
                var inventarioOrigen = await _context
                    .InventarioUnificado.Include(i => i.Articulo)
                    .FirstOrDefaultAsync(
                        i =>
                            i.InventarioId == detalle.InventarioId
                            && i.InstitucionID == institucionId
                            && i.TipoUbicacion == transferDto.TipoUbicacionOrigen
                            && (
                                transferDto.UbicacionIdOrigen == null
                                || i.UbicacionId == transferDto.UbicacionIdOrigen
                            ),
                        cancellationToken
                    );

                if (inventarioOrigen == null)
                {
                    return ApiResponse<TransferenciaInventarioDto>.Failure(
                        $"Source inventory {detalle.InventarioId} not found or not in specified source location"
                    );
                }

                if (inventarioOrigen.Cantidad < detalle.CantidadSolicitada)
                {
                    return ApiResponse<TransferenciaInventarioDto>.Failure(
                        $"Insufficient quantity for article {inventarioOrigen.Articulo?.NombreArticulo}. Available: {inventarioOrigen.Cantidad}, Requested: {detalle.CantidadSolicitada}"
                    );
                }

                // Find or create destination inventory for the same article
                var inventarioDestino = await FindOrCreateDestinationInventory(
                    inventarioOrigen.ArticuloId, // Use ArticuloId, not InventarioId
                    transferDto.TipoUbicacionDestino,
                    transferDto.UbicacionIdDestino!.Value,
                    institucionId,
                    cancellationToken
                );

                var detalleTransferencia = new DetalleTransferenciaInventario
                {
                    InventarioId = inventarioOrigen.InventarioId, // Use source inventory ID
                    ArticuloId = inventarioOrigen.ArticuloId, // Use article ID from source
                    CantidadSolicitada = detalle.CantidadSolicitada,
                    CantidadDisponible = inventarioOrigen.Cantidad,
                    CantidadTransferida = null, // Will be set when transfer is executed
                    FueTransferido = null, // Will be set when transfer is executed
                    Notas = detalle.Notas,
                };

                transferencia.Detalles.Add(detalleTransferencia);
            }

            _context.TransferenciasInventario.Add(transferencia);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var transferDto_result = MapToTransferenciaDto(transferencia);

            _logger.LogInformation(
                "Transfer {TransferNumber} created by user {UserId} with {ItemCount} items",
                transferNumber,
                userId,
                transferDto.Detalles.Count
            );

            return ApiResponse<TransferenciaInventarioDto>.Success(
                transferDto_result,
                "Transfer created successfully"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error creating transfer in institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<TransferenciaInventarioDto>.Failure(
                "Error creating transfer",
                "An error occurred while creating the transfer"
            );
        }
    }

    public async Task<ApiResponse<List<TransferenciaInventarioDto>>> CreateBatchTransfersAsync(
        TransferenciaBatchCreateDto batchDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var createdTransfers = new List<TransferenciaInventarioDto>();
            var errors = new List<string>();

            for (int i = 0; i < batchDto.Transferencias.Count; i++)
            {
                try
                {
                    var transferDto = batchDto.Transferencias[i];
                    var result = await CreateSingleTransferInBatch(
                        transferDto,
                        institucionId,
                        userId,
                        ipAddress,
                        i + 1,
                        cancellationToken
                    );

                    if (result.IsSuccess)
                    {
                        createdTransfers.Add(result.Data!);
                    }
                    else
                    {
                        errors.AddRange(result.Errors.Select(e => $"Transfer {i + 1}: {e}"));
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Transfer {i + 1}: {ex.Message}");
                }
            }

            if (errors.Any())
            {
                await transaction.RollbackAsync(cancellationToken);
                return ApiResponse<List<TransferenciaInventarioDto>>.Failure(
                    "Batch transfer failed",
                    errors.First()
                );
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var message = errors.Any()
                ? $"Batch completed with {createdTransfers.Count} successes and {errors.Count} errors"
                : $"All {createdTransfers.Count} transfers created successfully";

            var response = ApiResponse<List<TransferenciaInventarioDto>>.Success(
                createdTransfers,
                message
            );
            if (errors.Any())
            {
                response.Errors.AddRange(errors);
            }

            _logger.LogInformation(
                "Batch transfer completed: {SuccessCount} created, {ErrorCount} errors",
                createdTransfers.Count,
                errors.Count
            );

            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error creating batch transfers in institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<List<TransferenciaInventarioDto>>.Failure(
                "Error creating batch transfers",
                "An error occurred while creating the batch transfers"
            );
        }
    }

    public async Task<ApiResponse<PagedResult<TransferenciaInventarioDto>>> GetTransfersAsync(
        int institucionId,
        TransferenciaInventarioFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .TransferenciasInventario.AsNoTracking()
                .Include(t => t.Detalles)
                .ThenInclude(d => d.Articulo)
                .Include(t => t.CreadoPor)
                .Include(t => t.AprobadoPor)
                .Include(t => t.CompletadoPor)
                .Where(t => t.InstitucionID == institucionId);

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Estado))
            {
                query = query.Where(t => t.Estado == filter.Estado);
            }

            if (!string.IsNullOrEmpty(filter.Prioridad))
            {
                query = query.Where(t => t.Prioridad == filter.Prioridad);
            }

            if (filter.TipoUbicacionOrigen.HasValue)
            {
                query = query.Where(t => t.TipoUbicacionOrigen == filter.TipoUbicacionOrigen.Value);
            }

            if (filter.TipoUbicacionDestino.HasValue)
            {
                query = query.Where(t =>
                    t.TipoUbicacionDestino == filter.TipoUbicacionDestino.Value
                );
            }

            if (filter.UbicacionOrigenId.HasValue)
            {
                query = query.Where(t => t.UbicacionIdOrigen == filter.UbicacionOrigenId.Value);
            }

            if (filter.UbicacionDestinoId.HasValue)
            {
                query = query.Where(t => t.UbicacionIdDestino == filter.UbicacionDestinoId.Value);
            }

            if (filter.FechaDesde.HasValue)
            {
                query = query.Where(t => t.FechaCreacion >= filter.FechaDesde.Value);
            }

            if (filter.FechaHasta.HasValue)
            {
                query = query.Where(t => t.FechaCreacion <= filter.FechaHasta.Value);
            }

            if (!string.IsNullOrEmpty(filter.UsuarioCreacion))
            {
                query = query.Where(t => t.UsuarioCreacion == filter.UsuarioCreacion);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = filter.OrdenarPor?.ToLower() switch
            {
                "fecha" => filter.Descendente
                    ? query.OrderByDescending(t => t.FechaCreacion)
                    : query.OrderBy(t => t.FechaCreacion),
                "estado" => filter.Descendente
                    ? query.OrderByDescending(t => t.Estado)
                    : query.OrderBy(t => t.Estado),
                "prioridad" => filter.Descendente
                    ? query.OrderByDescending(t => t.Prioridad)
                    : query.OrderBy(t => t.Prioridad),
                "numero" => filter.Descendente
                    ? query.OrderByDescending(t => t.NumeroTransferencia)
                    : query.OrderBy(t => t.NumeroTransferencia),
                _ => query.OrderByDescending(t => t.FechaCreacion),
            };

            // Apply pagination
            var transferencias = await query
                .Skip((filter.Pagina - 1) * filter.TamanoPagina)
                .Take(filter.TamanoPagina)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var transferenciaDtos = new List<TransferenciaInventarioDto>();
            foreach (var transferencia in transferencias)
            {
                transferenciaDtos.Add(MapToTransferenciaDto(transferencia));
            }

            var result = new PagedResult<TransferenciaInventarioDto>
            {
                Items = transferenciaDtos,
                TotalCount = totalCount,
                Page = filter.Pagina,
                PageSize = filter.TamanoPagina,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.TamanoPagina),
            };

            return ApiResponse<PagedResult<TransferenciaInventarioDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving transfers for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<PagedResult<TransferenciaInventarioDto>>.Failure(
                "Error retrieving transfers",
                "An error occurred while retrieving the transfers"
            );
        }
    }

    public async Task<ApiResponse<TransferenciaInventarioDto>> ApproveTransferAsync(
        int transferId,
        TransferenciaApprovalDto approvalDto,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var transferencia = await _context
                .TransferenciasInventario.Include(t => t.Detalles)
                .ThenInclude(d => d.Articulo)
                .FirstOrDefaultAsync(
                    t => t.TransferenciaId == transferId && t.InstitucionID == institucionId,
                    cancellationToken
                );

            if (transferencia == null)
            {
                return ApiResponse<TransferenciaInventarioDto>.Failure("Transfer not found");
            }

            if (transferencia.Estado != EstadoTransferencia.Pendiente)
            {
                return ApiResponse<TransferenciaInventarioDto>.Failure(
                    $"Transfer is not pending approval. Current status: {transferencia.Estado}"
                );
            }

            var now = DateTime.UtcNow;

            if (approvalDto.Approved)
            {
                transferencia.Estado = EstadoTransferencia.Aprobada;
                transferencia.FechaAprobacion = now;
                transferencia.UsuarioAprobacion = userId;
                transferencia.ComentariosAprobacion = approvalDto.Comments;
            }
            else
            {
                transferencia.Estado = EstadoTransferencia.Rechazada;
                transferencia.FechaRechazo = now;
                transferencia.UsuarioRechazo = userId;
                transferencia.MotivoRechazo = approvalDto.Comments ?? "No reason provided";
            }

            await _context.SaveChangesAsync(cancellationToken);

            var transferDto = MapToTransferenciaDto(transferencia);

            _logger.LogInformation(
                "Transfer {TransferNumber} {Action} by user {UserId}",
                transferencia.NumeroTransferencia,
                approvalDto.Approved ? "approved" : "rejected",
                userId
            );

            return ApiResponse<TransferenciaInventarioDto>.Success(
                transferDto,
                $"Transfer {(approvalDto.Approved ? "approved" : "rejected")} successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error approving transfer {TransferId} in institution {InstitucionId}",
                transferId,
                institucionId
            );
            return ApiResponse<TransferenciaInventarioDto>.Failure(
                "Error processing approval",
                "An error occurred while processing the transfer approval"
            );
        }
    }

    public async Task<ApiResponse<TransferenciaInventarioDto>> GetTransferByIdAsync(
        int transferId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var transferencia = await _context
                .TransferenciasInventario.AsNoTracking()
                .Include(t => t.Detalles)
                .ThenInclude(d => d.Articulo)
                .Include(t => t.CreadoPor)
                .Include(t => t.AprobadoPor)
                .Include(t => t.CompletadoPor)
                .FirstOrDefaultAsync(
                    t => t.TransferenciaId == transferId && t.InstitucionID == institucionId,
                    cancellationToken
                );

            if (transferencia == null)
            {
                return ApiResponse<TransferenciaInventarioDto>.Failure("Transfer not found");
            }

            var transferDto = MapToTransferenciaDto(transferencia);
            return ApiResponse<TransferenciaInventarioDto>.Success(transferDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving transfer {TransferId} in institution {InstitucionId}",
                transferId,
                institucionId
            );
            return ApiResponse<TransferenciaInventarioDto>.Failure(
                "Error retrieving transfer",
                "An error occurred while retrieving the transfer details"
            );
        }
    }

    public async Task<ApiResponse<TransferenciaInventarioDto>> ExecuteTransferAsync(
        int transferId,
        TransferenciaExecutionDto executionDto,
        int institucionId,
        string userId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var transferencia = await _context
                .TransferenciasInventario.Include(t => t.Detalles)
                .ThenInclude(d => d.Inventario)
                .ThenInclude(i => i!.Articulo)
                .FirstOrDefaultAsync(
                    t => t.TransferenciaId == transferId && t.InstitucionID == institucionId,
                    cancellationToken
                );

            if (transferencia == null)
            {
                return ApiResponse<TransferenciaInventarioDto>.Failure("Transfer not found");
            }

            if (transferencia.Estado != EstadoTransferencia.Aprobada)
            {
                return ApiResponse<TransferenciaInventarioDto>.Failure(
                    $"Transfer is not approved. Current status: {transferencia.Estado}"
                );
            }

            transferencia.Estado = EstadoTransferencia.EnProceso;
            var completedItems = 0;
            var failedItems = 0;

            foreach (var detalle in transferencia.Detalles)
            {
                try
                {
                    // Get current source inventory
                    var inventarioOrigen = await _context.InventarioUnificado.FirstOrDefaultAsync(
                        i => i.InventarioId == detalle.InventarioId,
                        cancellationToken
                    );

                    if (
                        inventarioOrigen == null
                        || inventarioOrigen.Cantidad < detalle.CantidadSolicitada
                    )
                    {
                        detalle.MotivoFallo =
                            $"Insufficient inventory. Available: {inventarioOrigen?.Cantidad ?? 0}, Required: {detalle.CantidadSolicitada}";
                        failedItems++;
                        continue;
                    }

                    // Get destination inventory
                    var inventarioDestino = await _context.InventarioUnificado.FirstOrDefaultAsync(
                        i => i.InventarioId == detalle.InventarioId,
                        cancellationToken
                    );

                    if (inventarioDestino == null)
                    {
                        detalle.MotivoFallo = "Destination inventory not found";
                        failedItems++;
                        continue;
                    }

                    // Execute the transfer
                    var cantidadATransferir = detalle.CantidadSolicitada;

                    // Reduce source inventory
                    inventarioOrigen.Cantidad -= cantidadATransferir;
                    inventarioOrigen.FechaUltimaActualizacion = DateTime.UtcNow;

                    // Increase destination inventory
                    inventarioDestino.Cantidad += cantidadATransferir;
                    inventarioDestino.FechaUltimaActualizacion = DateTime.UtcNow;

                    // Update transfer detail
                    detalle.CantidadTransferida = cantidadATransferir;
                    detalle.FueTransferido = true;

                    // Create movement records
                    var metadata = new Dictionary<string, string>
                    {
                        { "TransferenciaId", transferId.ToString() },
                        { "NumeroTransferencia", transferencia.NumeroTransferencia },
                        { "TipoOperacion", "Transferencia" },
                        { "DireccionMovimiento", "Salida" }
                    };

                    // Source movement (outgoing)
                    await _movementService.CreateMovementAsync(
                        new MovimientoInventarioCreateDto
                        {
                            InventarioId = inventarioOrigen.InventarioId,
                            TipoMovimiento = "Transferencia",
                            CantidadAnterior = inventarioOrigen.Cantidad + cantidadATransferir,
                            CantidadNueva = inventarioOrigen.Cantidad,
                            CantidadCambiada = -cantidadATransferir,
                            Motivo =
                                $"Transferencia saliente - {transferencia.NumeroTransferencia}",
                            TransferenciaId = transferId,
                            Metadata = metadata,
                        },
                        institucionId,
                        userId,
                        ipAddress,
                        cancellationToken
                    );

                    // Destination movement (incoming)
                    await _movementService.CreateMovementAsync(
                        new MovimientoInventarioCreateDto
                        {
                            InventarioId = inventarioDestino.InventarioId,
                            TipoMovimiento = "Transferencia",
                            CantidadAnterior = inventarioDestino.Cantidad - cantidadATransferir,
                            CantidadNueva = inventarioDestino.Cantidad,
                            CantidadCambiada = cantidadATransferir,
                            Motivo =
                                $"Transferencia entrante - {transferencia.NumeroTransferencia}",
                            TransferenciaId = transferId,
                            Metadata = metadata,
                        },
                        institucionId,
                        userId,
                        ipAddress,
                        cancellationToken
                    );

                    completedItems++;
                }
                catch (Exception ex)
                {
                    detalle.MotivoFallo = $"Execution error: {ex.Message}";
                    failedItems++;
                    _logger.LogError(
                        ex,
                        "Error executing transfer detail for transfer {TransferId}",
                        transferId
                    );
                }
            }

            // Determine final status
            if (completedItems == transferencia.Detalles.Count)
            {
                transferencia.Estado = EstadoTransferencia.Completada;
            }
            else if (completedItems > 0)
            {
                transferencia.Estado = EstadoTransferencia.ParcialmenteCompletada;
            }
            else
            {
                transferencia.Estado = EstadoTransferencia.Rechazada;
            }

            transferencia.FechaCompletado = DateTime.UtcNow;
            transferencia.UsuarioCompletado = userId;
            transferencia.NotasCompletado =
                executionDto.Notes
                ?? $"Execution completed: {completedItems} successful, {failedItems} failed";

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var transferDto = MapToTransferenciaDto(transferencia);

            _logger.LogInformation(
                "Transfer {TransferNumber} executed: {CompletedItems} completed, {FailedItems} failed",
                transferencia.NumeroTransferencia,
                completedItems,
                failedItems
            );

            return ApiResponse<TransferenciaInventarioDto>.Success(
                transferDto,
                $"Transfer execution completed: {completedItems} successful, {failedItems} failed"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Error executing transfer {TransferId} in institution {InstitucionId}",
                transferId,
                institucionId
            );
            return ApiResponse<TransferenciaInventarioDto>.Failure(
                "Error executing transfer",
                "An error occurred while executing the transfer"
            );
        }
    }

    public async Task<ApiResponse<TransferenciaInventarioDto>> CancelTransferAsync(
        int transferId,
        string cancellationReason,
        int institucionId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var transferencia = await _context
                .TransferenciasInventario.Include(t => t.Detalles)
                .FirstOrDefaultAsync(
                    t => t.TransferenciaId == transferId && t.InstitucionID == institucionId,
                    cancellationToken
                );

            if (transferencia == null)
            {
                return ApiResponse<TransferenciaInventarioDto>.Failure("Transfer not found");
            }

            if (
                transferencia.Estado == EstadoTransferencia.Completada
                || transferencia.Estado == EstadoTransferencia.Cancelada
            )
            {
                return ApiResponse<TransferenciaInventarioDto>.Failure(
                    $"Cannot cancel transfer with status: {transferencia.Estado}"
                );
            }

            transferencia.Estado = EstadoTransferencia.Cancelada;
            transferencia.FechaRechazo = DateTime.UtcNow;
            transferencia.UsuarioRechazo = userId;
            transferencia.MotivoRechazo = cancellationReason;

            await _context.SaveChangesAsync(cancellationToken);

            var transferDto = MapToTransferenciaDto(transferencia);

            _logger.LogInformation(
                "Transfer {TransferNumber} cancelled by user {UserId}",
                transferencia.NumeroTransferencia,
                userId
            );

            return ApiResponse<TransferenciaInventarioDto>.Success(
                transferDto,
                "Transfer cancelled successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error cancelling transfer {TransferId} in institution {InstitucionId}",
                transferId,
                institucionId
            );
            return ApiResponse<TransferenciaInventarioDto>.Failure(
                "Error cancelling transfer",
                "An error occurred while cancelling the transfer"
            );
        }
    }

    public async Task<
        ApiResponse<PagedResult<TransferenciaInventarioDto>>
    > GetPendingApprovalsAsync(
        int institucionId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .TransferenciasInventario.AsNoTracking()
                .Include(t => t.Detalles)
                .ThenInclude(d => d.Articulo)
                .Include(t => t.CreadoPor)
                .Where(t =>
                    t.InstitucionID == institucionId
                    && t.Estado == EstadoTransferencia.Pendiente
                    && t.RequiereAprobacion
                )
                .OrderBy(t => t.FechaCreacion);

            var totalCount = await query.CountAsync(cancellationToken);

            var transferencias = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var transferenciaDtos = new List<TransferenciaInventarioDto>();
            foreach (var transferencia in transferencias)
            {
                transferenciaDtos.Add(MapToTransferenciaDto(transferencia));
            }

            var result = new PagedResult<TransferenciaInventarioDto>
            {
                Items = transferenciaDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            };

            return ApiResponse<PagedResult<TransferenciaInventarioDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving pending approvals for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<PagedResult<TransferenciaInventarioDto>>.Failure(
                "Error retrieving pending approvals",
                "An error occurred while retrieving pending transfer approvals"
            );
        }
    }

    public async Task<ApiResponse<TransferenciaEstadisticasDto>> GetTransferStatisticsAsync(
        int institucionId,
        TransferenciaEstadisticasFilterDto filter,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .TransferenciasInventario.AsNoTracking()
                .Where(t => t.InstitucionID == institucionId);

            // Apply date filters
            if (filter.FechaDesde.HasValue)
            {
                query = query.Where(t => t.FechaCreacion >= filter.FechaDesde.Value);
            }

            if (filter.FechaHasta.HasValue)
            {
                query = query.Where(t => t.FechaCreacion <= filter.FechaHasta.Value);
            }

            // Get statistics
            var totalTransferencias = await query.CountAsync(cancellationToken);

            var transferenciasPorEstado = await query
                .GroupBy(t => t.Estado)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .ToListAsync(cancellationToken);

            var transferenciasPorPrioridad = await query
                .GroupBy(t => t.Prioridad)
                .Select(g => new { Prioridad = g.Key, Cantidad = g.Count() })
                .ToListAsync(cancellationToken);

            var transferenciasPorDia = await query
                .Where(t => t.FechaCreacion >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(t => t.FechaCreacion.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
                .OrderBy(x => x.Fecha)
                .ToListAsync(cancellationToken);

            var tiempoPromedioAprobacion = await CalculateAverageApprovalTime(
                query,
                cancellationToken
            );
            var tasaAprobacion = await CalculateApprovalRate(query, cancellationToken);

            var estadisticas = new TransferenciaEstadisticasDto
            {
                TotalTransferencias = totalTransferencias,
                TransferenciasPorEstado = transferenciasPorEstado.ToDictionary(
                    x => x.Estado,
                    x => x.Cantidad
                ),
                TransferenciasPorPrioridad = transferenciasPorPrioridad.ToDictionary(
                    x => x.Prioridad,
                    x => x.Cantidad
                ),
                TransferenciasPorDia = transferenciasPorDia.ToDictionary(
                    x => x.Fecha,
                    x => x.Cantidad
                ),
                TiempoPromedioAprobacionHoras = tiempoPromedioAprobacion,
                TasaAprobacionPorcentaje = tasaAprobacion,
                TransferenciasPendientes =
                    transferenciasPorEstado
                        .FirstOrDefault(x => x.Estado == EstadoTransferencia.Pendiente)
                        ?.Cantidad ?? 0,
                TransferenciasCompletadas =
                    transferenciasPorEstado
                        .FirstOrDefault(x => x.Estado == EstadoTransferencia.Completada)
                        ?.Cantidad ?? 0,
            };

            return ApiResponse<TransferenciaEstadisticasDto>.Success(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving transfer statistics for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<TransferenciaEstadisticasDto>.Failure(
                "Error retrieving statistics",
                "An error occurred while retrieving transfer statistics"
            );
        }
    }

    #region Private Methods

    private async Task<string> GenerateTransferNumber(
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        var prefix = $"TRF-{institucionId:D4}-{DateTime.UtcNow:yyyyMM}";

        var lastTransfer = await _context
            .TransferenciasInventario.Where(t =>
                t.InstitucionID == institucionId && t.NumeroTransferencia.StartsWith(prefix)
            )
            .OrderByDescending(t => t.NumeroTransferencia)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastTransfer != null)
        {
            var lastSequence = lastTransfer.NumeroTransferencia.Split('-').LastOrDefault();
            if (int.TryParse(lastSequence, out int lastNum))
            {
                sequence = lastNum + 1;
            }
        }

        return $"{prefix}-{sequence:D4}";
    }

    private async Task<InventarioUnificado> FindOrCreateDestinationInventory(
        int articuloId,
        int tipoUbicacion,
        int ubicacionId,
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        var existing = await _context.InventarioUnificado.FirstOrDefaultAsync(
            i =>
                i.ArticuloId == articuloId
                && i.TipoUbicacion == tipoUbicacion
                && i.UbicacionId == ubicacionId
                && i.InstitucionID == institucionId,
            cancellationToken
        );

        if (existing != null)
        {
            return existing;
        }

        // Create new inventory entry
        var newInventory = new InventarioUnificado
        {
            ArticuloId = articuloId,
            InstitucionID = institucionId,
            TipoUbicacion = tipoUbicacion,
            UbicacionId = ubicacionId,
            Cantidad = 0,
            FechaRegistro = DateTime.UtcNow,
            FechaUltimaActualizacion = DateTime.UtcNow,
        };

        _context.InventarioUnificado.Add(newInventory);
        await _context.SaveChangesAsync(cancellationToken);

        return newInventory;
    }

    private async Task<ApiResponse<TransferenciaInventarioDto>> CreateSingleTransferInBatch(
        TransferenciaCreateDto transferDto,
        int institucionId,
        string userId,
        string? ipAddress,
        int batchIndex,
        CancellationToken cancellationToken
    )
    {
        // This is a simplified version for batch processing
        // In a real implementation, you might want to avoid recursive calls
        return await CreateTransferAsync(
            transferDto,
            institucionId,
            userId,
            ipAddress,
            cancellationToken
        );
    }

    private TransferenciaInventarioDto MapToTransferenciaDto(TransferenciaInventario transferencia)
    {
        var dto = new TransferenciaInventarioDto
        {
            TransferenciaId = transferencia.TransferenciaId,
            NumeroTransferencia = transferencia.NumeroTransferencia,
            TipoUbicacionOrigen = transferencia.TipoUbicacionOrigen,
            UbicacionIdOrigen = transferencia.UbicacionIdOrigen,
            TipoUbicacionDestino = transferencia.TipoUbicacionDestino,
            UbicacionIdDestino = transferencia.UbicacionIdDestino,
            Estado = transferencia.Estado,
            Prioridad = transferencia.Prioridad,
            Motivo = transferencia.Motivo,
            Notas = transferencia.Notas,
            RequiereAprobacion = transferencia.RequiereAprobacion,
            FechaCreacion = transferencia.FechaCreacion,
            FechaAprobacion = transferencia.FechaAprobacion,
            FechaRechazo = transferencia.FechaRechazo,
            FechaCompletado = transferencia.FechaCompletado,
            UsuarioNombreCreacion = transferencia.CreadoPor?.UserName ?? "Usuario Desconocido",
            UsuarioNombreAprobacion = transferencia.AprobadoPor?.UserName,
            UsuarioNombreCompletado = transferencia.CompletadoPor?.UserName,
            ComentariosAprobacion = transferencia.ComentariosAprobacion,
            MotivoRechazo = transferencia.MotivoRechazo,
            NotasCompletado = transferencia.NotasCompletado,
            UbicacionNombreOrigen = GetUbicacionName(
                transferencia.TipoUbicacionOrigen,
                transferencia.UbicacionIdOrigen ?? 0
            ),
            UbicacionNombreDestino = GetUbicacionName(
                transferencia.TipoUbicacionDestino,
                transferencia.UbicacionIdDestino ?? 0
            ),
            Detalles =
                transferencia
                    .Detalles?.Select(d => new DetalleTransferenciaDto
                    {
                        DetalleId = d.DetalleId,
                        ArticuloId = d.ArticuloId,
                        ArticuloNombre = d.Articulo?.NombreArticulo ?? "Artículo Desconocido",
                        CantidadSolicitada = d.CantidadSolicitada,
                        CantidadTransferida = d.CantidadTransferida,
                        FueTransferido = d.FueTransferido,
                        Notas = d.Notas,
                        MotivoFallo = d.MotivoFallo,
                    })
                    .ToList() ?? new List<DetalleTransferenciaDto>(),
        };

        return dto;
    }

    private string GetUbicacionName(int tipoUbicacion, int ubicacionId)
    {
        return tipoUbicacion switch
        {
            0 => "Inventario General",
            1 => $"Habitación {ubicacionId}",
            2 => $"Almacén {ubicacionId}",
            _ => "Ubicación Desconocida",
        };
    }

    private async Task<double> CalculateAverageApprovalTime(
        IQueryable<TransferenciaInventario> query,
        CancellationToken cancellationToken
    )
    {
        var approvedTransfers = await query
            .Where(t => t.FechaAprobacion.HasValue)
            .Select(t => new { Created = t.FechaCreacion, Approved = t.FechaAprobacion!.Value })
            .ToListAsync(cancellationToken);

        if (!approvedTransfers.Any())
            return 0;

        return approvedTransfers.Select(t => (t.Approved - t.Created).TotalHours).Average();
    }

    private async Task<double> CalculateApprovalRate(
        IQueryable<TransferenciaInventario> query,
        CancellationToken cancellationToken
    )
    {
        var totalRequiringApproval = await query.CountAsync(
            t => t.RequiereAprobacion,
            cancellationToken
        );

        if (totalRequiringApproval == 0)
            return 0;

        var approvedCount = await query.CountAsync(
            t => t.RequiereAprobacion && t.Estado == EstadoTransferencia.Aprobada,
            cancellationToken
        );

        return (double)approvedCount / totalRequiringApproval * 100;
    }

    #endregion
}
