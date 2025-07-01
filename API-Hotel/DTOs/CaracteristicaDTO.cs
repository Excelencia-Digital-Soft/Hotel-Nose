namespace hotel.DTOs
{
    public class CaracteristicaDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; } // Opcional
        public IFormFile? Icono { get; set; } // Opcional
    }

}
