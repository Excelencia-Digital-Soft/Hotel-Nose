namespace ApiObjetos.Models
{
    public class Empeño
    {
        public int EmpeñoID { get; set; }
        public int VisitaID { get; set; }
        public string? Detalle { get; set; }
        public float Monto { get; set; }
        public int? PagoID { get; set; }

        public Pagos? Pago { get; set; }
        public Visitas Visita { get; set; }
    }
}
