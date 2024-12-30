using System;
using System.Collections.Generic;

namespace ApiObjetos.Models;

public partial class Roles
{
    public int RolId { get; set; }

    public string NombreRol { get; set; } = null!;
    public int InstitucionID { get; set; }

    public virtual ICollection<Personal> Personal { get; } = new List<Personal>();

    public virtual ICollection<Usuarios> Usuarios { get; } = new List<Usuarios>();
}
