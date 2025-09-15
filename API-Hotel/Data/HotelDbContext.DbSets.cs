using hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace hotel.Data;

/// <summary>
/// Partial class del contexto de base de datos que contiene las definiciones de DbSets
/// </summary>
public partial class HotelDbContext
{
    #region Identity DbSets

    // Los DbSets de Identity son heredados de IdentityDbContext<ApplicationUser, ApplicationRole, string>
    // Users, Roles, UserRoles, UserClaims, UserLogins, UserTokens, RoleClaims

    #endregion

    #region All Business DbSets - Todos los DbSets del negocio

    public virtual DbSet<Acompanantes> Acompanantes { get; set; }

    public virtual DbSet<Articulos> Articulos { get; set; }

    public virtual DbSet<CategoriasArticulos> CategoriasArticulos { get; set; }

    public virtual DbSet<CategoriasHabitaciones> CategoriasHabitaciones { get; set; }

    public virtual DbSet<Caracteristica> Caracteristicas { get; set; }

    public virtual DbSet<Cierre> Cierre { get; set; }

    public virtual DbSet<Configuracion> Configuraciones { get; set; }

    public virtual DbSet<Consumo> Consumo { get; set; }

    public virtual DbSet<CuadreCierre> CuadreCierre { get; set; }

    public virtual DbSet<DescuentoEfectivo> DescuentoEfectivo { get; set; }

    public virtual DbSet<DiasSemana> DiasSemana { get; set; }

    public virtual DbSet<Egresos> Egresos { get; set; }

    public virtual DbSet<Empeño> Empeño { get; set; }

    public virtual DbSet<Encargos> Encargos { get; set; }

    public virtual DbSet<HabitacionCaracteristica> HabitacionCaracteristicas { get; set; }

    public virtual DbSet<Habitaciones> Habitaciones { get; set; }

    public virtual DbSet<HabitacionesVirtuales> HabitacionesVirtuales { get; set; }

    public virtual DbSet<HabitacionImagenes> HabitacionImagenes { get; set; }

    public virtual DbSet<Imagenes> Imagenes { get; set; }

    public virtual DbSet<Institucion> Instituciones { get; set; }

    public virtual DbSet<InventarioInicial> InventarioInicial { get; set; }

    public virtual DbSet<Inventarios> Inventarios { get; set; }

    public virtual DbSet<InventarioGeneral> InventarioGeneral { get; set; }

    public virtual DbSet<InventarioUnificado> InventarioUnificado { get; set; }

    public virtual DbSet<MovimientoInventario> MovimientosInventario { get; set; }

    public virtual DbSet<AlertaInventario> AlertasInventario { get; set; }

    public virtual DbSet<ConfiguracionAlertaInventario> ConfiguracionAlertasInventario { get; set; }

    public virtual DbSet<TransferenciaInventario> TransferenciasInventario { get; set; }

    public virtual DbSet<DetalleTransferenciaInventario> DetallesTransferenciaInventario { get; set; }

    public virtual DbSet<MediosPago> MediosPago { get; set; }

    public virtual DbSet<Movimientos> Movimientos { get; set; }

    public virtual DbSet<MovimientosServicios> MovimientosServicios { get; set; }

    public virtual DbSet<MovimientosStock> MovimientosStock { get; set; }

    public virtual DbSet<MovimientosUsuarios> MovimientosUsuarios { get; set; }

    public virtual DbSet<Pagos> Pagos { get; set; }

    public virtual DbSet<Personal> Personal { get; set; }

    public virtual DbSet<Promociones> Promociones { get; set; }

    public virtual DbSet<Recargos> Recargos { get; set; }

    public virtual DbSet<Registros> Registros { get; set; }

    public virtual DbSet<Reservas> Reservas { get; set; }

    public virtual DbSet<Roles> HotelRoles { get; set; }

    public virtual DbSet<Servicios> Servicios { get; set; }

    public virtual DbSet<ServiciosAdicionales> ServiciosAdicionales { get; set; }

    public virtual DbSet<Tarifas> Tarifas { get; set; }

    public virtual DbSet<Tarjetas> Tarjetas { get; set; }

    public virtual DbSet<TipoEgreso> TipoEgreso { get; set; }

    public virtual DbSet<TipoMovimiento> TipoMovimiento { get; set; }

    public virtual DbSet<TipoTarifa> TipoTarifa { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    public virtual DbSet<UsuariosInstituciones> UsuariosInstituciones { get; set; }

    public virtual DbSet<Visitas> Visitas { get; set; }

    #endregion
}

