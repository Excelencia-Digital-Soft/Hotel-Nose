using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    public class HabitacionesVirtualesConfiguration : IEntityTypeConfiguration<HabitacionesVirtuales>
    {
        public void Configure(EntityTypeBuilder<HabitacionesVirtuales> builder)
        {
            builder.HasKey(e => e.HabitacionVirtualId);

            // Relación con Habitacion1
            builder.HasOne(d => d.Habitacion1)
                .WithMany(p => p.HabitacionesVirtualesHabitacion1)
                .HasForeignKey(d => d.Habitacion1Id)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Relación con Habitacion2
            builder.HasOne(d => d.Habitacion2)
                .WithMany(p => p.HabitacionesVirtualesHabitacion2)
                .HasForeignKey(d => d.Habitacion2Id)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}