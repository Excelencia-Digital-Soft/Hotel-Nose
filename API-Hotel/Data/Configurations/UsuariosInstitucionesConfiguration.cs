using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    public class UsuariosInstitucionesConfiguration : IEntityTypeConfiguration<UsuariosInstituciones>
    {
        public void Configure(EntityTypeBuilder<UsuariosInstituciones> builder)
        {
            // Map to correct table name
            builder.ToTable("UsuariosInstituciones");
            
            // Configure composite key to match database structure
            builder.HasKey(e => new { e.UsuarioId, e.InstitucionID });
            
            // Map properties explicitly to database columns
            builder.Property(e => e.UsuarioId).HasColumnName("UsuarioId");
            builder.Property(e => e.InstitucionID).HasColumnName("InstitucionID");
            
            // Configure UserId property for AspNetUsers relationship
            builder.Property(e => e.UserId)
                .HasMaxLength(450) // Standard length for AspNetUsers.Id
                .IsRequired(false);
            
            // Configure relationships with explicit navigation properties
            builder.HasOne(e => e.Usuario)
                .WithMany(u => u.UsuariosInstituciones)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(e => e.Institucion)
                .WithMany(i => i.UsuariosInstituciones)
                .HasForeignKey(e => e.InstitucionID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with ApplicationUser (AspNetUsers)
            builder.HasOne(e => e.ApplicationUser)
                .WithMany(au => au.UsuariosInstituciones)
                .HasForeignKey(e => e.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Add index for ApplicationUser relationship
            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_UsuariosInstituciones_UserId");
        }
    }
}