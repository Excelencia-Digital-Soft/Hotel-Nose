namespace hotel.DTOs.Visitas;

public class VisitaDto
{
    public int VisitaId { get; set; }
    public string? PatenteVehiculo { get; set; }
    public string? Identificador { get; set; }
    public string? NumeroTelefono { get; set; }
    public DateTime? FechaPrimerIngreso { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public bool Anulado { get; set; }
    public int InstitucionID { get; set; }
    public int? HabitacionId { get; set; }
    public string? HabitacionNombre { get; set; }
    public bool TieneReservaActiva { get; set; }
    public int? ReservaActivaId { get; set; }
}

public class VisitaCreateDto
{
    public string? PatenteVehiculo { get; set; }
    public string? Identificador { get; set; }
    public string? NumeroTelefono { get; set; }
    public int? HabitacionId { get; set; }
    public bool EsReserva { get; set; }
}

public class VisitaUpdateDto
{
    public string? PatenteVehiculo { get; set; }
    public string? Identificador { get; set; }
    public string? NumeroTelefono { get; set; }
}