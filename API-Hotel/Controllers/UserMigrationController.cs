using hotel.Data;
using hotel.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotel.Controllers;

public class UserInstitutionDto
{
    public int UsuarioId { get; set; }
    public int InstitucionID { get; set; }
}

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "Administrator")]
public class UserMigrationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly HotelDbContext _context;
    private readonly ILogger<UserMigrationController> _logger;

    public UserMigrationController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        HotelDbContext context,
        ILogger<UserMigrationController> logger
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    [HttpPost("create-identity-tables")]
    public async Task<IActionResult> CreateIdentityTables()
    {
        try
        {
            await hotel.CreateIdentityTables.CreateIdentityTablesAsync(_context);
            return Ok(new { Success = true, Message = "Identity tables created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Identity tables");
            return BadRequest(
                new
                {
                    Success = false,
                    Message = "Failed to create Identity tables",
                    Error = ex.Message,
                }
            );
        }
    }

    [HttpPost("migrate-users")]
    public async Task<IActionResult> MigrateUsersFromLegacyTable()
    {
        try
        {
            // Get all users from the legacy Usuarios table with roles and institutions
            var legacyUsers = await (
                from u in _context.Usuarios
                join r in _context.HotelRoles on u.RolId equals r.RolId into roles
                from rol in roles.DefaultIfEmpty()
                join ui in _context.UsuariosInstituciones
                    on u.UsuarioId equals ui.UsuarioId
                    into instituciones
                from inst in instituciones.DefaultIfEmpty()
                where !string.IsNullOrEmpty(u.NombreUsuario)
                select new
                {
                    Usuario = u,
                    Rol = rol,
                    InstitucionId = inst != null ? (int?)inst.InstitucionID : null,
                }
            ).ToListAsync();

            var migratedUsers = new List<object>();
            var errors = new List<string>();

            foreach (var legacyData in legacyUsers)
            {
                var legacyUser = legacyData.Usuario;
                var userRole = legacyData.Rol;

                // Trim username to remove any whitespace
                string trimmedUsername = legacyUser.NombreUsuario.Trim();

                try
                {
                    // Create fake email for legacy users
                    string email = trimmedUsername.Contains("@")
                        ? trimmedUsername
                        : $"{trimmedUsername}@hotel.fake";

                    // Check if user already exists in Identity (by email)
                    var existingUser = await _userManager.FindByEmailAsync(email);
                    if (existingUser != null)
                    {
                        _logger.LogInformation(
                            $"User {trimmedUsername} already exists - updating institution and roles"
                        );

                        // Update existing user's institution and legacy user ID
                        existingUser.InstitucionId = legacyData.InstitucionId;
                        existingUser.LegacyUserId = legacyUser.UsuarioId;
                        await _userManager.UpdateAsync(existingUser);

                        // Update roles
                        var currentRoles = await _userManager.GetRolesAsync(existingUser);
                        if (currentRoles.Any())
                        {
                            await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                        }

                        string roleName = MapLegacyRoleToIdentityRole(userRole?.NombreRol);
                        if (!string.IsNullOrEmpty(roleName))
                        {
                            // Ensure role exists
                            if (!await _roleManager.RoleExistsAsync(roleName))
                            {
                                await _roleManager.CreateAsync(
                                    new ApplicationRole { Name = roleName }
                                );
                            }

                            await _userManager.AddToRoleAsync(existingUser, roleName);
                        }

                        migratedUsers.Add(
                            new
                            {
                                LegacyUserId = legacyUser.UsuarioId,
                                NewUserId = existingUser.Id,
                                Username = existingUser.UserName,
                                Email = existingUser.Email,
                                Role = roleName,
                                InstitucionId = existingUser.InstitucionId,
                                LegacyUserIdReference = existingUser.LegacyUserId,
                                ForcePasswordChange = existingUser.ForcePasswordChange,
                                Updated = true,
                                PasswordMigrated = false,
                            }
                        );

                        continue;
                    }

                    // Create new ApplicationUser
                    var newUser = new ApplicationUser
                    {
                        UserName = trimmedUsername,
                        Email = email,
                        FirstName = ExtractFirstName(trimmedUsername),
                        LastName = ExtractLastName(trimmedUsername),
                        EmailConfirmed = true, // Auto-confirm for migrated users
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        ForcePasswordChange = false, // Keep existing passwords
                        // Set institution from the join query
                        InstitucionId = legacyData.InstitucionId,
                        // Store the legacy user ID for reference
                        LegacyUserId = legacyUser.UsuarioId,
                    };

                    // Create user without password first
                    var result = await _userManager.CreateAsync(newUser);

                    if (result.Succeeded)
                    {
                        // Set the BCrypt password hash directly
                        newUser.PasswordHash = legacyUser.Contraseña; // BCrypt hash from legacy system
                        await _userManager.UpdateAsync(newUser);

                        // Assign role based on legacy role
                        string roleName = MapLegacyRoleToIdentityRole(userRole?.NombreRol);
                        if (!string.IsNullOrEmpty(roleName))
                        {
                            // Ensure role exists
                            if (!await _roleManager.RoleExistsAsync(roleName))
                            {
                                await _roleManager.CreateAsync(
                                    new ApplicationRole { Name = roleName }
                                );
                            }

                            await _userManager.AddToRoleAsync(newUser, roleName);
                        }

                        migratedUsers.Add(
                            new
                            {
                                LegacyUserId = legacyUser.UsuarioId,
                                NewUserId = newUser.Id,
                                Username = newUser.UserName,
                                Email = newUser.Email,
                                Role = roleName,
                                InstitucionId = newUser.InstitucionId,
                                LegacyUserIdReference = newUser.LegacyUserId,
                                ForcePasswordChange = newUser.ForcePasswordChange,
                                Updated = false,
                                PasswordMigrated = true,
                            }
                        );

                        _logger.LogInformation($"Successfully migrated user: {trimmedUsername}");
                    }
                    else
                    {
                        var errorMessage =
                            $"Failed to create user {trimmedUsername}: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                        errors.Add(errorMessage);
                        _logger.LogError(errorMessage);
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error migrating user {trimmedUsername}: {ex.Message}";
                    errors.Add(errorMessage);
                    _logger.LogError(ex, errorMessage);
                }
            }

            var newUsers = migratedUsers.Where(u => !(bool)((dynamic)u).Updated).Count();
            var updatedUsers = migratedUsers.Where(u => (bool)((dynamic)u).Updated).Count();

            return Ok(
                new
                {
                    Success = true,
                    Message = $"Migration completed. {newUsers} new users created, {updatedUsers} existing users updated.",
                    MigratedUsers = migratedUsers,
                    Errors = errors,
                    TotalProcessed = legacyUsers.Count,
                    NewUsersMigrated = newUsers,
                    ExistingUsersUpdated = updatedUsers,
                    FailedMigrations = errors.Count,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user migration process");
            return BadRequest(
                new
                {
                    Success = false,
                    Message = "Migration failed",
                    Error = ex.Message,
                }
            );
        }
    }

    [HttpGet("check-database-structure")]
    public async Task<IActionResult> CheckDatabaseStructure()
    {
        try
        {
            // Simple query to test UsuariosInstituciones access
            var count = await _context
                .Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM UsuariosInstituciones")
                .FirstOrDefaultAsync();

            return Ok(
                new
                {
                    TableName = "UsuariosInstituciones",
                    RecordCount = count,
                    Message = "Table accessible without ApplicationUserId",
                }
            );
        }
        catch (Exception ex)
        {
            return BadRequest(
                new { Error = ex.Message, Message = "Error accessing UsuariosInstituciones table" }
            );
        }
    }

    [HttpGet("migration-preview")]
    public async Task<IActionResult> PreviewMigration()
    {
        try
        {
            var legacyUsers = await (
                from u in _context.Usuarios
                join r in _context.HotelRoles on u.RolId equals r.RolId into roles
                from rol in roles.DefaultIfEmpty()
                where !string.IsNullOrEmpty(u.NombreUsuario)
                select new
                {
                    u.UsuarioId,
                    u.NombreUsuario,
                    RoleName = rol != null ? rol.NombreRol : "Sin Rol",
                    WillBeMigratedAs = new
                    {
                        Username = u.NombreUsuario,
                        Email = u.NombreUsuario.Contains("@")
                            ? u.NombreUsuario
                            : $"{u.NombreUsuario}@hotel.fake",
                        FirstName = ExtractFirstName(u.NombreUsuario),
                        LastName = ExtractLastName(u.NombreUsuario),
                        Role = MapLegacyRoleToIdentityRole(rol != null ? rol.NombreRol : null),
                        ForcePasswordChange = false,
                        PasswordWillBeMigrated = true,
                    },
                }
            ).ToListAsync();

            return Ok(new { TotalUsersToMigrate = legacyUsers.Count, UsersPreview = legacyUsers });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating migration preview");
            return BadRequest(
                new
                {
                    Success = false,
                    Message = "Failed to generate migration preview",
                    Error = ex.Message,
                }
            );
        }
    }

    private static string ExtractFirstName(string username)
    {
        // Simple logic to extract first name from username
        // You can customize this based on your username format
        if (username.Contains("@"))
        {
            return username.Split('@')[0];
        }

        if (username.Contains("."))
        {
            return username.Split('.')[0];
        }

        return username;
    }

    private static string ExtractLastName(string username)
    {
        // Simple logic to extract last name from username
        // You can customize this based on your username format
        if (username.Contains("."))
        {
            var parts = username.Split('.');
            if (parts.Length > 1)
            {
                return parts[1].Contains("@") ? parts[1].Split('@')[0] : parts[1];
            }
        }

        return "";
    }

    private static string MapLegacyRoleToIdentityRole(string? legacyRoleName)
    {
        // Map legacy roles to Identity roles based on the database screenshot
        return legacyRoleName?.ToUpper() switch
        {
            "DIRECTOR" => "Director",
            "ADMINISTRADOR" or "ADMIN" or "EXCELENCIAADMIN" or "ADMINISTRADOR DEL SISTEMA" =>
                "Administrator",
            "MUCAMA" => "Mucama",
            "CAJERO" => "Cajero",
            "CAJERO STOCK" => "Cajero Stock",
            "USUARIO" => "User",
            _ => "User", // Default role
        };
    }
}

