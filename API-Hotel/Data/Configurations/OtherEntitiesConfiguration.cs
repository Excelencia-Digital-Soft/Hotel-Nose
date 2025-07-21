using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;
using hotel.Models.Identity;

namespace hotel.Data.Configurations
{
    public class VisitasConfiguration : IEntityTypeConfiguration<Visitas>
    {
        public void Configure(EntityTypeBuilder<Visitas> builder)
        {
            builder.HasKey(e => e.VisitaId);
            
            // Configurar tabla explícitamente
            builder.ToTable("Visitas");
            
            // Configurar propiedades con nombres de columna explícitos según estructura de DB
            builder.Property(e => e.VisitaId)
                .HasColumnName("VisitaID")
                .ValueGeneratedOnAdd();
                
            builder.Property(e => e.UserId)
                .HasColumnName("UserId");
            
            builder.Property(e => e.InstitucionID)
                .HasColumnName("InstitucionID");
            
            // Configure HabitacionId column mapping (will be added to database via SQL script)
            builder.Property(e => e.HabitacionId)
                .HasColumnName("HabitacionId")
                .IsRequired(false); // Nullable
            
            // Configurar relación OPCIONAL con ApplicationUser (LEFT JOIN)
            builder.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Visitas_AspNetUsers")
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
            
            // Configurar relación OPCIONAL con Institucion (LEFT JOIN)
            // Usar string literal para evitar shadow properties automáticas
            builder.HasOne(d => d.Institucion)
                .WithMany() // NO usar p => p.Visitas para evitar conflictos bidireccionales
                .HasForeignKey(d => d.InstitucionID) // Usar la propiedad existente
                .HasConstraintName("FKVisitas_Institucion") // Usar el nombre exacto de la DB
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
            
            // Configure relationship with Habitacion using explicit foreign key
            // This creates a many-to-one relationship (many visits can be for same room over time)
            // Note: The one-to-one relationship (current active visit) is configured in HabitacionesConfiguration
            builder.HasOne<Habitaciones>()
                .WithMany() // No navigation property - removed from model to avoid conflicts
                .HasForeignKey(d => d.HabitacionId)
                .HasConstraintName("FK_Visitas_Habitaciones_HabitacionId")
                .OnDelete(DeleteBehavior.SetNull) // SET NULL on delete to match SQL script
                .IsRequired(false);
        }
    }

    public class ReservasConfiguration : IEntityTypeConfiguration<Reservas>
    {
        public void Configure(EntityTypeBuilder<Reservas> builder)
        {
            builder.HasKey(e => e.ReservaId);
            
            // Configurar tabla explícitamente
            builder.ToTable("Reservas");
            
            // Configurar propiedades con nombres de columna explícitos según estructura de DB
            builder.Property(e => e.ReservaId)
                .HasColumnName("ReservaID")
                .ValueGeneratedOnAdd();
                
            builder.Property(e => e.HabitacionId)
                .HasColumnName("HabitacionID");
            
            builder.Property(e => e.VisitaId)
                .HasColumnName("VisitaID");
            
            builder.Property(e => e.UserId)
                .HasColumnName("UserId");
            
            builder.Property(e => e.InstitucionID)
                .HasColumnName("InstitucionID");
                
            builder.Property(e => e.PromocionId)
                .HasColumnName("PromocionID");
                
            builder.Property(e => e.MovimientoId)
                .HasColumnName("MovimientoID");
            
            // Configurar relación OPCIONAL con Habitacion
            builder.HasOne(d => d.Habitacion)
                .WithMany(p => p.Reservas)
                .HasForeignKey(d => d.HabitacionId)
                .HasConstraintName("FK_Reservas_Habitaciones")
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false);
            
            // Configurar relación OPCIONAL con Visita
            builder.HasOne(d => d.Visita)
                .WithMany(p => p.Reservas)
                .HasForeignKey(d => d.VisitaId)
                .HasConstraintName("FK_Reservas_Visitas")
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false);
            
            // Configurar relación OPCIONAL con ApplicationUser (LEFT JOIN)
            builder.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Reservas_AspNetUsers")
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
            
            // Configurar relación OPCIONAL con Institucion (LEFT JOIN)
            // Usar string literal para evitar shadow properties automáticas
            builder.HasOne(d => d.Institucion)
                .WithMany() // NO usar p => p.Reservas para evitar conflictos bidireccionales
                .HasForeignKey(d => d.InstitucionID) // Usar la propiedad existente
                .HasConstraintName("FKReservas_Institucion") // Usar el nombre exacto de la DB
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
        }
    }

    public class RolesConfiguration : IEntityTypeConfiguration<Roles>
    {
        public void Configure(EntityTypeBuilder<Roles> builder)
        {
            builder.HasKey(e => e.RolId);
            builder.ToTable("Roles"); // Mapear explícitamente a la tabla Roles
        }
    }

    public class InstitucionConfiguration : IEntityTypeConfiguration<Institucion>
    {
        public void Configure(EntityTypeBuilder<Institucion> builder)
        {
            builder.HasKey(e => e.InstitucionId);
            builder.ToTable("Instituciones"); // Mapear explícitamente a la tabla Instituciones
            
            // Mapear la propiedad InstitucionId a la columna InstitucionID
            builder.Property(e => e.InstitucionId)
                .HasColumnName("InstitucionID");
            
            // Ignorar las navigation properties bidireccionales para evitar conflictos
            // Las relaciones se configuran desde el lado "many" (Reservas y Visitas)
            builder.Ignore(e => e.Reservas);
            builder.Ignore(e => e.Visitas);
        }
    }

    public class ArticulosConfiguration : IEntityTypeConfiguration<Articulos>
    {
        public void Configure(EntityTypeBuilder<Articulos> builder)
        {
            builder.HasKey(e => e.ArticuloId);
            
            // Configurar precisión del campo Precio - DECIMAL(18,2)
            builder.Property(e => e.Precio)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            // Configurar relación OPCIONAL con Imagen (LEFT JOIN)
            builder.HasOne(d => d.Imagen)
                .WithMany()
                .HasForeignKey(d => d.imagenID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
            
            // Configurar relación OPCIONAL con Usuario Creador (LEFT JOIN)
            builder.HasOne(d => d.CreadoPor)
                .WithMany()
                .HasForeignKey(d => d.CreadoPorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
            
            // Configurar relación OPCIONAL con Usuario Modificador (LEFT JOIN)
            builder.HasOne(d => d.ModificadoPor)
                .WithMany()
                .HasForeignKey(d => d.ModificadoPorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
        }
    }

    public class ConsumoConfiguration : IEntityTypeConfiguration<Consumo>
    {
        public void Configure(EntityTypeBuilder<Consumo> builder)
        {
            builder.HasKey(e => e.ConsumoId);
            
            // Configurar precisión del campo PrecioUnitario - DECIMAL(18,2)
            builder.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false); // Nullable
        }
    }

    public class PromocionesConfiguration : IEntityTypeConfiguration<Promociones>
    {
        public void Configure(EntityTypeBuilder<Promociones> builder)
        {
            builder.HasKey(e => e.PromocionID);
            
            // Configurar precisión del campo Tarifa - DECIMAL(18,2)
            builder.Property(e => e.Tarifa)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        }
    }

    public class PagosConfiguration : IEntityTypeConfiguration<Pagos>
    {
        public void Configure(EntityTypeBuilder<Pagos> builder)
        {
            builder.HasKey(e => e.PagoId);
            builder.ToTable("Pagos");
            
            // Mapear la propiedad PagoId a la columna PagoID si es necesario
            builder.Property(e => e.PagoId)
                .HasColumnName("PagoID");
            
            builder.Property(e => e.InstitucionID)
                .HasColumnName("InstitucionID");
            
            // Ignorar la navigation property bidireccional para evitar conflictos
            // La relación se configura desde el lado "many" (Movimientos)
            builder.Ignore(e => e.Movimientos);
        }
    }
}