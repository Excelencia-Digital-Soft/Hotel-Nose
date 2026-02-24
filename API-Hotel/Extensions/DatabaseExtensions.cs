using hotel.Data;
using Microsoft.EntityFrameworkCore;

namespace hotel.Extensions;

/// <summary>
/// Database extensions for automatic migration and initialization
/// This replaces the manual SQL scripts approach
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Ensures the database is created and migrations are applied
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="applyMigrations">Whether to automatically apply pending migrations</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder EnsureDatabase(this IApplicationBuilder app, bool applyMigrations = true)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HotelDbContext>>();

        try
        {
            logger.LogInformation("Checking database state...");

            if (applyMigrations)
            {
                // Apply any pending migrations
                var pendingMigrations = context.Database.GetPendingMigrations().ToList();

                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations: {Migrations}",
                        pendingMigrations.Count,
                        string.Join(", ", pendingMigrations));

                    context.Database.Migrate();

                    logger.LogInformation("Database migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("Database is up to date, no migrations needed");
                }
            }
            else
            {
                // Just ensure database exists without applying migrations
                context.Database.EnsureCreated();
                logger.LogInformation("Database existence verified");
            }

            // Verify critical tables exist and ensure required columns
            VerifyDatabaseTables(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database initialization");
            throw;
        }

        return app;
    }

    /// <summary>
    /// Seeds default data for roles and system configuration
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder SeedDefaultData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HotelDbContext>>();

        try
        {
            logger.LogInformation("Checking for default data seeding...");

            // Seed default roles if they don't exist
            SeedDefaultRoles(context, logger);

            // Seed default configuration if it doesn't exist
            SeedDefaultConfiguration(context, logger);

            context.SaveChanges();
            logger.LogInformation("Default data seeding completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during default data seeding");
            throw;
        }

        return app;
    }

    /// <summary>
    /// Provides a summary of the database migration status
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder LogDatabaseStatus(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HotelDbContext>>();

        try
        {
            var appliedMigrations = context.Database.GetAppliedMigrations().ToList();
            var pendingMigrations = context.Database.GetPendingMigrations().ToList();

            logger.LogInformation("Database Status Summary:");
            logger.LogInformation("- Applied migrations: {Count}", appliedMigrations.Count);
            logger.LogInformation("- Pending migrations: {Count}", pendingMigrations.Count);

            if (pendingMigrations.Any())
            {
                logger.LogWarning("Pending migrations found: {Migrations}",
                    string.Join(", ", pendingMigrations));
            }

            // Check if legacy user migration is needed
            CheckLegacyMigrationStatus(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking database status");
        }

        return app;
    }

    private static void VerifyDatabaseTables(HotelDbContext context, ILogger logger)
    {
        var criticalTables = new[]
        {
            "AspNetUsers", "AspNetRoles", "AspNetUserRoles",
            "Usuarios", "Habitaciones", "Instituciones", "Roles", "Reservas", "Visitas", "Movimientos"
        };

        foreach (var tableName in criticalTables)
        {
            try
            {
                var tableExists = context.Database.SqlQueryRaw<int>(
                    $"SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'"
                ).ToList().FirstOrDefault() > 0;

                if (tableExists)
                {
                    logger.LogDebug("✅ Table {TableName} exists", tableName);

                    // Ensure required columns exist for active tables
                    // This is necessary because the project often adds columns to models without migrations
                    if (tableName == "Habitaciones" || tableName == "Reservas" ||
                        tableName == "Visitas" || tableName == "Movimientos")
                    {
                        var columns = context.Database.SqlQueryRaw<string>(
                            $"SELECT COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'"
                        ).ToList();

                        if (tableName == "Habitaciones" && !columns.Contains("Anulado", StringComparer.OrdinalIgnoreCase))
                        {
                            logger.LogWarning("Adding missing Anulado column to Habitaciones table");
                            context.Database.ExecuteSqlRaw("ALTER TABLE Habitaciones ADD Anulado BIT NULL DEFAULT 0");
                        }

                        if (tableName == "Reservas" && !columns.Contains("CierreID", StringComparer.OrdinalIgnoreCase))
                        {
                            logger.LogWarning("Adding missing CierreID column to Reservas table");
                            context.Database.ExecuteSqlRaw("ALTER TABLE Reservas ADD CierreID INT NULL");
                        }

                        if (tableName == "Visitas" && !columns.Contains("HabitacionId", StringComparer.OrdinalIgnoreCase))
                        {
                            logger.LogWarning("Adding missing HabitacionId column to Visitas table");
                            context.Database.ExecuteSqlRaw("ALTER TABLE Visitas ADD HabitacionId INT NULL");
                        }

                        if (tableName == "Movimientos")
                        {
                            if (!columns.Contains("UserId", StringComparer.OrdinalIgnoreCase))
                            {
                                logger.LogWarning("Adding missing UserId column to Movimientos table");
                                context.Database.ExecuteSqlRaw("ALTER TABLE Movimientos ADD UserId NVARCHAR(450) NULL");
                            }
                            if (!columns.Contains("Anulado", StringComparer.OrdinalIgnoreCase))
                            {
                                logger.LogWarning("Adding missing Anulado column to Movimientos table");
                                context.Database.ExecuteSqlRaw("ALTER TABLE Movimientos ADD Anulado BIT NULL DEFAULT 0");
                            }
                        }
                    }
                }
                else
                {
                    logger.LogWarning("❌ Table {TableName} not found", tableName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking table {TableName}", tableName);
            }
        }
    }

    private static void SeedDefaultRoles(HotelDbContext context, ILogger logger)
    {
        var defaultRoles = new[]
        {
            new { Name = "Administrator", Description = "Administrador de sistemas con acceso completo" },
            new { Name = "Director", Description = "Director de hotel con acceso a la gestión" },
            new { Name = "Mucama", Description = "Miembro del personal de limpieza" },
            new { Name = "Cajero", Description = "Cajero con acceso a transacciones financieras" },
            new { Name = "Cajero Stock", Description = "Cajero de stock con acceso al inventario" },
            new { Name = "User", Description = "Basic user with minimal access" }
        };

        var existingRoles = context.Roles.Select(r => r.Name).ToHashSet();
        var newRolesAdded = 0;

        foreach (var role in defaultRoles)
        {
            if (!existingRoles.Contains(role.Name))
            {
                context.Roles.Add(new Models.Identity.ApplicationRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = role.Name,
                    NormalizedName = role.Name.ToUpper(),
                    Description = role.Description,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                newRolesAdded++;
            }
        }

        if (newRolesAdded > 0)
        {
            logger.LogInformation("Added {Count} default roles", newRolesAdded);
        }
    }

    private static void SeedDefaultConfiguration(HotelDbContext context, ILogger logger)
    {
        try
        {
            var hasConfig = context.Configuraciones.Any();

            if (!hasConfig)
            {
                logger.LogInformation("No configuration found, seeding default placeholder");
                // Original seeding logic or placeholder
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Configuracion table may not exist yet");
        }
    }

    private static void CheckLegacyMigrationStatus(HotelDbContext context, ILogger logger)
    {
        try
        {
            var legacyUserCount = context.Usuarios.Count();
            var migratedUserCount = context.Users.Count(u => u.LegacyUserId != null);

            logger.LogInformation("Legacy Migration Status:");
            logger.LogInformation("- Legacy users: {LegacyCount}", legacyUserCount);
            logger.LogInformation("- Migrated users: {MigratedCount}", migratedUserCount);

            if (legacyUserCount > 0 && migratedUserCount == 0)
            {
                logger.LogWarning("⚠️  Legacy users found but no migration detected");
            }
            else if (migratedUserCount > 0)
            {
                var needPasswordChange = context.Users.Count(u => u.ForcePasswordChange);
                logger.LogInformation("✅ User migration detected");

                if (needPasswordChange > 0)
                {
                    logger.LogInformation("📋 Users requiring password change: {Count}", needPasswordChange);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not check legacy migration status");
        }
    }
}