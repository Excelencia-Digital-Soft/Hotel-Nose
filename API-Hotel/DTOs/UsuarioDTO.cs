namespace hotel.DTOs
{
    [Obsolete("Usar Identity")]
    public class UsuarioDTO
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = null!;
        public int RolId { get; set; }
        public string NombreRol { get; set; } = null!;
    }
}
