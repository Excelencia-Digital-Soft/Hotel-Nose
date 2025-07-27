using hotel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace hotel.Data.Configurations;

/// <summary>
/// Entity Framework configuration for AlertaInventario
/// </summary>
public class AlertaInventarioConfiguration : IEntityTypeConfiguration<AlertaInventario>
{
    public void Configure(EntityTypeBuilder<AlertaInventario> builder)
    {
        // Table configuration
        builder.ToTable("AlertasInventario");
        
        // Primary key
        builder.HasKey(a => a.AlertaId);
        
        // Properties configuration
        builder.Property(a => a.AlertaId)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(a => a.InventarioId)
            .IsRequired();

        builder.Property(a => a.InstitucionID)
            .IsRequired();

        builder.Property(a => a.TipoAlerta)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.Severidad)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Mensaje)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.CantidadActual)
            .IsRequired();

        builder.Property(a => a.EsActiva)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.FueReconocida)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.FechaCreacion)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(a => a.UsuarioReconocimiento)
            .HasMaxLength(450);

        builder.Property(a => a.UsuarioResolucion)
            .HasMaxLength(450);

        builder.Property(a => a.NotasResolucion)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(a => a.Inventario)
            .WithMany()
            .HasForeignKey(a => a.InventarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Institucion)
            .WithMany()
            .HasForeignKey(a => a.InstitucionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.UsuarioQueReconocio)
            .WithMany()
            .HasForeignKey(a => a.UsuarioReconocimiento)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.UsuarioQueResolvio)
            .WithMany()
            .HasForeignKey(a => a.UsuarioResolucion)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.InventarioId)
            .HasDatabaseName("IX_AlertasInventario_InventarioId");

        builder.HasIndex(a => a.InstitucionID)
            .HasDatabaseName("IX_AlertasInventario_InstitucionID");

        builder.HasIndex(a => a.EsActiva)
            .HasDatabaseName("IX_AlertasInventario_EsActiva");

        builder.HasIndex(a => a.FueReconocida)
            .HasDatabaseName("IX_AlertasInventario_FueReconocida");

        builder.HasIndex(a => a.TipoAlerta)
            .HasDatabaseName("IX_AlertasInventario_TipoAlerta");

        builder.HasIndex(a => a.Severidad)
            .HasDatabaseName("IX_AlertasInventario_Severidad");

        builder.HasIndex(a => a.FechaCreacion)
            .HasDatabaseName("IX_AlertasInventario_FechaCreacion");

        // Check constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_AlertasInventario_TipoAlerta", 
            "[TipoAlerta] IN ('StockBajo', 'StockAlto', 'StockAgotado', 'StockCritico', 'ProximoVencimiento', 'ArticuloInactivo', 'DiscrepanciaInventario')"));
        
        builder.ToTable(t => t.HasCheckConstraint("CK_AlertasInventario_Severidad", 
            "[Severidad] IN ('Baja', 'Media', 'Alta', 'Critica')"));
    }
}

/// <summary>
/// Entity Framework configuration for ConfiguracionAlertaInventario
/// </summary>
public class ConfiguracionAlertaInventarioConfiguration : IEntityTypeConfiguration<ConfiguracionAlertaInventario>
{
    public void Configure(EntityTypeBuilder<ConfiguracionAlertaInventario> builder)
    {
        // Table configuration
        builder.ToTable("ConfiguracionAlertasInventario");
        
        // Primary key
        builder.HasKey(c => c.ConfiguracionId);
        
        // Properties configuration
        builder.Property(c => c.ConfiguracionId)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(c => c.InventarioId)
            .IsRequired();

        builder.Property(c => c.InstitucionID)
            .IsRequired();

        builder.Property(c => c.AlertasStockBajoActivas)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.AlertasStockAltoActivas)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.AlertasStockCriticoActivas)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.NotificacionEmailActiva)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.NotificacionSmsActiva)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.FechaCreacion)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(c => c.UsuarioCreacion)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(c => c.UsuarioActualizacion)
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(c => c.Inventario)
            .WithMany()
            .HasForeignKey(c => c.InventarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Institucion)
            .WithMany()
            .HasForeignKey(c => c.InstitucionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CreadoPor)
            .WithMany()
            .HasForeignKey(c => c.UsuarioCreacion)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ModificadoPor)
            .WithMany()
            .HasForeignKey(c => c.UsuarioActualizacion)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint
        builder.HasIndex(c => c.InventarioId)
            .IsUnique()
            .HasDatabaseName("UQ_ConfiguracionAlertasInventario_InventarioId");

        // Indexes
        builder.HasIndex(c => c.InstitucionID)
            .HasDatabaseName("IX_ConfiguracionAlertasInventario_InstitucionID");

        builder.HasIndex(c => c.UsuarioCreacion)
            .HasDatabaseName("IX_ConfiguracionAlertasInventario_UsuarioCreacion");
    }
}