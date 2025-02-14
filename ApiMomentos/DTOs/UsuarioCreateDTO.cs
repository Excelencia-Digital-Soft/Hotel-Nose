namespace ApiObjetos.DTOs
{
    public class UsuarioCreateDto
    {
        public string NombreUsuario { get; set; } = null!;
        public string Contraseña { get; set; } = null!;
        public int RolId { get; set; }
        public int InstitucionID { get; set; }

    }
}
