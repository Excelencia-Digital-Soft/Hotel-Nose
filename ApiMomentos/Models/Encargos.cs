using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiObjetos.Models
{
    public class Encargos
    {
        [Key]
        public int EncargosId { get; set; }
        public int? ArticuloId { get; set; }
        public int? VisitaId { get; set; }
        public int? CantidadArt { get; set; }
        public string? Comentario { get; set; }
        public bool? Entregado { get; set; }
        public bool? Anulado { get; set; }
        public DateTime? FechaCrea { get; set; }
        public Visitas? Visita { get; set; }
        public Articulos? Articulo { get; set; }
    }
}
