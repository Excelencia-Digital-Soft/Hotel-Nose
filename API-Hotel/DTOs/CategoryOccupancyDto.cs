namespace hotel.DTOs
{
    public class CategoryOccupancyDto
    {
        public int CategoriaID { get; set; }
        public string NombreCategoria { get; set; }
        public double TasaOcupacion { get; set; }
        public int TotalHorasOcupadas { get; set; }
    }
}