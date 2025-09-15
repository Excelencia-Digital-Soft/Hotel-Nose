using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Empenos
{
    /// <summary>
    /// DTO for representing Empeño (Pawn/Collateral) data
    /// </summary>
    public class EmpenoDto
    {
        public int EmpenoId { get; set; }
        public int VisitaId { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public double Monto { get; set; }
        public int? PagoId { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public bool Anulado { get; set; }
        public int InstitucionId { get; set; }
        
        // Navigation properties info
        public string? EstadoPago => PagoId.HasValue ? "Pagado" : "Pendiente";
        public DateTime? FechaPago { get; set; }
        
        // Additional info from related entities
        public string? NombreVisita { get; set; }
        public string? NumeroHabitacion { get; set; }
    }
    
    /// <summary>
    /// DTO for creating new Empeño records
    /// </summary>
    public class EmpenoCreateDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "VisitaId es requerido")]
        public int VisitaId { get; set; }
        
        [Required]
        [StringLength(500, ErrorMessage = "El detalle no puede exceder 500 caracteres")]
        public string Detalle { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public double Monto { get; set; }
    }
    
    /// <summary>
    /// DTO for updating existing Empeño records
    /// </summary>
    public class EmpenoUpdateDto
    {
        [StringLength(500, ErrorMessage = "El detalle no puede exceder 500 caracteres")]
        public string? Detalle { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public double? Monto { get; set; }
    }
    
    /// <summary>
    /// DTO for processing Empeño payment
    /// </summary>
    public class EmpernoPagoDto
    {
        [Required]
        [StringLength(200, ErrorMessage = "La observación no puede exceder 200 caracteres")]
        public string Observacion { get; set; } = string.Empty;
        
        [Range(0, double.MaxValue, ErrorMessage = "El monto en efectivo no puede ser negativo")]
        public decimal MontoEfectivo { get; set; } = 0;
        
        [Range(0, double.MaxValue, ErrorMessage = "El monto con tarjeta no puede ser negativo")]
        public decimal MontoTarjeta { get; set; } = 0;
        
        public int? TarjetaId { get; set; }
        
        /// <summary>
        /// Custom validation to ensure total payment amount is greater than 0
        /// </summary>
        public bool IsValidPayment => MontoEfectivo + MontoTarjeta > 0;
        
        public decimal MontoTotal => MontoEfectivo + MontoTarjeta;
    }
}