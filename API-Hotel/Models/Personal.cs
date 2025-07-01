using System;
using System.Collections.Generic;

namespace hotel.Models;

public partial class Personal
{
    public int PersonalId { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public int RolId { get; set; }

    public virtual Roles Rol { get; set; } = null!;
}
