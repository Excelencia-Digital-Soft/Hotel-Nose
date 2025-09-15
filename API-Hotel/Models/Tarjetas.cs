namespace hotel.Models;

public partial class Tarjetas
{
    public int TarjetaID { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public int MontoPorcentual { get; set; }

    public int InstitucionID { get; set; }
}
