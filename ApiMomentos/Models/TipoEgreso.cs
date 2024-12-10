using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace ApiObjetos.Models;

public partial class TipoEgreso
{
    public int TipoEgresoId { get; set; }

    public string Nombre { get; set; }

    [JsonIgnore]
    public virtual ICollection<Egresos> Egresos { get; } = new List<Egresos>();
}
