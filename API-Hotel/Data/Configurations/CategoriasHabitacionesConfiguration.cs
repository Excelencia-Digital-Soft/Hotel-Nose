using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    public class CategoriasHabitacionesConfiguration : IEntityTypeConfiguration<CategoriasHabitaciones>
    {
        public void Configure(EntityTypeBuilder<CategoriasHabitaciones> builder)
        {
            builder.HasKey(e => e.CategoriaId);
        }
    }
}