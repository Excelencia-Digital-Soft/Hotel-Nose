using System.Reflection;
using hotel.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace hotel.Data;

/// <summary>
/// Contexto principal de Entity Framework para la base de datos del hotel.
/// Hereda de IdentityDbContext para integrar ASP.NET Core Identity.
/// </summary>
public partial class HotelDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public HotelDbContext() { }

    /// <summary>
    /// Constructor con opciones de configuración
    /// </summary>
    /// <param name="options">Opciones de configuración del DbContext</param>
    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options) { }

    /// <summary>
    /// Configura el modelo de Entity Framework
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Llamar a la configuración base de Identity
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones desde el ensamblado actual
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Aplicar configuraciones adicionales definidas en partial classes
        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>
    /// Método parcial para permitir configuraciones adicionales en otras partial classes
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo</param>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    /// <summary>
    /// Guarda los cambios en la base de datos con manejo de auditoría
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de entidades afectadas</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Aquí se puede agregar lógica de auditoría antes de guardar
        // Por ejemplo: actualizar campos de auditoría, registrar cambios, etc.

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Guarda los cambios en la base de datos
    /// </summary>
    /// <returns>Número de entidades afectadas</returns>
    public override int SaveChanges()
    {
        // Aquí se puede agregar lógica de auditoría antes de guardar

        return base.SaveChanges();
    }
}

