namespace hotel.Models
{
    public class Imagenes
    {
        public int ImagenId { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
        public int InstitucionID { get; set; }

        // Relación con HabitacionImagenes
        public virtual ICollection<HabitacionImagenes> HabitacionImagenes { get; set; } = [];
    }
}
