using hotel.Data;
using hotel.DTOs.Caja;
using hotel.DTOs.Common;
using hotel.Interfaces;
using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service for cash register (caja) operations
/// </summary>
public class CajaService : ICajaService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<CajaService> _logger;
    private readonly ICajaDtoMapper _mapper;

    public CajaService(HotelDbContext context, ILogger<CajaService> logger, ICajaDtoMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    #region Common Query Builders

    /// <summary>
    /// Internal type for base payment query results
    /// </summary>
    private class PagoQueryResult
    {
        public Pagos Pago { get; set; } = null!;
        public Tarjetas? Tarjeta { get; set; }
        public Empeño? Empeno { get; set; }
        public Movimientos? Movimiento { get; set; }
        public Visitas? Visita { get; set; }
        public Reservas? Reserva { get; set; }
        public Habitaciones? Habitacion { get; set; }
        public CategoriasHabitaciones? Categoria { get; set; }
    }

    /// <summary>
    /// Base query for payments with all related data - reusable across multiple methods
    /// </summary>
    private IQueryable<PagoQueryResult> BuildPagosBaseQuery(
        IQueryable<Pagos> pagosSource,
        int institucionId
    )
    {
        return from pago in pagosSource
            where pago.InstitucionID == institucionId
            join tarjeta in _context.Tarjetas.AsNoTracking()
                on pago.TarjetaId equals tarjeta.TarjetaID
                into tarjetas
            from tarjeta in tarjetas.DefaultIfEmpty()
            join empeno in _context.Empeño.AsNoTracking()
                on pago.PagoId equals empeno.PagoID
                into empenos
            from empeno in empenos.DefaultIfEmpty()
            join movimiento in _context.Movimientos.AsNoTracking()
                on pago.PagoId equals movimiento.PagoId
                into movimientos
            from movimiento in movimientos.DefaultIfEmpty()
            join visita in _context.Visitas.AsNoTracking()
                on movimiento.VisitaId equals visita.VisitaId
                into visitas
            from visita in visitas.DefaultIfEmpty()
            join reserva in _context.Reservas.AsNoTracking()
                on visita.VisitaId equals reserva.VisitaId
                into reservas
            from reserva in reservas.DefaultIfEmpty()
            join habitacion in _context.Habitaciones.AsNoTracking()
                on reserva.HabitacionId equals habitacion.HabitacionId
                into habitaciones
            from habitacion in habitaciones.DefaultIfEmpty()
            join categoria in _context.CategoriasHabitaciones.AsNoTracking()
                on habitacion.CategoriaId equals categoria.CategoriaId
                into categorias
            from categoria in categorias.DefaultIfEmpty()
            select new PagoQueryResult
            {
                Pago = pago,
                Tarjeta = tarjeta,
                Empeno = empeno,
                Movimiento = movimiento,
                Visita = visita,
                Reserva = reserva,
                Habitacion = habitacion,
                Categoria = categoria,
            };
    }

    /// <summary>
    /// Gets detailed payment information using optimized base query
    /// </summary>
    private async Task<List<PagoDetalladoDto>> GetPagosDetalladosOptimizedAsync(
        IQueryable<Pagos> pagosSource,
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        var baseQuery = BuildPagosBaseQuery(pagosSource, institucionId);

        var pagosQuery = baseQuery.Select(x => new PagoDetalladoDto
        {
            PagoId = x.Pago.PagoId,
            Fecha = x.Pago.fechaHora,
            MontoEfectivo = x.Pago.MontoEfectivo ?? 0,
            MontoTarjeta = x.Pago.MontoTarjeta ?? 0,
            MontoBillVirt = x.Pago.MontoBillVirt ?? 0,
            MontoDescuento = x.Pago.MontoDescuento ?? 0,
            MontoAdicional = x.Pago.Adicional ?? 0,
            Observacion = x.Pago.Observacion,
            TipoTransaccion = x.Empeno != null ? "Empeño" : "Habitación",
            TarjetaNombre = x.Tarjeta != null ? x.Tarjeta.Nombre : null,
            CategoriaNombre = x.Categoria != null ? x.Categoria.NombreCategoria : null,
            TipoHabitacion = x.Habitacion != null ? x.Habitacion.NombreHabitacion : null,
            HabitacionId = x.Habitacion != null ? x.Habitacion.HabitacionId : null,
            HoraIngreso = x.Reserva != null ? x.Reserva.FechaReserva : null,
            HoraSalida = x.Pago.fechaHora,
            Periodo = x.Movimiento != null ? x.Movimiento.TotalFacturado : 0,
            TotalConsumo = 0, // Will be calculated separately
        });

        var pagos = await pagosQuery.ToListAsync(cancellationToken);
        await EnrichWithConsumptionTotalsAsync(pagos, cancellationToken);
        return pagos;
    }

    /// <summary>
    /// Gets complete payment details using optimized base query
    /// </summary>
    private async Task<List<PagoDetalleCompletoDto>> GetPagosDetalleCompletoOptimizedAsync(
        IQueryable<Pagos> pagosSource,
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        var baseQuery = BuildPagosBaseQuery(pagosSource, institucionId);

        var pagosQuery = baseQuery.Select(x => new PagoDetalleCompletoDto
        {
            PagoId = x.Pago.PagoId,
            Fecha = x.Pago.fechaHora,
            CategoriaNombre = x.Categoria != null ? x.Categoria.NombreCategoria : null,
            Periodo = x.Movimiento != null ? (x.Movimiento.TotalFacturado ?? 0) : 0,
            TarjetaNombre = x.Tarjeta != null ? x.Tarjeta.Nombre : null,
            HoraIngreso = x.Reserva != null ? x.Reserva.FechaReserva : null,
            HoraSalida = x.Pago.fechaHora,
            TotalConsumo = 0, // Will be calculated separately
            MontoAdicional = x.Pago.Adicional ?? 0,
            MontoEfectivo = x.Pago.MontoEfectivo,
            MontoTarjeta = x.Pago.MontoTarjeta,
            MontoBillVirt = x.Pago.MontoBillVirt,
            MontoDescuento = x.Pago.MontoDescuento,
            Observacion = x.Pago.Observacion,
            TipoHabitacion = x.Habitacion != null ? x.Habitacion.NombreHabitacion : null,
            TipoTransaccion = x.Empeno != null ? "Empeño" : "Habitación",
        });

        var pagos = await pagosQuery.ToListAsync(cancellationToken);
        await EnrichWithConsumptionTotalsAsync(pagos, cancellationToken);
        return pagos;
    }

    /// <summary>
    /// Gets pending transactions using optimized base query
    /// </summary>
    private async Task<List<TransaccionPendienteDto>> GetTransaccionesPendientesOptimizedAsync(
        IQueryable<Pagos> pagosSource,
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        var baseQuery = BuildPagosBaseQuery(pagosSource, institucionId);

        var pagosQuery = baseQuery.Select(x => new TransaccionPendienteDto
        {
            PagoId = x.Pago.PagoId,
            HabitacionId = x.Habitacion != null ? x.Habitacion.HabitacionId : null,
            TarjetaNombre = x.Tarjeta != null ? x.Tarjeta.Nombre : null,
            Periodo = x.Movimiento != null ? (x.Movimiento.TotalFacturado ?? 0) : 0,
            CategoriaNombre = x.Categoria != null ? x.Categoria.NombreCategoria : null,
            Fecha = x.Pago.fechaHora,
            HoraIngreso = x.Reserva != null ? x.Reserva.FechaReserva : null,
            HoraSalida = x.Pago.fechaHora,
            MontoAdicional = x.Pago.Adicional ?? 0,
            TotalConsumo = 0, // Will be calculated separately
            MontoEfectivo = x.Pago.MontoEfectivo,
            MontoTarjeta = x.Pago.MontoTarjeta,
            MontoBillVirt = x.Pago.MontoBillVirt,
            MontoDescuento = x.Pago.MontoDescuento,
            Observacion = x.Pago.Observacion,
            TipoHabitacion = x.Habitacion != null ? x.Habitacion.NombreHabitacion : null,
            TipoTransaccion = x.Empeno != null ? "Empeño" : "Habitación",
        });

        var pagos = await pagosQuery.ToListAsync(cancellationToken);
        await EnrichWithConsumptionTotalsAsync(pagos, cancellationToken);
        return pagos;
    }

    /// <summary>
    /// Enriches payment DTOs with consumption totals - reusable across all payment queries
    /// </summary>
    private async Task EnrichWithConsumptionTotalsAsync<T>(
        List<T> payments,
        CancellationToken cancellationToken
    )
        where T : class
    {
        if (!payments.Any())
            return;

        // Get PagoId from the payment objects using reflection to handle different DTO types
        var pagoIdProperty = typeof(T).GetProperty("PagoId");
        var totalConsumoProperty = typeof(T).GetProperty("TotalConsumo");

        if (pagoIdProperty == null || totalConsumoProperty == null)
            return;

        var pagoIds = payments
            .Select(p => (int)(pagoIdProperty.GetValue(p) ?? 0))
            .Where(id => id > 0)
            .ToList();

        if (!pagoIds.Any())
            return;

        var consumos = await (
            from c in _context.Consumo.AsNoTracking()
            join m in _context.Movimientos.AsNoTracking() on c.MovimientosId equals m.MovimientosId
            where pagoIds.Contains(m.PagoId ?? 0)
            group c by m.PagoId into g
            select new { PagoId = g.Key, Total = g.Sum(x => x.PrecioUnitario) }
        ).ToListAsync(cancellationToken);

        // Apply consumption totals to payments
        foreach (var payment in payments)
        {
            var pagoId = (int)(pagoIdProperty.GetValue(payment) ?? 0);
            var consumo = consumos.FirstOrDefault(c => c.PagoId == pagoId);
            if (consumo != null)
            {
                totalConsumoProperty.SetValue(payment, consumo.Total ?? 0);
            }
        }
    }

    /// <summary>
    /// Common error handling wrapper for service methods
    /// </summary>
    private async Task<ApiResponse<T>> ExecuteWithErrorHandlingAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        int? institucionId = null,
        object? additionalContext = null
    )
    {
        try
        {
            var result = await operation();
            return ApiResponse<T>.Success(result);
        }
        catch (Exception ex)
        {
            var contextInfo = institucionId.HasValue
                ? $" for institution {institucionId}"
                : string.Empty;

            _logger.LogError(ex, "Error in {OperationName}{Context}", operationName, contextInfo);

            return ApiResponse<T>.Failure(
                $"Error en {operationName}",
                "Ocurrió un error interno. Por favor, inténtelo nuevamente."
            );
        }
    }

    #endregion

    /// <summary>
    /// Creates a new cash register entry
    /// </summary>
    public async Task<ApiResponse<CajaDto>> CrearCajaAsync(
        CrearCajaDto crearCajaDto,
        int institucionId,
        string? userId = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Creating new cash register for institution {InstitucionId} with initial amount {MontoInicial}",
                institucionId,
                crearCajaDto.MontoInicial
            );

            // Create new Cierre entity
            var nuevoCierre = new Cierre
            {
                MontoInicialCaja = crearCajaDto.MontoInicial,
                Observaciones = crearCajaDto.Observacion,
                TotalIngresosBillVirt = 0,
                TotalIngresosEfectivo = 0,
                TotalIngresosTarjeta = 0,
                EstadoCierre = false,
                InstitucionID = institucionId,
                UserId = userId, // Set AspNetUsers ID
                FechaHoraCierre = null, // Will be set when closing the cash register
            };

            _context.Cierre.Add(nuevoCierre);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cash register created successfully with ID {CierreId} for institution {InstitucionId}",
                nuevoCierre.CierreId,
                institucionId
            );

            // Map to DTO using dedicated mapper
            var cajaDto = _mapper.MapToCajaDto(nuevoCierre);

            return ApiResponse<CajaDto>.Success(cajaDto, "Caja creada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating cash register for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<CajaDto>.Failure(
                "Error al crear la caja",
                "Ocurrió un error interno al crear la caja. Por favor, inténtelo nuevamente."
            );
        }
    }

    /// <summary>
    /// Gets all cash register closures for an institution
    /// </summary>
    public async Task<ApiResponse<IEnumerable<CajaDetalladaDto>>> GetCierresAsync(
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteWithErrorHandlingAsync(
            async () =>
            {
                _logger.LogInformation(
                    "Retrieving cash register closures for institution {InstitucionId}",
                    institucionId
                );

                var cierres = await _context
                    .Cierre.AsNoTracking()
                    .Where(c => c.InstitucionID == institucionId)
                    .Include(c => c.Pagos)
                    .Include(c => c.User) // Use AspNetUsers instead of legacy Usuario
                    .OrderByDescending(c => c.FechaHoraCierre ?? DateTime.MinValue)
                    .ToListAsync(cancellationToken);

                var cierresDto = cierres.Select(_mapper.MapToCajaDetalladaDto).ToList();

                _logger.LogInformation(
                    "Retrieved {Count} cash register closures for institution {InstitucionId}",
                    cierresDto.Count,
                    institucionId
                );

                return (IEnumerable<CajaDetalladaDto>)cierresDto;
            },
            "obtener los cierres de caja",
            institucionId
        );
    }

    /// <summary>
    /// Gets cash register closures with detailed payment information using optimized queries
    /// </summary>
    public async Task<ApiResponse<CierresConPagosPaginadosDto>> GetCierresConPagosAsync(
        int institucionId,
        CierresConPagosFiltroDto filtro,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Retrieving cash closures with payments for institution {InstitucionId} with filters",
                institucionId
            );

            // Build base query for closures with filters
            var cierresQuery = _context
                .Cierre.AsNoTracking()
                .Where(c => c.InstitucionID == institucionId && c.FechaHoraCierre != null);

            // Apply date filters
            if (filtro.FechaDesde.HasValue)
                cierresQuery = cierresQuery.Where(c =>
                    c.FechaHoraCierre >= filtro.FechaDesde.Value
                );

            if (filtro.FechaHasta.HasValue)
                cierresQuery = cierresQuery.Where(c =>
                    c.FechaHoraCierre <= filtro.FechaHasta.Value
                );

            if (filtro.EstadoCierre.HasValue)
                cierresQuery = cierresQuery.Where(c => c.EstadoCierre == filtro.EstadoCierre.Value);

            if (filtro.SoloConPagos)
                cierresQuery = cierresQuery.Where(c => c.Pagos.Any());

            // Get total count for pagination
            var totalCount = await cierresQuery.CountAsync(cancellationToken);

            // Apply pagination and ordering
            var cierres = await cierresQuery
                .OrderByDescending(c => c.FechaHoraCierre)
                .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .Include(c => c.Pagos)
                .ToListAsync(cancellationToken);

            // Get closure IDs for efficient payment details query
            var cierreIds = cierres.Select(c => c.CierreId).ToList();

            // Get payment details with all related data in a single optimized query
            var pagosConDetalles = filtro.IncluirDetallePagos
                ? await GetPagosDetalladosAsync(cierreIds, institucionId, cancellationToken)
                : new List<PagoDetalladoDto>();

            // Get pending payments (not associated with any closure)
            var pagosSinCierre = await GetPagosSinCierreAsync(institucionId, cancellationToken);

            // Map to DTOs
            var cierresDto = cierres
                .Select(cierre => new CierreConPagosDto
                {
                    CierreId = cierre.CierreId,
                    FechaHoraCierre = cierre.FechaHoraCierre,
                    TotalIngresosEfectivo = cierre.TotalIngresosEfectivo,
                    TotalIngresosBillVirt = cierre.TotalIngresosBillVirt,
                    TotalIngresosTarjeta = cierre.TotalIngresosTarjeta,
                    Observaciones = cierre.Observaciones,
                    EstadoCierre = cierre.EstadoCierre,
                    MontoInicialCaja = cierre.MontoInicialCaja,
                    Pagos = pagosConDetalles
                        .Where(p => p.PagoId != 0 && cierre.Pagos.Any(cp => cp.PagoId == p.PagoId))
                        .ToList(),
                })
                .ToList();

            var result = new CierresConPagosPaginadosDto
            {
                Data = new CierresConPagosDto
                {
                    Cierres = cierresDto,
                    PagosSinCierre = pagosSinCierre,
                },
                Pagination = new PaginationDto
                {
                    PageNumber = filtro.PageNumber,
                    PageSize = filtro.PageSize,
                    TotalRecords = totalCount,
                },
            };

            _logger.LogInformation(
                "Retrieved {Count} cash closures with payments for institution {InstitucionId} (Page {Page})",
                cierresDto.Count,
                institucionId,
                filtro.PageNumber
            );

            return ApiResponse<CierresConPagosPaginadosDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving cash closures with payments for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<CierresConPagosPaginadosDto>.Failure(
                "Error al obtener los cierres con pagos",
                "Ocurrió un error interno al obtener los cierres con pagos. Por favor, inténtelo nuevamente."
            );
        }
    }

    /// <summary>
    /// Gets detailed payment information with business context using optimized queries
    /// </summary>
    private async Task<List<PagoDetalladoDto>> GetPagosDetalladosAsync(
        List<int> cierreIds,
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        var pagosSource = _context
            .Pagos.AsNoTracking()
            .Where(p => cierreIds.Contains(p.CierreId ?? 0));

        return await GetPagosDetalladosOptimizedAsync(
            pagosSource,
            institucionId,
            cancellationToken
        );
    }

    /// <summary>
    /// Gets payments that haven't been included in any closure yet
    /// </summary>
    private async Task<List<PagoDetalladoDto>> GetPagosSinCierreAsync(
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        var pagosSource = _context.Pagos.AsNoTracking().Where(p => p.CierreId == null);

        return await GetPagosDetalladosOptimizedAsync(
            pagosSource,
            institucionId,
            cancellationToken
        );
    }

    /// <summary>
    /// Gets a specific cash register closure by ID with all related information
    /// </summary>
    public async Task<ApiResponse<CajaDetalladaDto>> GetCierreByIdAsync(
        int cierreId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteWithErrorHandlingAsync(
            async () =>
            {
                _logger.LogInformation(
                    "Retrieving cash register closure {CierreId} for institution {InstitucionId}",
                    cierreId,
                    institucionId
                );

                var cierre = await _context
                    .Cierre.AsNoTracking()
                    .Where(c => c.CierreId == cierreId && c.InstitucionID == institucionId)
                    .Include(c => c.Pagos)
                    .Include(c => c.User) // Use AspNetUsers instead of legacy Usuario
                    .FirstOrDefaultAsync(cancellationToken);

                if (cierre == null)
                {
                    _logger.LogWarning(
                        "Cash register closure {CierreId} not found for institution {InstitucionId}",
                        cierreId,
                        institucionId
                    );
                    throw new InvalidOperationException("Cierre no encontrado");
                }

                var cierreDto = _mapper.MapToCajaDetalladaDto(cierre);

                _logger.LogInformation(
                    "Retrieved cash register closure {CierreId} for institution {InstitucionId}",
                    cierreId,
                    institucionId
                );

                return cierreDto;
            },
            "obtener el cierre de caja",
            institucionId,
            new { cierreId }
        );
    }

    /// <summary>
    /// Gets complete details of a cash register closure including payments, cancellations, and expenses
    /// </summary>
    public async Task<ApiResponse<CierreDetalleCompletoDto>> GetCierreDetalleAsync(
        int cierreId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Retrieving complete details for cash register closure {CierreId} for institution {InstitucionId}",
                cierreId,
                institucionId
            );

            // Get the main closure record
            var cierre = await _context
                .Cierre.AsNoTracking()
                .Where(c => c.CierreId == cierreId && c.InstitucionID == institucionId)
                .Include(c => c.Pagos)
                .FirstOrDefaultAsync(cancellationToken);

            if (cierre == null)
            {
                _logger.LogWarning(
                    "Cash register closure {CierreId} not found for institution {InstitucionId}",
                    cierreId,
                    institucionId
                );
                return ApiResponse<CierreDetalleCompletoDto>.Failure(
                    "Cierre no encontrado",
                    "No se encontró el cierre de caja especificado"
                );
            }

            // Get previous closure to determine time range for canceled reservations
            var cierreAnterior = await _context
                .Cierre.AsNoTracking()
                .Where(c =>
                    c.FechaHoraCierre < cierre.FechaHoraCierre && c.InstitucionID == institucionId
                )
                .OrderByDescending(c => c.FechaHoraCierre)
                .FirstOrDefaultAsync(cancellationToken);

            var fechaCierreAnterior = cierreAnterior?.FechaHoraCierre ?? DateTime.MinValue;

            // Get expenses for this closure
            var egresos = await _context
                .Egresos.AsNoTracking()
                .Where(e => e.CierreID == cierreId && e.InstitucionID == institucionId)
                .Include(e => e.TipoEgreso)
                .ToListAsync(cancellationToken);

            // Get canceled reservations in the time range
            var reservasAnuladas = await _context
                .Reservas.AsNoTracking()
                .Where(r =>
                    r.FechaAnula < cierre.FechaHoraCierre
                    && r.FechaAnula > fechaCierreAnterior
                    && r.InstitucionID == institucionId
                )
                .Include(r => r.Habitacion)
                .ThenInclude(h => h!.Categoria)
                .ToListAsync(cancellationToken);

            // Get payment details with optimized query
            var pagoIds = cierre.Pagos.Select(p => p.PagoId).ToList();
            var transacciones = new List<PagoDetalleCompletoDto>();

            if (pagoIds.Any())
            {
                // Get payment details with all related information in optimized queries
                var pagosConDetalle = await GetPagosDetalleCompletoAsync(
                    pagoIds,
                    institucionId,
                    cancellationToken
                );
                transacciones.AddRange(pagosConDetalle);
            }

            // Add canceled reservations as transactions using mapper
            foreach (var reserva in reservasAnuladas)
            {
                transacciones.Add(_mapper.MapReservaAnuladaToPagoDetalleCompleto(reserva));
            }

            // Map expenses using dedicated mapper
            var egresosDto = _mapper.MapToEgresoDetalleDtos(egresos);

            var resultado = new CierreDetalleCompletoDto
            {
                CierreId = cierre.CierreId,
                FechaHoraCierre = cierre.FechaHoraCierre,
                EstadoCierre = cierre.EstadoCierre ?? false,
                TotalIngresosEfectivo = cierre.TotalIngresosEfectivo,
                TotalIngresosBillVirt = cierre.TotalIngresosBillVirt,
                TotalIngresosTarjeta = cierre.TotalIngresosTarjeta,
                MontoInicialCaja = cierre.MontoInicialCaja,
                Observaciones = cierre.Observaciones,
                InstitucionID = cierre.InstitucionID,
                Transacciones = transacciones.OrderBy(t => t.Fecha).ToList(),
                Egresos = egresosDto,
            };

            _logger.LogInformation(
                "Retrieved complete details for closure {CierreId}: {TransactionCount} transactions, {ExpenseCount} expenses",
                cierreId,
                transacciones.Count,
                egresosDto.Count
            );

            return ApiResponse<CierreDetalleCompletoDto>.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving complete details for cash register closure {CierreId} for institution {InstitucionId}",
                cierreId,
                institucionId
            );
            return ApiResponse<CierreDetalleCompletoDto>.Failure(
                "Error al obtener el detalle del cierre",
                "Ocurrió un error interno al obtener el detalle del cierre. Por favor, inténtelo nuevamente."
            );
        }
    }

    /// <summary>
    /// Gets complete payment details for the closure detail view
    /// </summary>
    private async Task<List<PagoDetalleCompletoDto>> GetPagosDetalleCompletoAsync(
        List<int> pagoIds,
        int institucionId,
        CancellationToken cancellationToken
    )
    {
        var pagosSource = _context.Pagos.AsNoTracking().Where(p => pagoIds.Contains(p.PagoId));

        return await GetPagosDetalleCompletoOptimizedAsync(
            pagosSource,
            institucionId,
            cancellationToken
        );
    }

    /// <summary>
    /// Gets all cash register closures and current pending transactions for an institution with optimized queries
    /// </summary>
    public async Task<ApiResponse<CierresyActualDto>> GetCierresyActualAsync(
        int institucionId,
        string? userId = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Retrieving cash closures and current transactions for institution {InstitucionId}",
                institucionId
            );

            // Get all completed closures for the institution
            var cierres = await _context
                .Cierre.AsNoTracking()
                .Where(c => c.FechaHoraCierre != null && c.InstitucionID == institucionId)
                .Include(c => c.Pagos)
                .OrderByDescending(c => c.FechaHoraCierre)
                .ToListAsync(cancellationToken);

            // Get the most recent closure to determine cutoff date for pending transactions
            var ultimoCierre = cierres.FirstOrDefault();
            var fechaUltimoCierre = ultimoCierre?.FechaHoraCierre ?? DateTime.MinValue;

            // Get pending payments (not associated with any closure)
            var pagosPendientes = await GetTransaccionesPendientesAsync(
                institucionId,
                fechaUltimoCierre,
                userId,
                cancellationToken
            );

            // Get pending expenses (not associated with any closure)
            var egresosPendientesRaw = await _context
                .Egresos.AsNoTracking()
                .Where(e => e.CierreID == null && e.InstitucionID == institucionId)
                .Include(e => e.TipoEgreso)
                .ToListAsync(cancellationToken);

            var egresosPendientes = _mapper.MapToEgresoDetalleDtos(egresosPendientesRaw);

            // Map closures to DTOs using dedicated mapper
            var cierresDto = _mapper.MapToCierreBasicoDtos(cierres);

            var resultado = new CierresyActualDto
            {
                Cierres = cierresDto,
                TransaccionesPendientes = pagosPendientes,
                EgresosPendientes = egresosPendientes,
            };

            _logger.LogInformation(
                "Retrieved {CierresCount} closures and {PendingCount} pending transactions for institution {InstitucionId}",
                cierresDto.Count,
                pagosPendientes.Count + egresosPendientes.Count,
                institucionId
            );

            return ApiResponse<CierresyActualDto>.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving cash closures and current transactions for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<CierresyActualDto>.Failure(
                "Error al obtener los cierres y transacciones actuales",
                "Ocurrió un error interno al obtener los datos. Por favor, inténtelo nuevamente."
            );
        }
    }

    /// <summary>
    /// Gets all pending transactions (payments, cancellations) since the last closure
    /// </summary>
    private async Task<List<TransaccionPendienteDto>> GetTransaccionesPendientesAsync(
        int institucionId,
        DateTime fechaUltimoCierre,
        string? userId = null,
        CancellationToken cancellationToken = default
    )
    {
        var transacciones = new List<TransaccionPendienteDto>();

        // Get pending payments using optimized query
        var pagosSource = _context
            .Pagos.AsNoTracking()
            .Where(p => p.CierreId == null && p.UserId == userId);

        var pagosPendientes = await GetTransaccionesPendientesOptimizedAsync(
            pagosSource,
            institucionId,
            cancellationToken
        );

        transacciones.AddRange(pagosPendientes);

        // Get canceled reservations since last closure
        var reservasAnuladas = await _context
            .Reservas.AsNoTracking()
            .Where(r => r.FechaAnula > fechaUltimoCierre && r.InstitucionID == institucionId)
            .Include(r => r.Habitacion)
            .ThenInclude(h => h!.Categoria)
            .ToListAsync(cancellationToken);

        // Add canceled reservations as transactions using mapper
        foreach (var reserva in reservasAnuladas)
        {
            transacciones.Add(_mapper.MapReservaAnuladaToTransaccionPendiente(reserva));
        }

        return transacciones.OrderBy(t => t.Fecha).ToList();
    }
}
