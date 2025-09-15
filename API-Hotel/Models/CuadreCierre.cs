namespace hotel.Models;

public partial class CuadreCierre
{
    public int CuadreCierreId { get; set; }
    public int? CierreId { get; set; }
    public string? TipoMovimiento { get; set; }
    public string? Descripcion { get; set; }
    public decimal Monto { get; set; }
    public int? InstitucionId { get; set; }

    public virtual Cierre? Cierre { get; set; }
    public virtual Institucion? Institucion { get; set; }
}