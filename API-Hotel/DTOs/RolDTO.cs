namespace hotel.DTOs
{
    [Obsolete("Usar Identity")]
    public class RolDTO
    {
        public int RolId { get; set; }
        public string NombreRol { get; set; } = null!;
    }
}
