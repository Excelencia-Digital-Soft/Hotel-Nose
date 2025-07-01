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
        }
    }
}