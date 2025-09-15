using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    public class CategoriasArticulosConfiguration : IEntityTypeConfiguration<CategoriasArticulos>
    {
        public void Configure(EntityTypeBuilder<CategoriasArticulos> builder)
        {
            builder.HasKey(e => e.CategoriaId);
            
            // Configurar relación OPCIONAL con Imagen (LEFT JOIN)
            builder.HasOne(d => d.Imagen)
                .WithMany()
                .HasForeignKey(d => d.imagenID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
            
            // Configurar relación OPCIONAL con Usuario Creador (LEFT JOIN)
            builder.HasOne(d => d.CreadoPor)
                .WithMany()
                .HasForeignKey(d => d.CreadoPorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
            
            // Configurar relación OPCIONAL con Usuario Modificador (LEFT JOIN)
            builder.HasOne(d => d.ModificadoPor)
                .WithMany()
                .HasForeignKey(d => d.ModificadoPorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
        }
    }
}