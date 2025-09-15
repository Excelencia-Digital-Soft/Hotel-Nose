using hotel.Models;
using hotel.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace hotel.Data.Configurations;

/// <summary>
/// Configuración de Entity Framework para ApplicationUser
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Configurar la tabla base (ya configurada por Identity)
        builder.ToTable("AspNetUsers");

        // Configurar propiedades adicionales
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(u => u.LegacyUserId)
            .IsRequired(false);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.LastLoginAt)
            .IsRequired(false);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.ForcePasswordChange)
            .IsRequired()
            .HasDefaultValue(false);

        // Configurar relación con Institucion
        builder.HasOne(u => u.Institucion)
            .WithMany() // Institucion no tiene navegación hacia ApplicationUser
            .HasForeignKey(u => u.InstitucionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada

        // Configurar índices
        builder.HasIndex(u => u.InstitucionId)
            .HasDatabaseName("IX_AspNetUsers_InstitucionId");

        builder.HasIndex(u => u.LegacyUserId)
            .HasDatabaseName("IX_AspNetUsers_LegacyUserId");

        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_AspNetUsers_IsActive");

        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("IX_AspNetUsers_CreatedAt");

        // Configurar relación con UsuariosInstituciones (many-to-many)
        builder.HasMany(u => u.UsuariosInstituciones)
            .WithOne(ui => ui.ApplicationUser)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}