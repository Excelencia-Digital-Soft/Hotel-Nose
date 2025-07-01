using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace hotel.Models;

public partial class Promociones
{
    public int PromocionID { get; set; }

    public decimal Tarifa { get; set; }

    public int CantidadHoras { get; set; }

    public int CategoriaID { get; set; }
    public bool? Anulado { get; set; }
    public string Detalle { get; set; }
    public int InstitucionID { get; set; }
    [JsonIgnore]
    public virtual CategoriasHabitaciones? Categoria { get; set; }


}
