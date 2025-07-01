using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations;

public class ConfiguracionConfiguration : IEntityTypeConfiguration<Configuracion>
{
    public void Configure(EntityTypeBuilder<Configuracion> entity)
    {
        entity.ToTable("Configuracion");

        entity.HasKey(e => e.ConfiguracionId)
            .HasName("PK_Configuracion");

        entity.Property(e => e.ConfiguracionId)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Clave)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(false);

        entity.Property(e => e.Valor)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode(true);

        entity.Property(e => e.Descripcion)
            .HasMaxLength(255)
            .IsUnicode(true);

        entity.Property(e => e.Categoria)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.Property(e => e.FechaCreacion)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETDATE()");

        entity.Property(e => e.FechaModificacion)
            .HasColumnType("datetime2");

        entity.Property(e => e.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        entity.Property(e => e.InstitucionId)
            .IsRequired(false);

        // Foreign key relationship with Institucion
        entity.HasOne(d => d.Institucion)
            .WithMany()
            .HasForeignKey(d => d.InstitucionId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_Configuracion_Institucion");

        // Create unique index for Clave + InstitucionId to prevent duplicates per institution
        entity.HasIndex(e => new { e.Clave, e.InstitucionId })
            .IsUnique()
            .HasDatabaseName("IX_Configuracion_Clave_InstitucionId");

        // Create index for category for faster filtering
        entity.HasIndex(e => e.Categoria)
            .HasDatabaseName("IX_Configuracion_Categoria");

        // Create index for active configurations
        entity.HasIndex(e => e.Activo)
            .HasDatabaseName("IX_Configuracion_Activo");
    }
}