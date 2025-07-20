namespace hotel.DTOs
{
    [Obsolete("No usar")]
    public class UsuarioResponseDTO
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public int RolId { get; set; }
    }
}
