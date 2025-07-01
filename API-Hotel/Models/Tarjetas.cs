using System;
using System.Collections.Generic;

namespace hotel.Models;

public partial class Tarjetas
{
    public int TarjetaID { get; set; }

    public string Nombre { get; set; }
    public int MontoPorcentual { get; set; }

    public int InstitucionID { get; set; }
    
}
