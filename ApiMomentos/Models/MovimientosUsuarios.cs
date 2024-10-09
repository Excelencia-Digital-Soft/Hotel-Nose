using System;
using System.Collections.Generic;

namespace ApiObjetos.Models;

public partial class MovimientosUsuarios
{
    public int MovimientoId { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaHora { get; set; }

    public string Accion { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
