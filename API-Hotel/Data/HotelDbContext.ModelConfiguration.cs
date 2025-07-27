using Microsoft.EntityFrameworkCore;
using hotel.Models;
using hotel.Data.Configurations;

namespace hotel.Data;

/// <summary>
/// Partial class del contexto que contiene configuraciones específicas del modelo
/// </summary>
public partial class HotelDbContext
{
    /// <summary>
    /// Configuraciones adicionales del modelo para resolver problemas de relaciones complejas
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo</param>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Aplicar configuraciones desde archivos separados
        ApplyConfigurations(modelBuilder);
    }

    private void ApplyConfigurations(ModelBuilder modelBuilder)
    {
        // Aplicar configuraciones individuales
        modelBuilder.ApplyConfiguration(new RecargosConfiguration());
        modelBuilder.ApplyConfiguration(new InventarioInicialConfiguration());
        modelBuilder.ApplyConfiguration(new AcompanantesConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriasArticulosConfiguration());
        modelBuilder.ApplyConfiguration(new ConfiguracionConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriasHabitacionesConfiguration());
        modelBuilder.ApplyConfiguration(new HabitacionesVirtualesConfiguration());
        
        // Configuraciones relacionadas con Habitaciones
        modelBuilder.ApplyConfiguration(new HabitacionesConfiguration());
        modelBuilder.ApplyConfiguration(new HabitacionImagenesConfiguration());
        modelBuilder.ApplyConfiguration(new HabitacionCaracteristicaConfiguration());
        modelBuilder.ApplyConfiguration(new CaracteristicaConfiguration());
        modelBuilder.ApplyConfiguration(new ImagenesConfiguration());
        
        modelBuilder.ApplyConfiguration(new VisitasConfiguration());
        modelBuilder.ApplyConfiguration(new ReservasConfiguration());
        modelBuilder.ApplyConfiguration(new MovimientosConfiguration());
        modelBuilder.ApplyConfiguration(new RolesConfiguration());
        modelBuilder.ApplyConfiguration(new InstitucionConfiguration());
        modelBuilder.ApplyConfiguration(new ArticulosConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumoConfiguration());
        modelBuilder.ApplyConfiguration(new PromocionesConfiguration());
        modelBuilder.ApplyConfiguration(new InventarioUnificadoConfiguration());
        
        // Configuraciones del sistema extendido de inventario
        modelBuilder.ApplyConfiguration(new MovimientoInventarioConfiguration());
        modelBuilder.ApplyConfiguration(new AlertaInventarioConfiguration());
        modelBuilder.ApplyConfiguration(new ConfiguracionAlertaInventarioConfiguration());
        modelBuilder.ApplyConfiguration(new TransferenciaInventarioConfiguration());
        modelBuilder.ApplyConfiguration(new DetalleTransferenciaInventarioConfiguration());
        
        modelBuilder.ApplyConfiguration(new UsuariosInstitucionesConfiguration());
        
        // Aplicar configuraciones simples
        SimpleEntitiesConfiguration.Configure(modelBuilder);
    }

}