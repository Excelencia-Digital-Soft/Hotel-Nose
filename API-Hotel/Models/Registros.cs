using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace hotel.Models;

public partial class Registros
{
    public int RegistroID;
    public string Contenido;
    public int ReservaId { get; set; } // Nullable en caso de registros sin reserva

    // Otras propiedades de Registros...

    public Reservas? Reserva { get; set; } // Propiedad de navegación
}
