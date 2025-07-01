using System;
using System.Collections.Generic;

namespace hotel.Models;

public partial class CategoriasArticulos
{
    public int CategoriaId { get; set; }

    public string? NombreCategoria { get; set; }

    public bool? Anulado { get; set; }
    public int InstitucionID { get; set; }
}
