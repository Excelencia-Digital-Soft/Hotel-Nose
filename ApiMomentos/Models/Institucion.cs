namespace ApiObjetos.Models
{
    public class Institucion
    {
        public int InstitucionId { get; set; }
        public string Nombre { get; set; } = null!;
        public int TipoID { get; set; }
        public DateTime FechaAnulado { get; set; }

        public virtual ICollection<UsuariosInstituciones> UsuariosInstituciones { get; set; } = new List<UsuariosInstituciones>();
    }
}
