using System;
using System.Collections.Generic;
using ApiObjetos.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace ApiObjetos.Data;

public partial class HotelDbContext : DbContext
{
    public HotelDbContext()
    {
    }

    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Articulos> Articulos { get; set; }

    public virtual DbSet<CategoriasHabitaciones> CategoriasHabitaciones { get; set; }

    public virtual DbSet<Cierre> Cierre { get; set; }

    public virtual DbSet<Consumo> Consumo { get; set; }

    public virtual DbSet<DiasSemana> DiasSemana { get; set; }
    public virtual DbSet<Encargos> Encargos { get; set; }

    public virtual DbSet<Habitaciones> Habitaciones { get; set; }

    public virtual DbSet<HabitacionesVirtuales> HabitacionesVirtuales { get; set; }

    public virtual DbSet<InventarioInicial> InventarioInicial { get; set; }

    public virtual DbSet<Inventarios> Inventarios { get; set; }
    public virtual DbSet<InventarioGeneral> InventarioGeneral { get; set; }

    public virtual DbSet<MediosPago> MediosPago { get; set; }

    public virtual DbSet<Movimientos> Movimientos { get; set; }

    public virtual DbSet<MovimientosServicios> MovimientosServicios { get; set; }

    public virtual DbSet<MovimientosStock> MovimientosStock { get; set; }

    public virtual DbSet<MovimientosUsuarios> MovimientosUsuarios { get; set; }

    public virtual DbSet<Pagos> Pagos { get; set; }
    public virtual DbSet<Recargos> Recargos { get; set; }

    public virtual DbSet<Personal> Personal { get; set; }

    public virtual DbSet<Reservas> Reservas { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<ServiciosAdicionales> ServiciosAdicionales { get; set; }

    public virtual DbSet<Tarifas> Tarifas { get; set; }
    public virtual DbSet<Promociones> Promociones { get; set; }
    public virtual DbSet<TipoMovimiento> TipoMovimiento { get; set; }

    public virtual DbSet<TipoTarifa> TipoTarifa { get; set; }

    public virtual DbSet<Empeño> Empeño{ get; set; }
    public virtual DbSet<CategoriasArticulos> CategoriasArticulos { get; set; }


    public virtual DbSet<Usuarios> Usuarios { get; set; }

    public virtual DbSet<Visitas> Visitas { get; set; }
    public virtual DbSet<Imagenes> Imagenes { get; set; }
    public virtual DbSet<TipoEgreso> TipoEgreso { get; set; }
    public virtual DbSet<Egresos> Egresos { get; set; }
    public virtual DbSet<DescuentoEfectivo> DescuentoEfectivo { get; set; }
    public virtual DbSet<Tarjetas> Tarjetas { get; set; }
    public virtual DbSet<Registros> Registros { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Articulos>(entity =>
        {
            entity.HasKey(e => e.ArticuloId).HasName("PK__Articulo__C0D7258D17E6EF80");

            entity.Property(e => e.ArticuloId).HasColumnName("ArticuloID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreArticulo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
        });

        modelBuilder.Entity<CategoriasHabitaciones>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__Categori__F353C1C5CB370FC8");

            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrecioNormal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PorcentajeXPersona).HasColumnType("int");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
        });

        modelBuilder.Entity<Cierre>(entity =>
        {
            entity.HasKey(e => e.CierreId).HasName("PK__Cierre__0BAD3FDAD7614B59");

            entity.Property(e => e.CierreId)
                .ValueGeneratedOnAdd()  // Change this line
                .HasColumnName("CierreID");

            entity.Property(e => e.FechaHoraCierre).HasColumnType("datetime");
            entity.Property(e => e.MontoInicialCaja).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Observaciones).HasMaxLength(200);
            entity.Property(e => e.TotalIngresosBillVirt).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalIngresosEfectivo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalIngresosTarjeta).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Cierre)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Usuario");
        });

        modelBuilder.Entity<CategoriasArticulos>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK_CategoriaArticulo");
            entity.Property(e => e.CategoriaId)
                .ValueGeneratedOnAdd() // Auto-incrementing primary key
                .HasColumnName("CategoriaID");

            entity.Property(e => e.NombreCategoria)
                .IsRequired() // Mark as required
                .HasMaxLength(50) // Set maximum length to 50
                .HasColumnName("NombreCategoria");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");

        });



        modelBuilder.Entity<Consumo>(entity =>
        {
            entity.HasKey(e => e.ConsumoId).HasName("PK__Consumo__206D9CC6C6EEA4D5");

            entity.Property(e => e.ConsumoId).HasColumnName("ConsumoID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.ArticuloId).HasColumnName("ArticuloID");
            entity.Property(e => e.MovimientosId).HasColumnName("MovimientosID");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Articulo).WithMany(p => p.Consumo)
                .HasForeignKey(d => d.ArticuloId)
                .HasConstraintName("FK__Consumo__Articul__793DFFAF");

            entity.HasOne(d => d.Movimientos).WithMany(p => p.Consumo)
                .HasForeignKey(d => d.MovimientosId)
                .HasConstraintName("FK__Consumo__Movimie__7849DB76");
        });

        modelBuilder.Entity<DiasSemana>(entity =>
        {
            entity.HasKey(e => e.DiaSemanaId).HasName("PK__DiasSema__C5898FE1E9681C02");

            entity.Property(e => e.DiaSemanaId).HasColumnName("DiaSemanaID");
            entity.Property(e => e.NombreDiaSemana)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Habitaciones>(entity =>
        {
            entity.HasKey(e => e.HabitacionId).HasName("PK__Habitaci__11AD4441DE85318D");

            entity.Property(e => e.HabitacionId).HasColumnName("HabitacionID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.Disponible).HasDefaultValueSql("((1))");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreHabitacion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProximaReserva).HasColumnType("datetime");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Habitaciones)
                .HasForeignKey(d => d.CategoriaId)
                .HasConstraintName("FK__Habitacio__Categ__2739D489");
        });

        modelBuilder.Entity<HabitacionesVirtuales>(entity =>
        {
            entity.HasKey(e => e.HabitacionVirtualId).HasName("PK__Habitaci__31AD1AAA5FB01D72");

            entity.Property(e => e.HabitacionVirtualId).HasColumnName("HabitacionVirtualID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Habitacion1Id).HasColumnName("Habitacion1ID");
            entity.Property(e => e.Habitacion2Id).HasColumnName("Habitacion2ID");
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Habitacion1).WithMany(p => p.HabitacionesVirtualesHabitacion1)
                .HasForeignKey(d => d.Habitacion1Id)
                .HasConstraintName("FK__Habitacio__Habit__3B40CD36");

            entity.HasOne(d => d.Habitacion2).WithMany(p => p.HabitacionesVirtualesHabitacion2)
                .HasForeignKey(d => d.Habitacion2Id)
                .HasConstraintName("FK__Habitacio__Habit__3C34F16F");
        });

        modelBuilder.Entity<InventarioInicial>(entity =>
        {
            entity.HasKey(e => e.ArticuloId).HasName("PK__Inventar__C0D7258D7CEAADA9");

            entity.Property(e => e.ArticuloId)
                .ValueGeneratedNever()
                .HasColumnName("ArticuloID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");

            entity.HasOne(d => d.Articulo).WithOne(p => p.InventarioInicial)
                .HasForeignKey<InventarioInicial>(d => d.ArticuloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventari__Artic__7E02B4CC");
        });

        modelBuilder.Entity<Inventarios>(entity =>
        {
            entity.HasKey(e => e.InventarioId).HasName("PK__Inventar__FB8A24B74BCD65D5");

            entity.Property(e => e.InventarioId).HasColumnName("InventarioID");
            entity.Property(e => e.ArticuloId).HasColumnName("ArticuloID");
            entity.Property(e => e.Cantidad).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaRegistro).HasColumnType("date");
            entity.Property(e => e.HabitacionId).HasColumnName("HabitacionID");

            entity.HasOne(d => d.Articulo).WithMany(p => p.Inventarios)
                .HasForeignKey(d => d.ArticuloId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Inventari__Artic__43A1090D");

            entity.HasOne(d => d.Habitacion).WithMany(p => p.Inventarios)
                .HasForeignKey(d => d.HabitacionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Inventari__Habit__42ACE4D4");
        });

        modelBuilder.Entity<InventarioGeneral>(entity =>
        {
            entity.Property(e => e.InventarioId).HasColumnName("InventarioID");
            entity.Property(e => e.ArticuloId).HasColumnName("ArticuloID");
            entity.Property(e => e.Cantidad).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaRegistro).HasColumnType("date");

            entity.HasOne(d => d.Articulo).WithMany(p => p.InventarioGeneral)
                .HasForeignKey(d => d.ArticuloId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Inventari__Artic__43A1090D");

        });

        modelBuilder.Entity<MediosPago>(entity =>
        {
            entity.HasKey(e => e.MedioPagoId).HasName("PK__MediosPa__6D54078ED1D306FC");

            entity.Property(e => e.MedioPagoId).HasColumnName("MedioPagoID");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Movimientos>(entity =>
        {
            entity.HasKey(e => e.MovimientosId).HasName("PK__Movimien__1B3A75F85AC62449");

            entity.Property(e => e.MovimientosId).HasColumnName("MovimientosID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.HabitacionId).HasColumnName("HabitacionID");
            entity.Property(e => e.PagoId).HasColumnName("PagoID");
            entity.Property(e => e.Descripcion).HasMaxLength(50);
            entity.Property(e => e.TotalFacturado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.VisitaId).HasColumnName("VisitaID");

            entity.HasOne(d => d.Habitacion).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.HabitacionId)
                .HasConstraintName("FK__Movimient__Habit__30C33EC3");

            entity.HasOne(d => d.Pago).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.PagoId)
                .HasConstraintName("FK_Movimientos_Pagos");

            entity.HasOne(d => d.Visita).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.VisitaId)
                .HasConstraintName("FK__Movimient__Visit__2FCF1A8A");
        });

        modelBuilder.Entity<MovimientosServicios>(entity =>
        {
            entity.HasKey(e => e.MovimientosServicioId).HasName("PK__Movimien__518225B897E0CDB2");

            entity.Property(e => e.MovimientosServicioId).HasColumnName("MovimientosServicioID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.MovimientosId).HasColumnName("MovimientosID");
            entity.Property(e => e.ServicioId).HasColumnName("ServicioID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Movimientos).WithMany(p => p.MovimientosServicios)
                .HasForeignKey(d => d.MovimientosId)
                .HasConstraintName("FK__Movimient__Movim__42E1EEFE");

            entity.HasOne(d => d.Servicio).WithMany(p => p.MovimientosServicios)
                .HasForeignKey(d => d.ServicioId)
                .HasConstraintName("FK__Movimient__Servi__43D61337");
        });

        modelBuilder.Entity<MovimientosStock>(entity =>
        {
            entity.HasKey(e => e.MovimientoId).HasName("PK__Movimien__BF923FCC1993AA0F");

            entity.ToTable("Movimientos_Stock");

            entity.Property(e => e.MovimientoId).HasColumnName("MovimientoID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.ArticuloId).HasColumnName("ArticuloID");
            entity.Property(e => e.FechaMovimiento).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.MovimientosId).HasColumnName("MovimientosID");
            entity.Property(e => e.TipoMovimientoId).HasColumnName("TipoMovimientoID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Articulo).WithMany(p => p.MovimientosStock)
                .HasForeignKey(d => d.ArticuloId)
                .HasConstraintName("FK__Movimient__Artic__5CA1C101");

            entity.HasOne(d => d.Movimientos).WithMany(p => p.MovimientosStock)
                .HasForeignKey(d => d.MovimientosId)
                .HasConstraintName("FK__Movimient__Movim__5E8A0973");

            entity.HasOne(d => d.TipoMovimiento).WithMany(p => p.MovimientosStock)
                .HasForeignKey(d => d.TipoMovimientoId)
                .HasConstraintName("FK__Movimient__TipoM__5D95E53A");
        });

        modelBuilder.Entity<MovimientosUsuarios>(entity =>
        {
            entity.HasKey(e => e.MovimientoId).HasName("PK__Movimien__BF923FCC31DCE6C3");

            entity.Property(e => e.MovimientoId).HasColumnName("MovimientoID");
            entity.Property(e => e.Accion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaHora).HasColumnType("datetime");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.MovimientosUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientosUsuarios_Usuarios");
        });

        modelBuilder.Entity<Pagos>(entity =>
        {
            entity.HasKey(e => e.PagoId).HasName("PK__Pagos__F00B615896F7F706");

            entity.Property(e => e.PagoId).HasColumnName("PagoID");
            entity.Property(e => e.CierreId).HasColumnName("CierreID");
            entity.Property(e => e.MedioPagoId).HasColumnName("MedioPagoID");
            entity.Property(e => e.MontoBillVirt).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MontoEfectivo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MontoTarjeta).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Observacion).HasMaxLength(200);

            entity.HasOne(d => d.Cierre).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.CierreId)
                .HasConstraintName("FK_Pago_Cierre");

            entity.HasOne(d => d.MedioPago).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.MedioPagoId)
                .HasConstraintName("FK_Pagos_MediosPago");
        });

        modelBuilder.Entity<Recargos>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.RecargoID);

            // Properties
            entity.Property(e => e.RecargoID)
                .HasColumnName("RecargoID")
                .ValueGeneratedOnAdd(); // Identity column

            entity.Property(e => e.Descripcion)
                .HasColumnName("Descripcion")
                .HasMaxLength(200)
                .IsUnicode(true);

            entity.Property(e => e.Valor) 
                .HasColumnName("Valor")
                .HasColumnType("decimal(10, 2)");


            // Foreign key relationship with Pagos table
            entity.HasOne(e => e.Pago)
                   .WithOne(p => p.Recargo) // One-to-one relationship
                   .HasForeignKey<Recargos>(e => e.PagoID) // Foreign key in Recargos table
                   .HasConstraintName("FK_Recargos_Pagos")
                   .OnDelete(DeleteBehavior.Cascade); // Cascade delete behavior
        });
        modelBuilder.Entity<Personal>(entity =>
        {
            entity.HasKey(e => e.PersonalId).HasName("PK__Personal__283437138A5F91D4");

            entity.Property(e => e.PersonalId).HasColumnName("PersonalID");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.RolId).HasColumnName("RolID");

            entity.HasOne(d => d.Rol).WithMany(p => p.Personal)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Roles");
        });

        modelBuilder.Entity<Promociones>(entity =>
        {
            entity.HasKey(e => e.PromocionID).HasName("PK__Promociones");

            entity.Property(e => e.Tarifa).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CantidadHoras).HasColumnType("int");
            entity.Property(e => e.CategoriaID).HasColumnName("CategoriaID");
            entity.Property(e => e.Detalle)
    .HasMaxLength(100)
    .IsUnicode(false);
            entity.HasOne(e => e.Categoria)
                  .WithMany(c => c.Promociones) // Each category has many promociones
                  .HasForeignKey(e => e.CategoriaID) // Foreign key in Promociones
                  .OnDelete(DeleteBehavior.Restrict) // Restrict deletion if related records exist
                  .HasConstraintName("FK_Promociones_CategoriasHabitaciones");
        });

        modelBuilder.Entity<Reservas>(entity =>
        {
            entity.HasKey(e => e.ReservaId).HasName("PK__Reservas__C39937031EAE2A3E");

            entity.Property(e => e.ReservaId).HasColumnName("ReservaID");
            entity.Property(e => e.FechaAnula).HasColumnType("datetime");
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.MovimientoId).HasColumnType("int");
            entity.Property(e => e.FechaReserva).HasColumnType("datetime");
            entity.Property(e => e.HabitacionId).HasColumnName("HabitacionID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.VisitaId).HasColumnName("VisitaID");

            entity.HasOne(d => d.Habitacion).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.HabitacionId)
                .HasConstraintName("FK__Reservas__Habita__2BFE89A6");

            entity.HasOne(d => d.Visita).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.VisitaId)
                .HasConstraintName("FK__Reservas__Visita__2B0A656D");
            entity.HasOne(d => d.Promocion)
                .WithMany()
                .HasForeignKey(d => d.PromocionId)
                .IsRequired(false) 
                .HasConstraintName("FK_Reservas_Promociones");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roles__F92302D13E9BFBA1");

            entity.Property(e => e.RolId).HasColumnName("RolID");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ServiciosAdicionales>(entity =>
        {
            entity.HasKey(e => e.ServicioId).HasName("PK__Servicio__D5AEEC221E25B13E");

            entity.Property(e => e.ServicioId).HasColumnName("ServicioID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreServicio)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
        });

        modelBuilder.Entity<Tarifas>(entity =>
        {
            entity.HasKey(e => e.TarifaId).HasName("PK__Tarifas__E81AC21B585D7CD5");

            entity.Property(e => e.TarifaId).HasColumnName("TarifaID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.DetalleTarifa)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DiaSemanaId).HasColumnName("DiaSemanaID");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TipoTarifaId).HasColumnName("TipoTarifaID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Tarifas)
                .HasForeignKey(d => d.CategoriaId)
                .HasConstraintName("FK__Tarifas__Categor__2180FB33");

            entity.HasOne(d => d.DiaSemana).WithMany(p => p.Tarifas)
                .HasForeignKey(d => d.DiaSemanaId)
                .HasConstraintName("FK__Tarifas__DiaSema__22751F6C");

            entity.HasOne(d => d.TipoTarifa).WithMany(p => p.Tarifas)
                .HasForeignKey(d => d.TipoTarifaId)
                .HasConstraintName("FK__Tarifas__TipoTar__236943A5");
        });

        modelBuilder.Entity<TipoMovimiento>(entity =>
        {
            entity.HasKey(e => e.TipoMovimientoId).HasName("PK__Tipo_Mov__097C73011746A78A");

            entity.ToTable("Tipo_Movimiento");

            entity.Property(e => e.TipoMovimientoId).HasColumnName("TipoMovimientoID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.NombreTipoMovimiento)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TipoTarifa>(entity =>
        {
            entity.HasKey(e => e.TipoTarifaId).HasName("PK__TipoTari__92EDBAA1CC6254D0");

            entity.Property(e => e.TipoTarifaId).HasColumnName("TipoTarifaID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreTipoTarifa)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuarios__2B3DE798F38AEFCF");

            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.Contraseña)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.RolId).HasColumnName("RolID");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        modelBuilder.Entity<Visitas>(entity =>
        {
            entity.HasKey(e => e.VisitaId).HasName("PK__Visitas__D8D4BC22AFD09745");

            entity.Property(e => e.VisitaId).HasColumnName("VisitaID");
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaPrimerIngreso).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Identificador)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NumeroTelefono)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PatenteVehiculo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
        });


        modelBuilder.Entity<Encargos>(entity =>
        {
            entity.HasKey(e => e.EncargosId).HasName("PK_Encargos"); 
            entity.Property(e => e.ArticuloId).HasColumnName("ArticuloId");
            entity.Property(e => e.VisitaId).HasColumnName("VisitaId");
            entity.Property(e => e.CantidadArt).HasDefaultValueSql("((0))");
            entity.Property(e => e.Entregado).HasDefaultValueSql("((0))"); ;
            entity.Property(e => e.FechaCrea).HasDefaultValueSql("GETDATE()"); ;
            entity.Property(e => e.Anulado).HasDefaultValueSql("((0))"); ;

            entity.HasOne(d => d.Articulo).WithMany(p => p.Encargos)
                .HasForeignKey(d => d.ArticuloId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Encargos_Articulos");

            entity.HasOne(d => d.Visita).WithMany(p => p.Encargos)
                .HasForeignKey(d => d.VisitaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Encargos_Visitas");
        });

        modelBuilder.Entity<Empeño>(entity =>
        {
            entity.HasKey(e => e.EmpeñoID).HasName("PK_Empeño");
            entity.Property(e => e.VisitaID).HasColumnName("VisitaID");
            entity.Property(e => e.Detalle).HasColumnName("Detalle");
            entity.Property(e => e.Monto).HasDefaultValueSql("((0))");
            entity.Property(e => e.PagoID).HasColumnName("PagoID");


           /* entity.HasOne(d => d.Pago).WithMany(p => p.Empeño)
                .HasForeignKey(d => d.ArticuloId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Encargos_Articulos");

            entity.HasOne(d => d.Visita).WithMany(p => p.Encargos)
                .HasForeignKey(d => d.VisitaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Encargos_Visitas"); */
        });

        modelBuilder.Entity<Imagenes>(entity =>
        {
            entity.HasKey(e => e.ImagenId).HasName("PK_Imagen");
            entity.Property(e => e.Origen).HasColumnName("Origen");
            entity.Property(e => e.NombreArchivo).HasColumnName("NombreArchivo");

        });

        modelBuilder.Entity<Egresos>(entity =>
        {
            entity.HasKey(e => e.EgresoId);

            entity.Property(e => e.EgresoId).HasColumnName("EgresoID");
            entity.Property(e => e.TipoEgresoId).HasColumnName("TipoEgresoID");
            entity.Property(e => e.Cantidad).HasDefaultValue(0);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Fecha).HasColumnType("datetime").HasDefaultValueSql("getdate()");

            entity.HasOne(d => d.TipoEgreso)
                .WithMany(p => p.Egresos)
                .HasForeignKey(d => d.TipoEgresoId)
                .HasConstraintName("FK_Egresos_TipoEgreso");
        });

        modelBuilder.Entity<TipoEgreso>(entity =>
        {
            entity.HasKey(e => e.TipoEgresoId);

            entity.Property(e => e.TipoEgresoId).HasColumnName("TipoEgresoID");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(true);
        });

        modelBuilder.Entity<Tarjetas>(entity =>
        {
            entity.HasKey(e => e.TarjetaID);

            entity.Property(e => e.TarjetaID).HasColumnName("TarjetaID");
            entity.Property(e => e.MontoPorcentual).HasColumnName("MontoPorcentual");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(true);
        });


        modelBuilder.Entity<DescuentoEfectivo>(entity =>
        {
            entity.HasKey(e => e.DescuentoID);

            entity.Property(e => e.DescuentoID).HasColumnName("DescuentoID");
            entity.Property(e => e.MontoPorcentual).HasColumnName("MontoPorcentual");
            entity.Property(e => e.InstitucionID).HasColumnName("InstitucionID");

        });

        modelBuilder.Entity<Registros>(entity =>
        {
            entity.HasKey(e => e.RegistroID);

            entity.Property(e => e.RegistroID).HasColumnName("RegistroID");
            entity.Property(e => e.Contenido)
                .HasMaxLength(250)
                .IsUnicode(true);
            entity.Property(e => e.ReservaId).HasColumnName("ReservaID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
