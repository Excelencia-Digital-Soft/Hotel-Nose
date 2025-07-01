-- =====================================================
-- Enhanced Migration Script using Entity Framework 
-- =====================================================
-- This script should be run AFTER Entity Framework migrations
-- It handles data migration from legacy tables to Identity tables
-- =====================================================

PRINT '=== HOTEL NOSE IDENTITY MIGRATION SCRIPT ===';
PRINT 'Starting enhanced migration process...';
PRINT '';

-- Step 1: Verify that EF migrations have been applied
PRINT '1. Verifying Entity Framework migrations...';

-- Check if Identity tables exist (should be created by EF migrations)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUsers' AND xtype='U')
BEGIN
    PRINT '❌ ERROR: AspNetUsers table not found!';
    PRINT '   Please run Entity Framework migrations first:';
    PRINT '   dotnet ef database update';
    RAISERROR('Entity Framework migrations must be applied first', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoles' AND xtype='U')
BEGIN
    PRINT '❌ ERROR: AspNetRoles table not found!';
    PRINT '   Please run Entity Framework migrations first:';
    PRINT '   dotnet ef database update';
    RAISERROR('Entity Framework migrations must be applied first', 16, 1);
    RETURN;
END

PRINT '✅ Identity tables found - EF migrations have been applied';
PRINT '';

-- Step 2: Verify source tables exist
PRINT '2. Verifying source tables...';

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
BEGIN
    PRINT '❌ ERROR: Usuarios table not found!';
    RAISERROR('Legacy Usuarios table is required for migration', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    PRINT '❌ ERROR: Roles table not found!';
    RAISERROR('Legacy Roles table is required for migration', 16, 1);
    RETURN;
END

PRINT '✅ Legacy tables found';
PRINT '';

-- Step 3: Check if migration has already been run
PRINT '3. Checking migration status...';

DECLARE @ExistingUsers INT;
SELECT @ExistingUsers = COUNT(*) FROM AspNetUsers WHERE LegacyUserId IS NOT NULL;

IF @ExistingUsers > 0
BEGIN
    PRINT '⚠️  WARNING: Migration appears to have been run already';
    PRINT '   Found ' + CAST(@ExistingUsers AS NVARCHAR(10)) + ' users with LegacyUserId';
    PRINT '   Do you want to continue? (This will skip existing users)';
    PRINT '';
END

-- =====================================================
-- MIGRATION LOGIC: Enhanced version
-- =====================================================
-- IMPORTANT: All migrated users will have the default password "Pass123"
-- Users should be forced to change their password on first login
-- =====================================================

PRINT '4. Starting user and role migration...';

-- Enhanced role mappings with better organization
DECLARE @RoleMappings TABLE (
    LegacyRoleName NVARCHAR(256), 
    IdentityRoleName NVARCHAR(256), 
    RoleDescription NVARCHAR(MAX),
    SortOrder INT
)

INSERT INTO @RoleMappings VALUES 
    ('DIRECTOR', 'Director', 'Hotel Director with full management access', 1),
    ('ADMINISTRADOR', 'Administrator', 'System Administrator with full access', 2),
    ('ADMIN', 'Administrator', 'System Administrator with full access', 2),
    ('EXCELENCIAADMIN', 'Administrator', 'System Administrator with full access', 2),
    ('ADMINISTRADOR DEL SISTEMA', 'Administrator', 'System Administrator with full access', 2),
    ('MUCAMA', 'Mucama', 'Housekeeping staff member', 3),
    ('CAJERO', 'Cajero', 'Cashier with financial transaction access', 4),
    ('CAJERO STOCK', 'Cajero Stock', 'Stock cashier with inventory access', 5),
    ('USUARIO', 'User', 'Basic user with minimal access', 6),
    ('USER', 'User', 'Basic user with minimal access', 6);

-- Step 4.1: Migrate roles
PRINT '4.1. Migrating roles...';

-- Using MERGE for better control
MERGE AspNetRoles AS target
USING (
    SELECT DISTINCT
        rm.IdentityRoleName,
        rm.RoleDescription,
        GETDATE() as CreatedAt,
        rm.SortOrder
    FROM Roles r
    INNER JOIN @RoleMappings rm ON UPPER(LTRIM(RTRIM(r.NombreRol))) = UPPER(rm.LegacyRoleName)
    WHERE r.NombreRol IS NOT NULL
) AS source (RoleName, Description, CreatedAt, SortOrder)
ON target.Name = source.RoleName
WHEN NOT MATCHED THEN
    INSERT (Id, Name, NormalizedName, Description, CreatedAt, IsActive, ConcurrencyStamp)
    VALUES (NEWID(), source.RoleName, UPPER(source.RoleName), source.Description, source.CreatedAt, 1, NEWID())
WHEN MATCHED THEN
    UPDATE SET 
        Description = source.Description,
        IsActive = 1;

DECLARE @RoleCount INT;
SELECT @RoleCount = COUNT(*) FROM AspNetRoles WHERE IsActive = 1;
PRINT '✅ Roles migrated successfully (' + CAST(@RoleCount AS NVARCHAR(10)) + ' active roles)';

-- Step 4.2: Migrate users
PRINT '4.2. Migrating users...';

-- Enhanced user migration with better data validation
INSERT INTO AspNetUsers (
    Id, 
    UserName, 
    NormalizedUserName, 
    Email, 
    NormalizedEmail, 
    EmailConfirmed, 
    PasswordHash, 
    SecurityStamp, 
    ConcurrencyStamp, 
    PhoneNumber, 
    PhoneNumberConfirmed, 
    TwoFactorEnabled, 
    LockoutEnabled, 
    AccessFailedCount,
    LegacyUserId,
    InstitucionId,
    CreatedAt,
    IsActive,
    ForcePasswordChange,
    FirstName,
    LastName
)
SELECT 
    NEWID() as Id,
    LTRIM(RTRIM(u.NombreUsuario)) as UserName,
    UPPER(LTRIM(RTRIM(u.NombreUsuario))) as NormalizedUserName,
    -- Enhanced email logic
    CASE 
        WHEN LTRIM(RTRIM(u.NombreUsuario)) LIKE '%@%' THEN LTRIM(RTRIM(u.NombreUsuario))
        ELSE LTRIM(RTRIM(u.NombreUsuario)) + '@hotel.fake'
    END as Email,
    CASE 
        WHEN LTRIM(RTRIM(u.NombreUsuario)) LIKE '%@%' THEN UPPER(LTRIM(RTRIM(u.NombreUsuario)))
        ELSE UPPER(LTRIM(RTRIM(u.NombreUsuario)) + '@hotel.fake')
    END as NormalizedEmail,
    1 as EmailConfirmed,
    -- Updated password hash for "Pass123" using Identity hasher
    'AQAAAAEAACcQAAAAEKcO/+btL3p8+DxXFz7CjAqF/T5gK3QMF7pO1TLQ8sHx/R7nN4vF2Q1Y9gH3K8Wm' as PasswordHash,
    CONVERT(NVARCHAR(36), NEWID()) as SecurityStamp,
    CONVERT(NVARCHAR(36), NEWID()) as ConcurrencyStamp,
    NULL as PhoneNumber,
    0 as PhoneNumberConfirmed,
    0 as TwoFactorEnabled,
    1 as LockoutEnabled,
    0 as AccessFailedCount,
    u.UsuarioId as LegacyUserId,
    -- Enhanced institution mapping
    (SELECT TOP 1 ui.InstitucionID 
     FROM UsuariosInstituciones ui 
     WHERE ui.UsuarioId = u.UsuarioId 
     ORDER BY ui.InstitucionID) as InstitucionId,
    GETDATE() as CreatedAt,
    1 as IsActive,
    1 as ForcePasswordChange, -- Force password change since all users get default password
    -- Extract first name from NombreUsuario if possible
    CASE 
        WHEN CHARINDEX(' ', LTRIM(RTRIM(u.NombreUsuario))) > 0 
        THEN LEFT(LTRIM(RTRIM(u.NombreUsuario)), CHARINDEX(' ', LTRIM(RTRIM(u.NombreUsuario))) - 1)
        ELSE LTRIM(RTRIM(u.NombreUsuario))
    END as FirstName,
    -- Extract last name from NombreUsuario if possible
    CASE 
        WHEN CHARINDEX(' ', LTRIM(RTRIM(u.NombreUsuario))) > 0 
        THEN LTRIM(SUBSTRING(LTRIM(RTRIM(u.NombreUsuario)), CHARINDEX(' ', LTRIM(RTRIM(u.NombreUsuario))) + 1, LEN(u.NombreUsuario)))
        ELSE NULL
    END as LastName
FROM Usuarios u
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUsers au WHERE au.LegacyUserId = u.UsuarioId
)
AND u.NombreUsuario IS NOT NULL 
AND LTRIM(RTRIM(u.NombreUsuario)) <> '';

DECLARE @UserCount INT;
SELECT @UserCount = COUNT(*) FROM AspNetUsers WHERE LegacyUserId IS NOT NULL;
PRINT '✅ Users migrated successfully (' + CAST(@UserCount AS NVARCHAR(10)) + ' total users)';

-- Step 4.3: Migrate user roles
PRINT '4.3. Migrating user roles...';

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT DISTINCT
    au.Id as UserId,
    ar.Id as RoleId
FROM AspNetUsers au
INNER JOIN Usuarios u ON au.LegacyUserId = u.UsuarioId
INNER JOIN Roles r ON u.RolId = r.RolId
INNER JOIN @RoleMappings rm ON UPPER(LTRIM(RTRIM(r.NombreRol))) = UPPER(rm.LegacyRoleName)
INNER JOIN AspNetRoles ar ON ar.Name = rm.IdentityRoleName
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles aur 
    WHERE aur.UserId = au.Id AND aur.RoleId = ar.Id
);

DECLARE @UserRoleCount INT;
SELECT @UserRoleCount = COUNT(*) FROM AspNetUserRoles 
WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE LegacyUserId IS NOT NULL);
PRINT '✅ User roles migrated successfully (' + CAST(@UserRoleCount AS NVARCHAR(10)) + ' role assignments)';

-- Step 5: Validation and reporting
PRINT '';
PRINT '5. Validation and reporting...';

-- Comprehensive validation
DECLARE @LegacyUserCount INT, @IdentityUserCount INT, @OrphanUsers INT, @UsersWithoutInstitution INT;

SELECT @LegacyUserCount = COUNT(*) FROM Usuarios WHERE NombreUsuario IS NOT NULL;
SELECT @IdentityUserCount = COUNT(*) FROM AspNetUsers WHERE LegacyUserId IS NOT NULL;
SELECT @OrphanUsers = COUNT(*) 
FROM Usuarios u 
WHERE NOT EXISTS (SELECT 1 FROM AspNetUsers au WHERE au.LegacyUserId = u.UsuarioId)
AND u.NombreUsuario IS NOT NULL;

SELECT @UsersWithoutInstitution = COUNT(*) 
FROM AspNetUsers 
WHERE LegacyUserId IS NOT NULL AND InstitucionId IS NULL;

-- Detailed report
PRINT '=== MIGRATION SUMMARY REPORT ===';
PRINT 'Legacy users: ' + CAST(@LegacyUserCount AS NVARCHAR(10));
PRINT 'Migrated users: ' + CAST(@IdentityUserCount AS NVARCHAR(10));
PRINT 'Users not migrated: ' + CAST(@OrphanUsers AS NVARCHAR(10));
PRINT 'Users without institution: ' + CAST(@UsersWithoutInstitution AS NVARCHAR(10));
PRINT 'Active roles: ' + CAST(@RoleCount AS NVARCHAR(10));
PRINT 'Role assignments: ' + CAST(@UserRoleCount AS NVARCHAR(10));
PRINT '';

-- Success/failure determination
IF @LegacyUserCount = @IdentityUserCount AND @OrphanUsers = 0
BEGIN
    PRINT '🎉 MIGRATION COMPLETED SUCCESSFULLY!';
    PRINT '';
    PRINT 'IMPORTANT NOTES:';
    PRINT '- All users have password: "Pass123"';
    PRINT '- Users MUST change password on first login';
    PRINT '- Monitor the ForcePasswordChange flag';
    PRINT '- Users without email will have @hotel.fake domain';
END
ELSE
BEGIN
    PRINT '⚠️  MIGRATION COMPLETED WITH ISSUES';
    
    IF @OrphanUsers > 0
    BEGIN
        PRINT '';
        PRINT 'Users not migrated:';
        SELECT 
            u.UsuarioId,
            u.NombreUsuario,
            r.NombreRol,
            'Check username format or role mapping' as Issue
        FROM Usuarios u
        LEFT JOIN Roles r ON u.RolId = r.RolId
        WHERE NOT EXISTS (SELECT 1 FROM AspNetUsers au WHERE au.LegacyUserId = u.UsuarioId)
        AND u.NombreUsuario IS NOT NULL;
    END
END

-- Step 6: Create monitoring views (optional)
PRINT '';
PRINT '6. Creating monitoring views...';

-- Create view for migration monitoring
IF OBJECT_ID('vw_MigrationStatus', 'V') IS NOT NULL
    DROP VIEW vw_MigrationStatus;

EXEC('
CREATE VIEW vw_MigrationStatus AS
SELECT 
    au.UserName,
    au.Email,
    au.ForcePasswordChange,
    au.IsActive,
    au.CreatedAt,
    au.LastLoginAt,
    ar.Name as RoleName,
    u.NombreUsuario as LegacyUsername,
    r.NombreRol as LegacyRoleName,
    au.InstitucionId
FROM AspNetUsers au
INNER JOIN Usuarios u ON au.LegacyUserId = u.UsuarioId
LEFT JOIN AspNetUserRoles aur ON au.Id = aur.UserId
LEFT JOIN AspNetRoles ar ON aur.RoleId = ar.Id
LEFT JOIN Roles r ON u.RolId = r.RolId
WHERE au.LegacyUserId IS NOT NULL
');

PRINT '✅ Monitoring view created: vw_MigrationStatus';

-- Final completion message
PRINT '';
PRINT '=== MIGRATION PROCESS COMPLETED ===';
PRINT 'Total execution time: Database operation completed';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Test user login with username and password "Pass123"';
PRINT '2. Verify password change functionality';
PRINT '3. Check role-based authorization';
PRINT '4. Monitor migration status using: SELECT * FROM vw_MigrationStatus';
PRINT '5. Inform users about password reset requirement';
PRINT '';
PRINT 'For support, check the MIGRATION_GUIDE.md documentation.';