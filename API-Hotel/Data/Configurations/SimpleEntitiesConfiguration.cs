using Microsoft.EntityFrameworkCore;
using hotel.Models;

namespace hotel.Data.Configurations
{
    /// <summary>
    /// Configuración para entidades simples que solo requieren definir su clave primaria
    /// </summary>
    public static class SimpleEntitiesConfiguration
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            // Entidades con configuración simple de clave primaria
            modelBuilder.Entity<DiasSemana>().HasKey(e => e.DiaSemanaId);
            modelBuilder.Entity<TipoMovimiento>().HasKey(e => e.TipoMovimientoId);
            modelBuilder.Entity<MediosPago>().HasKey(e => e.MedioPagoId);
            modelBuilder.Entity<TipoEgreso>().HasKey(e => e.TipoEgresoId);
            modelBuilder.Entity<Tarjetas>().HasKey(e => e.TarjetaID);
            modelBuilder.Entity<Registros>().HasKey(e => e.RegistroID);
            modelBuilder.Entity<Promociones>().HasKey(e => e.PromocionID);
            modelBuilder.Entity<DescuentoEfectivo>().HasKey(e => e.DescuentoID);
            modelBuilder.Entity<Egresos>().HasKey(e => e.EgresoId);
            modelBuilder.Entity<Imagenes>().HasKey(e => e.ImagenId);
            modelBuilder.Entity<Inventarios>().HasKey(e => e.InventarioId);
            
            // Agregar las entidades faltantes
            modelBuilder.Entity<Caracteristica>().HasKey(e => e.CaracteristicaId);
            modelBuilder.Entity<Cierre>().HasKey(e => e.CierreId);
            modelBuilder.Entity<Consumo>().HasKey(e => e.ConsumoId);
            modelBuilder.Entity<CuadreCierre>().HasKey(e => e.CuadreCierreId);
            modelBuilder.Entity<Empeño>().HasKey(e => e.EmpeñoID);
            modelBuilder.Entity<Encargos>().HasKey(e => e.EncargosId);
            modelBuilder.Entity<InventarioGeneral>().HasKey(e => e.InventarioId);
            modelBuilder.Entity<Movimientos>().HasKey(e => e.MovimientosId);
            modelBuilder.Entity<MovimientosServicios>().HasKey(e => e.MovimientosServicioId);
            modelBuilder.Entity<MovimientosStock>().HasKey(e => e.MovimientoId);
            modelBuilder.Entity<MovimientosUsuarios>().HasKey(e => new { e.MovimientoId, e.UsuarioId });
            modelBuilder.Entity<Pagos>().HasKey(e => e.PagoId);
            modelBuilder.Entity<Personal>().HasKey(e => e.PersonalId);
            modelBuilder.Entity<Servicios>().HasKey(e => e.ServicioId);
            modelBuilder.Entity<ServiciosAdicionales>().HasKey(e => e.ServicioId);
            modelBuilder.Entity<Tarifas>().HasKey(e => e.TarifaId);
            modelBuilder.Entity<TipoTarifa>().HasKey(e => e.TipoTarifaId);
            modelBuilder.Entity<Usuarios>().HasKey(e => e.UsuarioId);
            // UsuariosInstituciones moved to its own configuration file
        }
    }
}