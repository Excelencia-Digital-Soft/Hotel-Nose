using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    /// <summary>
    /// Configuración para la entidad Habitaciones
    /// </summary>
    public class HabitacionesConfiguration : IEntityTypeConfiguration<Habitaciones>
    {
        public void Configure(EntityTypeBuilder<Habitaciones> builder)
        {
            // Configurar tabla
            builder.ToTable("Habitaciones");

            // Configurar clave primaria
            builder.HasKey(h => h.HabitacionId);

            // Configurar propiedades
            builder.Property(h => h.HabitacionId)
                .ValueGeneratedOnAdd();

            builder.Property(h => h.NombreHabitacion)
                .HasMaxLength(100);

            builder.Property(h => h.Disponible)
                .HasDefaultValue(true);

            builder.Property(h => h.Anulado)
                .HasDefaultValue(false);

            builder.Property(h => h.FechaRegistro)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(h => h.InstitucionID)
                .IsRequired();

            // Configurar la relación con CategoriasHabitaciones
            builder.HasOne(h => h.Categoria)
                .WithMany(c => c.Habitaciones)
                .HasForeignKey(h => h.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Habitaciones_CategoriasHabitaciones");

            // Configurar la relación con Visitas
            builder.HasOne(h => h.Visita)
                .WithOne(v => v.Habitacion)
                .HasForeignKey<Habitaciones>(h => h.VisitaID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Habitaciones_Visitas");

            // Las relaciones many-to-many se configuran en las configuraciones específicas:
            // - HabitacionCaracteristicas en HabitacionCaracteristicaConfiguration
            // - HabitacionImagenes en HabitacionImagenesConfiguration

            // Índices
            builder.HasIndex(h => h.InstitucionID)
                .HasDatabaseName("IX_Habitaciones_InstitucionID");

            builder.HasIndex(h => h.CategoriaId)
                .HasDatabaseName("IX_Habitaciones_CategoriaId");

            builder.HasIndex(h => h.VisitaID)
                .HasDatabaseName("IX_Habitaciones_VisitaID");

            builder.HasIndex(h => h.Disponible)
                .HasDatabaseName("IX_Habitaciones_Disponible");

            builder.HasIndex(h => h.Anulado)
                .HasDatabaseName("IX_Habitaciones_Anulado");

            // Índice compuesto para búsquedas frecuentes
            builder.HasIndex(h => new { h.InstitucionID, h.Disponible, h.Anulado })
                .HasDatabaseName("IX_Habitaciones_InstitucionID_Disponible_Anulado");
        }
    }
}