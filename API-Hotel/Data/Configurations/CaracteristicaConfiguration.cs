using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    /// <summary>
    /// Configuración para la entidad Caracteristica
    /// </summary>
    public class CaracteristicaConfiguration : IEntityTypeConfiguration<Caracteristica>
    {
        public void Configure(EntityTypeBuilder<Caracteristica> builder)
        {
            // Configurar tabla
            builder.ToTable("Caracteristicas");

            // Configurar clave primaria
            builder.HasKey(c => c.CaracteristicaId);

            // Configurar propiedades
            builder.Property(c => c.CaracteristicaId)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Descripcion)
                .HasMaxLength(500);

            builder.Property(c => c.Icono)
                .HasMaxLength(255);

            // Configurar relaciones
            // La relación many-to-many con Habitaciones se configura en HabitacionCaracteristicaConfiguration

            // Índices
            builder.HasIndex(c => c.Nombre)
                .HasDatabaseName("IX_Caracteristicas_Nombre");
        }
    }
}