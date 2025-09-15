using hotel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace hotel.Data.Configurations;

/// <summary>
/// Entity Framework configuration for TransferenciaInventario
/// </summary>
public class TransferenciaInventarioConfiguration : IEntityTypeConfiguration<TransferenciaInventario>
{
    public void Configure(EntityTypeBuilder<TransferenciaInventario> builder)
    {
        // Table configuration
        builder.ToTable("TransferenciasInventario");
        
        // Primary key
        builder.HasKey(t => t.TransferenciaId);
        
        // Properties configuration
        builder.Property(t => t.TransferenciaId)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(t => t.InstitucionID)
            .IsRequired();

        builder.Property(t => t.NumeroTransferencia)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.TipoUbicacionOrigen)
            .IsRequired();

        builder.Property(t => t.TipoUbicacionDestino)
            .IsRequired();

        builder.Property(t => t.Estado)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(EstadoTransferencia.Pendiente);

        builder.Property(t => t.Prioridad)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(PrioridadTransferencia.Media);

        builder.Property(t => t.Motivo)
            .HasMaxLength(500);

        builder.Property(t => t.Notas)
            .HasMaxLength(1000);

        builder.Property(t => t.RequiereAprobacion)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.UsuarioAprobacion)
            .HasMaxLength(450);

        builder.Property(t => t.ComentariosAprobacion)
            .HasMaxLength(500);

        builder.Property(t => t.UsuarioRechazo)
            .HasMaxLength(450);

        builder.Property(t => t.MotivoRechazo)
            .HasMaxLength(500);

        builder.Property(t => t.UsuarioCompletado)
            .HasMaxLength(450);

        builder.Property(t => t.NotasCompletado)
            .HasMaxLength(500);

        builder.Property(t => t.FechaCreacion)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(t => t.UsuarioCreacion)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(t => t.UsuarioActualizacion)
            .HasMaxLength(450);

        builder.Property(t => t.DireccionIP)
            .HasMaxLength(45);

        // Relationships
        builder.HasOne(t => t.Institucion)
            .WithMany()
            .HasForeignKey(t => t.InstitucionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CreadoPor)
            .WithMany()
            .HasForeignKey(t => t.UsuarioCreacion)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AprobadoPor)
            .WithMany()
            .HasForeignKey(t => t.UsuarioAprobacion)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CompletadoPor)
            .WithMany()
            .HasForeignKey(t => t.UsuarioCompletado)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint
        builder.HasIndex(t => t.NumeroTransferencia)
            .IsUnique()
            .HasDatabaseName("UQ_TransferenciasInventario_NumeroTransferencia");

        // Indexes
        builder.HasIndex(t => t.InstitucionID)
            .HasDatabaseName("IX_TransferenciasInventario_InstitucionID");

        builder.HasIndex(t => t.Estado)
            .HasDatabaseName("IX_TransferenciasInventario_Estado");

        builder.HasIndex(t => t.Prioridad)
            .HasDatabaseName("IX_TransferenciasInventario_Prioridad");

        builder.HasIndex(t => t.FechaCreacion)
            .HasDatabaseName("IX_TransferenciasInventario_FechaCreacion");

        builder.HasIndex(t => t.UsuarioCreacion)
            .HasDatabaseName("IX_TransferenciasInventario_UsuarioCreacion");

        // Check constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_TransferenciasInventario_Estado", 
            "[Estado] IN ('Pendiente', 'Aprobada', 'Rechazada', 'EnProceso', 'Completada', 'Cancelada', 'ParcialmenteCompletada')"));
        
        builder.ToTable(t => t.HasCheckConstraint("CK_TransferenciasInventario_Prioridad", 
            "[Prioridad] IN ('Baja', 'Media', 'Alta', 'Urgente')"));
    }
}

/// <summary>
/// Entity Framework configuration for DetalleTransferenciaInventario
/// </summary>
public class DetalleTransferenciaInventarioConfiguration : IEntityTypeConfiguration<DetalleTransferenciaInventario>
{
    public void Configure(EntityTypeBuilder<DetalleTransferenciaInventario> builder)
    {
        // Table configuration
        builder.ToTable("DetallesTransferenciaInventario");
        
        // Primary key
        builder.HasKey(d => d.DetalleId);
        
        // Properties configuration
        builder.Property(d => d.DetalleId)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(d => d.TransferenciaId)
            .IsRequired();

        builder.Property(d => d.InventarioId)
            .IsRequired();

        builder.Property(d => d.ArticuloId)
            .IsRequired();

        builder.Property(d => d.CantidadSolicitada)
            .IsRequired();

        builder.Property(d => d.Notas)
            .HasMaxLength(500);

        builder.Property(d => d.MotivoFallo)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(d => d.Transferencia)
            .WithMany(t => t.Detalles)
            .HasForeignKey(d => d.TransferenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Inventario)
            .WithMany()
            .HasForeignKey(d => d.InventarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Articulo)
            .WithMany()
            .HasForeignKey(d => d.ArticuloId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(d => d.TransferenciaId)
            .HasDatabaseName("IX_DetallesTransferenciaInventario_TransferenciaId");

        builder.HasIndex(d => d.InventarioId)
            .HasDatabaseName("IX_DetallesTransferenciaInventario_InventarioId");

        builder.HasIndex(d => d.ArticuloId)
            .HasDatabaseName("IX_DetallesTransferenciaInventario_ArticuloId");

        builder.HasIndex(d => d.FueTransferido)
            .HasDatabaseName("IX_DetallesTransferenciaInventario_FueTransferido");

        // Check constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_DetallesTransferenciaInventario_CantidadSolicitada", 
            "[CantidadSolicitada] > 0"));
        
        builder.ToTable(t => t.HasCheckConstraint("CK_DetallesTransferenciaInventario_CantidadTransferida", 
            "[CantidadTransferida] >= 0"));
    }
}