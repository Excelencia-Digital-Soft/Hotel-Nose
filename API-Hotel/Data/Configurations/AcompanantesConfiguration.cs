using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    public class AcompanantesConfiguration : IEntityTypeConfiguration<Acompanantes>
    {
        public void Configure(EntityTypeBuilder<Acompanantes> builder)
        {
            builder.HasKey(e => e.AcompananteId);
            
            builder.HasOne(d => d.Institucion)
                .WithMany()
                .HasForeignKey(d => d.InstitucionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}