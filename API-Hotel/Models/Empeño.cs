using hotel.Models.Identity;

namespace hotel.Models
{
    public class Empeño
    {
        public int EmpeñoID { get; set; }
        public int VisitaID { get; set; }
        public string? Detalle { get; set; }
        public double Monto { get; set; }
        public int? PagoID { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public bool? Anulado { get; set; }
        public int InstitucionID { get; set; }
        
        // ASP.NET Identity User tracking
        public string? UserId { get; set; }

        // Navigation properties
        public Pagos? Pago { get; set; }
        public Visitas Visita { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
    }
}
