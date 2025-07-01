using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs;

public class ConfiguracionDto
{
    public int ConfiguracionId { get; set; }
    public string Clave { get; set; } = null!;
    public string Valor { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public bool Activo { get; set; }
    public int? InstitucionId { get; set; }
}

public class ConfiguracionCreateDto
{
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

    public int? InstitucionId { get; set; }
}

public class ConfiguracionUpdateDto
{
    [Required]
    [StringLength(500)]
    public string Valor { get; set; } = null!;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    [StringLength(50)]
    public string? Categoria { get; set; }

    public bool? Activo { get; set; }
}

public class TimerUpdateIntervalDto
{
    [Required]
    [Range(1, 1440, ErrorMessage = "Timer update interval must be between 1 and 1440 minutes (24 hours)")]
    public int IntervalMinutos { get; set; }

    public string? Descripcion { get; set; }
}

public class TimerUpdateIntervalResponseDto
{
    public int IntervalMinutos { get; set; }
    public string? Descripcion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}