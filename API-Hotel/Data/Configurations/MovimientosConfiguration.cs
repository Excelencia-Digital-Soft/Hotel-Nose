using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    /// <summary>
    /// Configuración para la entidad Movimientos
    /// </summary>
    public class MovimientosConfiguration : IEntityTypeConfiguration<Movimientos>
    {
        public void Configure(EntityTypeBuilder<Movimientos> builder)
        {
            // Configurar tabla
            builder.ToTable("Movimientos");

            // Configurar clave primaria
            builder.HasKey(m => m.MovimientosId);

            // Configurar propiedades según estructura de DB
            builder.Property(m => m.MovimientosId)
                .HasColumnName("MovimientosID")
                .ValueGeneratedOnAdd();

            // Configurar propiedades con nombres de columna explícitos
            builder.Property(m => m.HabitacionId)
                .HasColumnName("HabitacionID");

            builder.Property(m => m.VisitaId)
                .HasColumnName("VisitaID");

            builder.Property(m => m.PagoId)
                .HasColumnName("PagoID");

            builder.Property(m => m.InstitucionID)
                .HasColumnName("InstitucionID");

            // Configurar precisión del campo TotalFacturado - DECIMAL(18,2)
            builder.Property(m => m.TotalFacturado)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false); // Nullable

            builder.Property(m => m.Anulado)
                .HasDefaultValue(false);

            builder.Property(m => m.FechaRegistro)
                .HasDefaultValueSql("GETDATE()");

            // Configurar relación OPCIONAL con Habitacion
            builder.HasOne(m => m.Habitacion)
                .WithMany(h => h.Movimientos)
                .HasForeignKey(m => m.HabitacionId)
                .HasConstraintName("FK_Movimientos_Habitaciones")
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false);

            // Configurar relación OPCIONAL con Visita
            builder.HasOne(m => m.Visita)
                .WithMany(v => v.Movimientos)
                .HasForeignKey(m => m.VisitaId)
                .HasConstraintName("FK_Movimientos_Visitas")
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false);

            // Configurar relación OPCIONAL con Pago
            // Usar WithMany() sin especificar la navigation property de vuelta para evitar conflictos
            builder.HasOne(m => m.Pago)
                .WithMany() // NO usar p => p.Movimientos para evitar shadow properties
                .HasForeignKey(m => m.PagoId)
                .HasConstraintName("FK_Movimientos_Pagos")
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false);

            // Índices
            builder.HasIndex(m => m.InstitucionID)
                .HasDatabaseName("IX_Movimientos_InstitucionID");

            builder.HasIndex(m => m.HabitacionId)
                .HasDatabaseName("IX_Movimientos_HabitacionId");

            builder.HasIndex(m => m.VisitaId)
                .HasDatabaseName("IX_Movimientos_VisitaId");

            builder.HasIndex(m => m.PagoId)
                .HasDatabaseName("IX_Movimientos_PagoId");

            builder.HasIndex(m => m.Anulado)
                .HasDatabaseName("IX_Movimientos_Anulado");

            // Índice compuesto para búsquedas frecuentes
            builder.HasIndex(m => new { m.InstitucionID, m.Anulado })
                .HasDatabaseName("IX_Movimientos_InstitucionID_Anulado");
        }
    }
}