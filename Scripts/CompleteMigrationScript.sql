-- Script completo de migración para Identity y campos faltantes
-- Incluye tablas AspNet Identity y campos nuevos en Imagenes/HabitacionImagenes

BEGIN TRANSACTION;

-- Crear tabla __EFMigrationsHistory si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__EFMigrationsHistory' AND xtype='U')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT '__EFMigrationsHistory table created';
END

-- ===== TABLAS DE IDENTITY =====

-- Crear tabla AspNetRoles si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoles' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Description] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_AspNetRoles_NormalizedName] ON [AspNetRoles] ([NormalizedName]);
    PRINT 'AspNetRoles table created';
END

-- Crear tabla AspNetUsers si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUsers' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NULL,
        [LastName] nvarchar(max) NULL,
        [InstitucionId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [ForcePasswordChange] bit NOT NULL,
        [LegacyUserId] int NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_AspNetUsers_NormalizedEmail] ON [AspNetUsers] ([NormalizedEmail]);
    CREATE UNIQUE INDEX [IX_AspNetUsers_NormalizedUserName] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
    PRINT 'AspNetUsers table created';
END

-- Crear tabla AspNetUserRoles si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserRoles' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
    PRINT 'AspNetUserRoles table created';
END

-- Crear tabla AspNetUserClaims si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserClaims' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
    PRINT 'AspNetUserClaims table created';
END

-- Crear tabla AspNetUserLogins si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserLogins' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
    PRINT 'AspNetUserLogins table created';
END

-- Crear tabla AspNetUserTokens si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserTokens' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    PRINT 'AspNetUserTokens table created';
END

-- Crear tabla AspNetRoleClaims si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoleClaims' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
    PRINT 'AspNetRoleClaims table created';
END

-- ===== CAMPOS FALTANTES EN TABLAS EXISTENTES =====

-- Agregar campos faltantes en tabla Imagenes
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Imagenes' AND COLUMN_NAME = 'FechaSubida')
BEGIN
    ALTER TABLE [Imagenes] ADD [FechaSubida] datetime2 NOT NULL DEFAULT GETDATE();
    PRINT 'FechaSubida column added to Imagenes table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Imagenes' AND COLUMN_NAME = 'InstitucionID')
BEGIN
    ALTER TABLE [Imagenes] ADD [InstitucionID] int NOT NULL DEFAULT 1;
    PRINT 'InstitucionID column added to Imagenes table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Imagenes' AND COLUMN_NAME = 'Origen')
BEGIN
    ALTER TABLE [Imagenes] ADD [Origen] nvarchar(500) NOT NULL DEFAULT N'';
    PRINT 'Origen column added to Imagenes table';
END

-- Agregar campos faltantes en tabla HabitacionImagenes
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HabitacionImagenes' AND COLUMN_NAME = 'EsPrincipal')
BEGIN
    ALTER TABLE [HabitacionImagenes] ADD [EsPrincipal] bit NOT NULL DEFAULT 0;
    PRINT 'EsPrincipal column added to HabitacionImagenes table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HabitacionImagenes' AND COLUMN_NAME = 'Orden')
BEGIN
    ALTER TABLE [HabitacionImagenes] ADD [Orden] int NOT NULL DEFAULT 0;
    PRINT 'Orden column added to HabitacionImagenes table';
END

-- ===== ACTUALIZAR HISTORIAL DE MIGRACIONES =====

-- Marcar InitialCreate migration como aplicada
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250701174952_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250701174952_InitialCreate', N'7.0.0');
    PRINT 'InitialCreate migration marked as applied';
END

-- Marcar AddMissingImageFields migration como aplicada
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250701182811_AddMissingImageFields')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250701182811_AddMissingImageFields', N'7.0.0');
    PRINT 'AddMissingImageFields migration marked as applied';
END

-- ===== MIGRACIÓN DE DATOS LEGACY =====

-- Migrar roles desde tabla Roles
PRINT 'Migrando roles desde tabla legacy...';
DECLARE @RoleMappings TABLE (LegacyRoleName NVARCHAR(256), IdentityRoleName NVARCHAR(256), RoleDescription NVARCHAR(MAX))
INSERT INTO @RoleMappings VALUES 
    ('DIRECTOR', 'Director', 'Hotel Director with full management access'),
    ('ADMINISTRADOR', 'Administrator', 'System Administrator with full access'),
    ('ADMIN', 'Administrator', 'System Administrator with full access'),
    ('EXCELENCIAADMIN', 'Administrator', 'System Administrator with full access'),
    ('ADMINISTRADOR DEL SISTEMA', 'Administrator', 'System Administrator with full access'),
    ('MUCAMA', 'Mucama', 'Housekeeping staff member'),
    ('CAJERO', 'Cajero', 'Cashier with financial transaction access'),
    ('CAJERO STOCK', 'Cajero Stock', 'Stock cashier with inventory access'),
    ('USUARIO', 'User', 'Basic user with minimal access'),
    ('USER', 'User', 'Basic user with minimal access')

-- Solo migrar si existe la tabla Roles legacy
IF EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    -- Insertar roles en AspNetRoles
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, Description, CreatedAt, IsActive, ConcurrencyStamp)
    SELECT DISTINCT
        NEWID() as Id,
        rm.IdentityRoleName as Name,
        UPPER(rm.IdentityRoleName) as NormalizedName,
        rm.RoleDescription as Description,
        GETDATE() as CreatedAt,
        1 as IsActive,
        NEWID() as ConcurrencyStamp
    FROM Roles r
    INNER JOIN @RoleMappings rm ON UPPER(r.NombreRol) = UPPER(rm.LegacyRoleName)
    WHERE r.NombreRol IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM AspNetRoles ar WHERE ar.Name = rm.IdentityRoleName);
    
    PRINT 'Roles migrados correctamente';
END

-- Migrar usuarios desde tabla Usuarios
PRINT 'Migrando usuarios desde tabla legacy...';
IF EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
BEGIN
    -- Migrar usuarios a AspNetUsers
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
        ForcePasswordChange
    )
    SELECT 
        NEWID() as Id,
        u.NombreUsuario as UserName,
        UPPER(u.NombreUsuario) as NormalizedUserName,
        CASE 
            WHEN u.NombreUsuario LIKE '%@%' THEN u.NombreUsuario
            ELSE u.NombreUsuario + '@hotel.fake'
        END as Email,
        CASE 
            WHEN u.NombreUsuario LIKE '%@%' THEN UPPER(u.NombreUsuario)
            ELSE UPPER(u.NombreUsuario + '@hotel.fake')
        END as NormalizedEmail,
        1 as EmailConfirmed,
        'AQAAAAEAACcQAAAAEKcO/+btL3p8+DxXFz7CjAqF/T5gK3QMF7pO1TLQ8sHx/R7nN4vF2Q1Y9gH3K8Wm' as PasswordHash,
        CONVERT(NVARCHAR(36), NEWID()) as SecurityStamp,
        CONVERT(NVARCHAR(36), NEWID()) as ConcurrencyStamp,
        NULL as PhoneNumber,
        0 as PhoneNumberConfirmed,
        0 as TwoFactorEnabled,
        1 as LockoutEnabled,
        0 as AccessFailedCount,
        u.UsuarioId as LegacyUserId,
        (SELECT TOP 1 ui.InstitucionID 
         FROM UsuariosInstituciones ui 
         WHERE ui.UsuarioId = u.UsuarioId) as InstitucionId,
        GETDATE() as CreatedAt,
        1 as IsActive,
        1 as ForcePasswordChange
    FROM Usuarios u
    WHERE NOT EXISTS (
        SELECT 1 FROM AspNetUsers au WHERE au.LegacyUserId = u.UsuarioId
    );
    
    -- Asignar roles a usuarios
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    SELECT DISTINCT
        au.Id as UserId,
        ar.Id as RoleId
    FROM AspNetUsers au
    INNER JOIN Usuarios u ON au.LegacyUserId = u.UsuarioId
    INNER JOIN Roles r ON u.RolId = r.RolId
    INNER JOIN @RoleMappings rm ON UPPER(r.NombreRol) = UPPER(rm.LegacyRoleName)
    INNER JOIN AspNetRoles ar ON ar.Name = rm.IdentityRoleName
    WHERE NOT EXISTS (
        SELECT 1 FROM AspNetUserRoles aur 
        WHERE aur.UserId = au.Id AND aur.RoleId = ar.Id
    );
    
    PRINT 'Usuarios migrados correctamente';
END

COMMIT TRANSACTION;

-- ===== VERIFICACIÓN FINAL =====
PRINT '=== VERIFICACIÓN DE MIGRACIÓN ===';
PRINT 'Tablas Identity creadas:';
SELECT 'AspNetRoles' as TablaIdentity WHERE EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoles' AND xtype='U')
UNION ALL
SELECT 'AspNetUsers' WHERE EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUsers' AND xtype='U')
UNION ALL
SELECT 'AspNetUserRoles' WHERE EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserRoles' AND xtype='U');

PRINT 'Campos agregados en Imagenes:';
SELECT COLUMN_NAME as CampoImagenes FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Imagenes' AND COLUMN_NAME IN ('FechaSubida', 'InstitucionID', 'Origen');

PRINT 'Campos agregados en HabitacionImagenes:';
SELECT COLUMN_NAME as CampoHabitacionImagenes FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'HabitacionImagenes' AND COLUMN_NAME IN ('EsPrincipal', 'Orden');

PRINT 'Migraciones registradas:';
SELECT [MigrationId] FROM [__EFMigrationsHistory] ORDER BY [MigrationId];

-- Estadísticas de migración de usuarios
IF EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUsers' AND xtype='U')
BEGIN
    DECLARE @UserCount INT = (SELECT COUNT(*) FROM AspNetUsers WHERE LegacyUserId IS NOT NULL);
    DECLARE @RoleCount INT = (SELECT COUNT(*) FROM AspNetRoles);
    DECLARE @UserRoleCount INT = (SELECT COUNT(*) FROM AspNetUserRoles);
    
    PRINT 'Estadísticas de migración:';
    PRINT '- Usuarios migrados: ' + CAST(@UserCount AS NVARCHAR(10));
    PRINT '- Roles disponibles: ' + CAST(@RoleCount AS NVARCHAR(10));
    PRINT '- Asignaciones usuario-rol: ' + CAST(@UserRoleCount AS NVARCHAR(10));
END

PRINT 'Migración completa finalizada exitosamente';