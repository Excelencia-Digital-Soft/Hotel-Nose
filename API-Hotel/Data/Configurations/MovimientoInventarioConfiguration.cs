using hotel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace hotel.Data.Configurations;

/// <summary>
/// Entity Framework configuration for MovimientoInventario
/// </summary>
public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
    {
        // Table configuration
        builder.ToTable("MovimientosInventario");
        
        // Primary key
        builder.HasKey(m => m.MovimientoId);
        
        // Properties configuration
        builder.Property(m => m.MovimientoId)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(m => m.InventarioId)
            .IsRequired();

        builder.Property(m => m.InstitucionID)
            .IsRequired();

        builder.Property(m => m.TipoMovimiento)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.CantidadAnterior)
            .IsRequired();

        builder.Property(m => m.CantidadNueva)
            .IsRequired();

        builder.Property(m => m.CantidadCambiada)
            .IsRequired();

        builder.Property(m => m.Motivo)
            .HasMaxLength(500);

        builder.Property(m => m.NumeroDocumento)
            .HasMaxLength(100);

        builder.Property(m => m.FechaMovimiento)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(m => m.UsuarioId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(m => m.DireccionIP)
            .HasMaxLength(45);

        builder.Property(m => m.Metadata)
            .HasColumnType("ntext");

        // Relationships
        builder.HasOne(m => m.Inventario)
            .WithMany()
            .HasForeignKey(m => m.InventarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Institucion)
            .WithMany()
            .HasForeignKey(m => m.InstitucionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Usuario)
            .WithMany()
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Transferencia)
            .WithMany(t => t.Movimientos)
            .HasForeignKey(m => m.TransferenciaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(m => m.InventarioId)
            .HasDatabaseName("IX_MovimientosInventario_InventarioId");

        builder.HasIndex(m => m.InstitucionID)
            .HasDatabaseName("IX_MovimientosInventario_InstitucionID");

        builder.HasIndex(m => m.FechaMovimiento)
            .HasDatabaseName("IX_MovimientosInventario_FechaMovimiento");

        builder.HasIndex(m => m.TipoMovimiento)
            .HasDatabaseName("IX_MovimientosInventario_TipoMovimiento");

        builder.HasIndex(m => m.UsuarioId)
            .HasDatabaseName("IX_MovimientosInventario_UsuarioId");

        builder.HasIndex(m => m.TransferenciaId)
            .HasDatabaseName("IX_MovimientosInventario_TransferenciaId");

        // Check constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_MovimientosInventario_TipoMovimiento", 
            "[TipoMovimiento] IN ('Entrada', 'Salida', 'Transferencia', 'Ajuste', 'Consumo', 'Devolucion', 'Perdida', 'Sincronizacion')"));
    }
}