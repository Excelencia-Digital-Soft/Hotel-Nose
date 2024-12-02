using System;
using System.Collections.Generic;

namespace ApiObjetos.Models;

public partial class Promociones
{
    public int PromocionID { get; set; }

    public double Tarifa { get; set; }

    public int CantidadHoras { get; set; }

    public int CategoriaID { get; set; }
    public bool? Anulado { get; set; }


    public virtual CategoriasHabitaciones? Categoria { get; set; }


}
