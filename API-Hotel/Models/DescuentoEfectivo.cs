using System;
using System.Collections.Generic;

namespace hotel.Models;

public partial class DescuentoEfectivo
{
    public int DescuentoID { get; set; }

    public int MontoPorcentual { get; set; }
    public int InstitucionID { get; set; }


}
