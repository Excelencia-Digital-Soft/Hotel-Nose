namespace hotel.DTOs
{
    public class UsuarioCreateDTO
    {
        public string NombreUsuario { get; set; } = null!;
        public string Contraseña { get; set; } = null!;
        public int RolId { get; set; }

    }
}
