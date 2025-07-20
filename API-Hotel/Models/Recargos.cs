using System.Text.Json.Serialization;

namespace hotel.Models
{
    public partial class Recargos
    {
        public int RecargoID { get; set; }

        public string? Descripcion { get; set; }

        public decimal? Valor { get; set; }
        public int PagoID { get; set; }

        [JsonIgnore]
        public Pagos Pago { get; set; } = null!;
    }
}
