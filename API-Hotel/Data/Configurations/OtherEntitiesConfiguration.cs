using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using hotel.Models;

namespace hotel.Data.Configurations
{
    public class VisitasConfiguration : IEntityTypeConfiguration<Visitas>
    {
        public void Configure(EntityTypeBuilder<Visitas> builder)
        {
            builder.HasKey(e => e.VisitaId);
            
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
            
            builder.HasOne(d => d.Habitacion)
                .WithMany(p => p.Reservas)
                .HasForeignKey(d => d.HabitacionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
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
            
            // Configurar relación con Imagen
            builder.HasOne(d => d.Imagen)
                .WithMany()
                .HasForeignKey(d => d.imagenID)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}