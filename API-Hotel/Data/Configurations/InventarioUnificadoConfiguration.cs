using hotel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace hotel.Data.Configurations;

/// <summary>
/// Entity Framework configuration for InventarioUnificado
/// </summary>
public class InventarioUnificadoConfiguration : IEntityTypeConfiguration<InventarioUnificado>
{
    public void Configure(EntityTypeBuilder<InventarioUnificado> builder)
    {
        // Table configuration
        builder.ToTable("InventarioUnificado");
        builder.HasKey(e => e.InventarioId);

        // Primary key
        builder.Property(e => e.InventarioId)
            .HasColumnName("InventarioId")
            .ValueGeneratedOnAdd();

        // Required fields
        builder.Property(e => e.ArticuloId)
            .HasColumnName("ArticuloId")
            .IsRequired();

        builder.Property(e => e.Cantidad)
            .HasColumnName("Cantidad")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.InstitucionID)
            .HasColumnName("InstitucionID")
            .IsRequired();

        builder.Property(e => e.TipoUbicacion)
            .HasColumnName("TipoUbicacion")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.UbicacionId)
            .HasColumnName("UbicacionId");

        // Audit fields
        builder.Property(e => e.FechaRegistro)
            .HasColumnName("FechaRegistro")
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.FechaUltimaActualizacion)
            .HasColumnName("FechaUltimaActualizacion")
            .HasColumnType("datetime2");

        builder.Property(e => e.UsuarioRegistro)
            .HasColumnName("UsuarioRegistro")
            .HasMaxLength(450);

        builder.Property(e => e.UsuarioUltimaActualizacion)
            .HasColumnName("UsuarioUltimaActualizacion")
            .HasMaxLength(450);

        // Soft delete fields
        builder.Property(e => e.Anulado)
            .HasColumnName("Anulado")
            .HasDefaultValue(false);

        builder.Property(e => e.FechaAnulacion)
            .HasColumnName("FechaAnulacion")
            .HasColumnType("datetime2");

        builder.Property(e => e.UsuarioAnulacion)
            .HasColumnName("UsuarioAnulacion")
            .HasMaxLength(450);

        builder.Property(e => e.MotivoAnulacion)
            .HasColumnName("MotivoAnulacion")
            .HasMaxLength(200);

        // Stock management fields
        builder.Property(e => e.CantidadMinima)
            .HasColumnName("CantidadMinima")
            .HasDefaultValue(0);

        builder.Property(e => e.CantidadMaxima)
            .HasColumnName("CantidadMaxima");

        builder.Property(e => e.PuntoReorden)
            .HasColumnName("PuntoReorden")
            .HasDefaultValue(0);

        builder.Property(e => e.Notas)
            .HasColumnName("Notas")
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => new { e.InstitucionID, e.TipoUbicacion })
            .HasDatabaseName("IX_InventarioUnificado_InstitucionID_TipoUbicacion");

        builder.HasIndex(e => new { e.ArticuloId, e.InstitucionID })
            .HasDatabaseName("IX_InventarioUnificado_ArticuloId_InstitucionID");

        builder.HasIndex(e => e.UbicacionId)
            .HasDatabaseName("IX_InventarioUnificado_UbicacionId")
            .HasFilter("[UbicacionId] IS NOT NULL");

        // Unique constraint to prevent duplicates
        builder.HasIndex(e => new { e.ArticuloId, e.TipoUbicacion, e.UbicacionId, e.InstitucionID })
            .HasDatabaseName("UK_InventarioUnificado_Articulo_Ubicacion")
            .IsUnique();

        // Foreign key relationships
        builder.HasOne(e => e.Articulo)
            .WithMany()
            .HasForeignKey(e => e.ArticuloId)
            .HasConstraintName("FK_InventarioUnificado_Articulos")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Institucion)
            .WithMany()
            .HasForeignKey(e => e.InstitucionID)
            .HasConstraintName("FK_InventarioUnificado_Instituciones")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Habitacion)
            .WithMany()
            .HasForeignKey(e => e.UbicacionId)
            .HasConstraintName("FK_InventarioUnificado_Habitaciones")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CreadoPor)
            .WithMany()
            .HasForeignKey(e => e.UsuarioRegistro)
            .HasConstraintName("FK_InventarioUnificado_CreadoPor")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ModificadoPor)
            .WithMany()
            .HasForeignKey(e => e.UsuarioUltimaActualizacion)
            .HasConstraintName("FK_InventarioUnificado_ModificadoPor")
            .OnDelete(DeleteBehavior.SetNull);

        // Business constraints (check constraints)
        builder.HasCheckConstraint("CK_InventarioUnificado_Cantidad", "[Cantidad] >= 0");
        builder.HasCheckConstraint("CK_InventarioUnificado_TipoUbicacion", "[TipoUbicacion] IN (0, 1, 2)");
        builder.HasCheckConstraint("CK_InventarioUnificado_UbicacionRoom", 
            "([TipoUbicacion] = 1 AND [UbicacionId] IS NOT NULL) OR ([TipoUbicacion] != 1 AND ([UbicacionId] IS NULL OR [TipoUbicacion] = 2))");

        // Global query filter for soft delete
        builder.HasQueryFilter(e => e.Anulado != true);
    }
}