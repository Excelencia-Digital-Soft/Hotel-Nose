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
            
            // Configurar relación OPCIONAL con ApplicationUser (LEFT JOIN)
            builder.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
            
            // Configurar relación OPCIONAL con Institucion (LEFT JOIN)
            builder.HasOne(d => d.Institucion)
                .WithMany()
                .HasForeignKey(d => d.InstitucionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
            
            // La relación OneToOne entre Visitas.Habitacion y Habitaciones.Visita
            // ya está configurada en HabitacionesConfiguration
            // No necesitamos configurarla aquí para evitar conflictos
        }
    }

    public class ReservasConfiguration : IEntityTypeConfiguration<Reservas>
    {
        public void Configure(EntityTypeBuilder<Reservas> builder)
        {
            builder.HasKey(e => e.ReservaId);
            
            // Configurar relación OPCIONAL con Habitacion
            builder.HasOne(d => d.Habitacion)
                .WithMany(p => p.Reservas)
                .HasForeignKey(d => d.HabitacionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            
            // Configurar relación OPCIONAL con ApplicationUser (LEFT JOIN)
            builder.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false); // OPCIONAL - genera LEFT JOIN
            
            // Configurar relación OPCIONAL con Institucion (LEFT JOIN)
            builder.HasOne(d => d.Institucion)
                .WithMany()
                .HasForeignKey(d => d.InstitucionID)
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
}