using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    /// <summary>
    /// Configuración para la entidad de relación HabitacionCaracteristica
    /// Tabla de unión many-to-many entre Habitaciones y Características
    /// </summary>
    public class HabitacionCaracteristicaConfiguration : IEntityTypeConfiguration<HabitacionCaracteristica>
    {
        public void Configure(EntityTypeBuilder<HabitacionCaracteristica> builder)
        {
            // Configurar tabla
            builder.ToTable("HabitacionCaracteristicas");

            // Configurar clave primaria compuesta
            builder.HasKey(hc => new { hc.HabitacionId, hc.CaracteristicaId });

            // Configurar propiedades
            builder.Property(hc => hc.HabitacionId)
                .IsRequired();

            builder.Property(hc => hc.CaracteristicaId)
                .IsRequired();

            // Configurar relación con Habitaciones
            builder.HasOne(hc => hc.Habitacion)
                .WithMany(h => h.HabitacionCaracteristicas)
                .HasForeignKey(hc => hc.HabitacionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HabitacionCaracteristicas_Habitaciones");

            // Configurar relación con Características
            builder.HasOne(hc => hc.Caracteristica)
                .WithMany(c => c.HabitacionCaracteristicas)
                .HasForeignKey(hc => hc.CaracteristicaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HabitacionCaracteristicas_Caracteristicas");

            // Índices para optimizar consultas
            builder.HasIndex(hc => hc.HabitacionId)
                .HasDatabaseName("IX_HabitacionCaracteristicas_HabitacionId");

            builder.HasIndex(hc => hc.CaracteristicaId)
                .HasDatabaseName("IX_HabitacionCaracteristicas_CaracteristicaId");
        }
    }
}