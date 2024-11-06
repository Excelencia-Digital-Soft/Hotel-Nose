namespace ApiObjetos.DTOs
{
    public class EncargoDTO
    {
        public int EncargosId { get; set; }
        public int? ArticuloId { get; set; }
        public int? VisitaId { get; set; }
        public int? CantidadArt { get; set; }
        public bool? Entregado { get; set; }
        public bool? Anulado { get; set; }
        public DateTime? FechaCrea { get; set; }
        public int? HabitacionId { get; set; } // Agregar esta propiedad
    }
}
