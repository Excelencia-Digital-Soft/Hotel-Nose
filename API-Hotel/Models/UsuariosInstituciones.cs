namespace hotel.Models
{
    public class UsuariosInstituciones
    {
        public int UsuarioId { get; set; }
        public int InstitucionID { get; set; }

        public virtual Usuarios Usuario { get; set; } = null!;
        public virtual Institucion Institucion { get; set; } = null!;
    }
}
