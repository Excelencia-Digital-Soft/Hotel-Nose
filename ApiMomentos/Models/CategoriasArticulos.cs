using System;
using System.Collections.Generic;

namespace ApiObjetos.Models;

public partial class CategoriasArticulos
{
    public int CategoriaId { get; set; }

    public string? NombreCategoria { get; set; }

    public bool? Anulado { get; set; }
}
