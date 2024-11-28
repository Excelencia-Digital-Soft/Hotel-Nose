namespace ApiObjetos.Models
{
    public class Imagenes
    {
        public int ImagenId { get; set; }
        public string Origen { get; set; } // Identificador del origen, por ejemplo, "Articulos"
        public string NombreArchivo { get; set; } // Nombre único del archivo
    }
}
