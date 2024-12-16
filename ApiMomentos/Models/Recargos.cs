using System;
using System.Text.Json.Serialization;

namespace ApiObjetos.Models
{
    public partial class Recargos
    {
        public int RecargoID { get; set; }

        public string? Descripcion { get; set; }

        public decimal? Valor { get; set; }
        public int PagoID { get; set; }
        [JsonIgnore]
        public Pagos Pago { get; set; }

    }
}
