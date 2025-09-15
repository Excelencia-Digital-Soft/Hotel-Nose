using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    public class InventarioInicialConfiguration : IEntityTypeConfiguration<InventarioInicial>
    {
        public void Configure(EntityTypeBuilder<InventarioInicial> builder)
        {
            builder.HasKey(e => e.ArticuloId); // InventarioInicial usa ArticuloId como clave primaria
            
            builder.HasOne(d => d.Articulo)
                .WithOne(p => p.InventarioInicial)
                .HasForeignKey<InventarioInicial>(d => d.ArticuloId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}