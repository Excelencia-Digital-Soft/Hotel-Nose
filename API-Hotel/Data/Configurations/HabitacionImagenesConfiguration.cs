using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    /// <summary>
    /// Configuración para la entidad de relación HabitacionImagenes
    /// Tabla de unión one-to-many entre Habitaciones e Imágenes
    /// </summary>
    public class HabitacionImagenesConfiguration : IEntityTypeConfiguration<HabitacionImagenes>
    {
        public void Configure(EntityTypeBuilder<HabitacionImagenes> builder)
        {
            // Configurar tabla
            builder.ToTable("HabitacionImagenes");

            // Configurar clave primaria
            builder.HasKey(hi => hi.Id);

            // Configurar propiedades
            builder.Property(hi => hi.Id)
                .ValueGeneratedOnAdd();

            builder.Property(hi => hi.HabitacionId)
                .HasColumnName("HabitacionID")
                .IsRequired();

            builder.Property(hi => hi.ImagenId)
                .HasColumnName("ImagenID")
                .IsRequired();

            builder.Property(hi => hi.Anulado)
                .HasDefaultValue(false);

            builder.Property(hi => hi.EsPrincipal)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(hi => hi.Orden)
                .IsRequired()
                .HasDefaultValue(0);

            // Configurar relación con Habitaciones
            builder.HasOne(hi => hi.Habitacion)
                .WithMany(h => h.HabitacionImagenes)
                .HasForeignKey(hi => hi.HabitacionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HabitacionImagenes_Habitaciones");

            // Configurar relación con Imágenes
            builder.HasOne(hi => hi.Imagen)
                .WithMany(i => i.HabitacionImagenes)
                .HasForeignKey(hi => hi.ImagenId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HabitacionImagenes_Imagenes");

            // Índices para optimizar consultas
            builder.HasIndex(hi => hi.HabitacionId)
                .HasDatabaseName("IX_HabitacionImagenes_HabitacionId");

            builder.HasIndex(hi => hi.ImagenId)
                .HasDatabaseName("IX_HabitacionImagenes_ImagenId");

            builder.HasIndex(hi => hi.Anulado)
                .HasDatabaseName("IX_HabitacionImagenes_Anulado");

            // Índices adicionales
            builder.HasIndex(hi => hi.EsPrincipal)
                .HasDatabaseName("IX_HabitacionImagenes_EsPrincipal");

            builder.HasIndex(hi => hi.Orden)
                .HasDatabaseName("IX_HabitacionImagenes_Orden");
        }
    }
}