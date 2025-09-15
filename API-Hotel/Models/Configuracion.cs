using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotel.Models;

[Table("Configuracion")]
public partial class Configuracion
{
    [Key]
    public int ConfiguracionId { get; set; }

    [Required]
    [StringLength(100)]
    public string Clave { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string Valor { get; set; } = null!;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    [StringLength(50)]
    public string? Categoria { get; set; }

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaModificacion { get; set; }

    [Required]
    public bool Activo { get; set; } = true;

    public int? InstitucionId { get; set; }

    // Navigation properties
    public virtual Institucion? Institucion { get; set; }
}