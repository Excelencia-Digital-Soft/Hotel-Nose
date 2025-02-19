using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiObjetos.Models;

public partial class Usuarios
{
    public int UsuarioId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Contraseña { get; set; } = null!;

    public int RolId { get; set; }
    public int InstitucionID { get; set; }

    public virtual ICollection<Cierre> Cierre { get; } = new List<Cierre>();

    public virtual ICollection<MovimientosUsuarios> MovimientosUsuarios { get; } = new List<MovimientosUsuarios>();

    public virtual Roles Rol { get; set; } = null!;
}
