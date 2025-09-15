using System.Text.Json.Serialization;

namespace hotel.Models;

public partial class TipoEgreso
{
    public int TipoEgresoId { get; set; }

    public string Nombre { get; set; } = null!;

    public int InstitucionID { get; set; }

    [JsonIgnore]
    public virtual ICollection<Egresos> Egresos { get; } = new List<Egresos>();
}
