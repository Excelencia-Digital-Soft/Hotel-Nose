namespace hotel.Models;

public partial class CategoriasHabitaciones
{
    public int CategoriaId { get; set; }

    public string? NombreCategoria { get; set; }

    public int? CapacidadMaxima { get; set; }

    public int? UsuarioId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }

    public decimal? PrecioNormal { get; set; }
    public int InstitucionID { get; set; }

    public int PorcentajeXPersona { get; set; }

    public virtual ICollection<Habitaciones> Habitaciones { get; } = new List<Habitaciones>();

    public virtual ICollection<Tarifas> Tarifas { get; } = new List<Tarifas>();

    public ICollection<Promociones> Promociones { get; set; } = []; // Navigation property
}
