namespace hotel.Models;

public partial class Servicios
{
    public int ServicioId { get; set; }
    public string NombreServicio { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;
    public string? Categoria { get; set; }
    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<MovimientosServicios> MovimientosServicios { get; set; } = new List<MovimientosServicios>();
}