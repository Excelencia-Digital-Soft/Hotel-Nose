namespace hotel.DTOs.Caja;

/// <summary>
/// DTO for cash closures with detailed payment information and pending payments
/// </summary>
public class CierresConPagosDto
{
    /// <summary>
    /// Completed cash closures
    /// </summary>
    public IEnumerable<CierreConPagosDto> Cierres { get; set; } = new List<CierreConPagosDto>();

    /// <summary>
    /// Payments that haven't been included in any closure yet
    /// </summary>
    public IEnumerable<PagoDetalladoDto> PagosSinCierre { get; set; } = new List<PagoDetalladoDto>();
}

/// <summary>
/// DTO for a single cash closure with its payments
/// </summary>
public class CierreConPagosDto
{
    public int CierreId { get; set; }
    public DateTime? FechaHoraCierre { get; set; }
    public decimal? TotalIngresosEfectivo { get; set; }
    public decimal? TotalIngresosBillVirt { get; set; }
    public decimal? TotalIngresosTarjeta { get; set; }
    public string? Observaciones { get; set; }
    public bool? EstadoCierre { get; set; }
    public decimal? MontoInicialCaja { get; set; }

    /// <summary>
    /// Detailed payments for this closure
    /// </summary>
    public IEnumerable<PagoDetalladoDto> Pagos { get; set; } = new List<PagoDetalladoDto>();
}

/// <summary>
/// DTO for detailed payment information including related business data
/// </summary>
public class PagoDetalladoDto
{
    public int PagoId { get; set; }
    public DateTime? Fecha { get; set; }
    public decimal? MontoEfectivo { get; set; }
    public decimal? MontoTarjeta { get; set; }
    public decimal? MontoBillVirt { get; set; }
    public decimal? MontoDescuento { get; set; }
    public decimal? InteresTarjeta { get; set; }
    public decimal? MontoAdicional { get; set; }
    public string? Observacion { get; set; }

    // Business context information
    public string? TipoTransaccion { get; set; } // "Habitacion", "Empeño", etc.
    public string? TarjetaNombre { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? TipoHabitacion { get; set; }
    public int? HabitacionId { get; set; }

    // Time tracking
    public DateTime? HoraIngreso { get; set; }
    public DateTime? HoraSalida { get; set; }

    // Financial details
    public decimal? Periodo { get; set; } // Room period cost
    public decimal? TotalConsumo { get; set; } // Additional consumption
}

/// <summary>
/// DTO for pagination parameters
/// </summary>
public class PaginationDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// DTO for paginated cash closures with payments
/// </summary>
public class CierresConPagosPaginadosDto
{
    public CierresConPagosDto Data { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}