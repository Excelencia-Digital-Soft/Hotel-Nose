using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    /// <summary>
    /// Configuración para la entidad Imagenes
    /// </summary>
    public class ImagenesConfiguration : IEntityTypeConfiguration<Imagenes>
    {
        public void Configure(EntityTypeBuilder<Imagenes> builder)
        {
            // Configurar tabla
            builder.ToTable("Imagenes");

            // Configurar clave primaria
            builder.HasKey(i => i.ImagenId);

            // Mapear a la columna real de la base de datos
            builder.Property(i => i.ImagenId)
                .HasColumnName("imagenID")
                .ValueGeneratedOnAdd();

            builder.Property(i => i.NombreArchivo)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(i => i.Origen)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(i => i.FechaSubida)
                .IsRequired();

            builder.Property(i => i.InstitucionID)
                .IsRequired();

            // Configurar relaciones
            // La relación con HabitacionImagenes se configura en HabitacionImagenesConfiguration

            // Índices
            builder.HasIndex(i => i.NombreArchivo)
                .HasDatabaseName("IX_Imagenes_NombreArchivo");
        }
    }
}