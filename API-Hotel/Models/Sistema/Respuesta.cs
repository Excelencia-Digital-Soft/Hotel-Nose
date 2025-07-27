namespace hotel.Models.Sistema
{
    public class Respuesta
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = null!;
        public Object Data { get; set; } = null!;
    }
}
