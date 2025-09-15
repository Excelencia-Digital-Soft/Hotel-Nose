using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    public class RecargosConfiguration : IEntityTypeConfiguration<Recargos>
    {
        public void Configure(EntityTypeBuilder<Recargos> builder)
        {
            builder.HasKey(e => e.RecargoID);
            
            builder.HasOne(d => d.Pago)
                .WithOne(p => p.Recargo)
                .HasForeignKey<Recargos>(d => d.PagoID)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}