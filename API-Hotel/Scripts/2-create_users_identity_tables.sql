-- Create Identity tables manually
-- These tables are needed for ASP.NET Core Identity

-- Create AspNetRoles table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoles' AND xtype='U')
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

-- Create AspNetUsers table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUsers' AND xtype='U')
CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [InstitucionId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastLoginAt] datetime2 NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [ForcePasswordChange] bit NOT NULL DEFAULT 0,
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

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'InstitucionId')
BEGIN
    ALTER TABLE AspNetUsers
    ADD InstitucionId INT NULL;
END

-- 2. Crear la foreign key constraint hacia la tabla Instituciones
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
               WHERE CONSTRAINT_NAME = 'FK_AspNetUsers_Instituciones_InstitucionId')
BEGIN
    ALTER TABLE AspNetUsers
    ADD CONSTRAINT FK_AspNetUsers_Instituciones_InstitucionId
    FOREIGN KEY (InstitucionId) REFERENCES Instituciones(InstitucionId);
END

-- 3. Crear índice para mejorar rendimiento en consultas por institución
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_AspNetUsers_InstitucionId' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE INDEX IX_AspNetUsers_InstitucionId 
    ON AspNetUsers (InstitucionId);
END

-- Create AspNetUserRoles table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserRoles' AND xtype='U')
CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

-- Create AspNetUserClaims table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserClaims' AND xtype='U')
CREATE TABLE [AspNetUserClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

-- Create AspNetUserLogins table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserLogins' AND xtype='U')
CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

-- Create AspNetUserTokens table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserTokens' AND xtype='U')
CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

-- Create AspNetRoleClaims table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoleClaims' AND xtype='U')
CREATE TABLE [AspNetRoleClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

-- Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetRoleClaims_RoleId')
CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'RoleNameIndex')
CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserClaims_UserId')
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserLogins_UserId')
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserRoles_RoleId')
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'EmailIndex')
CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UserNameIndex')
CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

-- Add foreign key for InstitucionId if Institucion table exists
IF EXISTS (SELECT * FROM sysobjects WHERE name='Institucion' AND xtype='U')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUsers_Institucion_InstitucionId')
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Institucion_InstitucionId] 
    FOREIGN KEY ([InstitucionId]) REFERENCES [Institucion] ([InstitucionId]);
END

-- =====================================================
-- MIGRATION LOGIC: Sync existing users to Identity tables
-- =====================================================
-- IMPORTANT: All migrated users will have the default password "Pass123"
-- Users should be forced to change their password on first login
-- =====================================================

PRINT 'Starting user migration from legacy tables...'

-- First, create/update roles in AspNetRoles based on existing Roles table
PRINT 'Migrating roles...'

-- Function to map legacy role names to Identity role names
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

-- Insert/Update AspNetRoles based on legacy Roles table
MERGE AspNetRoles AS target
USING (
    SELECT DISTINCT
        rm.IdentityRoleName,
        rm.RoleDescription,
        GETDATE() as CreatedAt
    FROM Roles r
    INNER JOIN @RoleMappings rm ON UPPER(r.NombreRol) = UPPER(rm.LegacyRoleName)
    WHERE r.NombreRol IS NOT NULL
) AS source (RoleName, Description, CreatedAt)
ON target.Name = source.RoleName
WHEN NOT MATCHED THEN
    INSERT (Id, Name, NormalizedName, Description, CreatedAt, IsActive, ConcurrencyStamp)
    VALUES (NEWID(), source.RoleName, UPPER(source.RoleName), source.Description, source.CreatedAt, 1, NEWID())
WHEN MATCHED THEN
    UPDATE SET 
        Description = source.Description,
        IsActive = 1;

PRINT 'Roles migration completed.'

-- Now migrate users from Usuarios table to AspNetUsers
PRINT 'Migrating users...'

-- Add LegacyUserId column to AspNetUsers if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'LegacyUserId')
BEGIN
    ALTER TABLE AspNetUsers ADD LegacyUserId INT NULL;
    PRINT 'Added LegacyUserId column to AspNetUsers table.'
END

-- Migrate users from Usuarios to AspNetUsers
INSERT INTO AspNetUsers (
    Id,
    FirstName,
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
    RTRIM(u.NombreUsuario) as FirstName,
    CASE 
        WHEN RTRIM(u.NombreUsuario) LIKE '%@%' THEN RTRIM(u.NombreUsuario)
        ELSE RTRIM(u.NombreUsuario) + IIF(ui.InstitucionID = 1, '@hotel.nose', '@hotel.taos')
    END as UserName,
    CASE 
        WHEN u.NombreUsuario LIKE '%@%' THEN UPPER(RTRIM(u.NombreUsuario))
        ELSE UPPER(RTRIM(u.NombreUsuario) + IIF(ui.InstitucionID = 1, '@hotel.nose', '@hotel.taos'))
    END as NormalizedUserName,
    CASE 
        WHEN RTRIM(u.NombreUsuario) LIKE '%@%' THEN RTRIM(u.NombreUsuario)
        ELSE RTRIM(u.NombreUsuario) + IIF(ui.InstitucionID = 1, '@hotel.nose', '@hotel.taos')
    END as Email,
    CASE 
        WHEN u.NombreUsuario LIKE '%@%' THEN UPPER(RTRIM(u.NombreUsuario))
        ELSE UPPER(RTRIM(u.NombreUsuario) + IIF(ui.InstitucionID = 1, '@hotel.nose', '@hotel.taos'))
    END as NormalizedEmail,
    1 as EmailConfirmed, -- Assume legacy emails are confirmed
    'AQAAAAIAAYagAAAAEJ1KxN1CVU1AEahcUIrel+vlTVTQtPdyenkBqqrO8zwYjMp7xN4EIuDky+mFMQKQug==' as PasswordHash, -- Default password "Pass123" using Identity hasher
    CONVERT(NVARCHAR(36), NEWID()) as SecurityStamp,
    CONVERT(NVARCHAR(36), NEWID()) as ConcurrencyStamp,
    NULL as PhoneNumber,
    0 as PhoneNumberConfirmed,
    0 as TwoFactorEnabled,
    1 as LockoutEnabled,
    0 as AccessFailedCount,
    u.UsuarioId as LegacyUserId,
    -- Get InstitucionId from UsuariosInstituciones (take first one if multiple)
    ui.InstitucionID as InstitucionId,
    GETDATE() as CreatedAt,
    1 as IsActive,
    1 as ForcePasswordChange -- Force password change on first login since all users get default password
FROM Usuarios u
INNER JOIN UsuariosInstituciones ui ON u.UsuarioId = ui.UsuarioId
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUsers au WHERE au.LegacyUserId = u.UsuarioId
);

PRINT 'Users migration completed.'

-- Now assign roles to users in AspNetUserRoles
PRINT 'Migrating user roles...'

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

PRINT 'User roles migration completed.'

-- Create procedure to keep AspNetUsers and Usuarios tables synchronized
PRINT 'Creating synchronization procedures...'

-- Procedure to sync user updates from Usuarios to AspNetUsers
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SyncUsuarioToAspNetUser')
    DROP PROCEDURE SyncUsuarioToAspNetUser;
GO

CREATE PROCEDURE SyncUsuarioToAspNetUser
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Define role mappings inside the procedure
    DECLARE @RoleMappings TABLE (LegacyRoleName NVARCHAR(256), IdentityRoleName NVARCHAR(256))
    INSERT INTO @RoleMappings VALUES 
        ('DIRECTOR', 'Director'),
        ('ADMINISTRADOR', 'Administrator'),
        ('ADMIN', 'Administrator'),
        ('EXCELENCIAADMIN', 'Administrator'),
        ('ADMINISTRADOR DEL SISTEMA', 'Administrator'),
        ('MUCAMA', 'Mucama'),
        ('CAJERO', 'Cajero'),
        ('CAJERO STOCK', 'Cajero Stock'),
        ('USUARIO', 'User'),
        ('USER', 'User')
    
    DECLARE @UserId NVARCHAR(450);
    SELECT @UserId = Id FROM AspNetUsers WHERE LegacyUserId = @UsuarioId;
    
    IF @UserId IS NOT NULL
    BEGIN
        -- Update AspNetUsers with changes from Usuarios
        UPDATE au SET
            FirstName = RTRIM(u.NombreUsuario),
            UserName = CASE 
                WHEN RTRIM(u.NombreUsuario) LIKE '%@%' THEN RTRIM(u.NombreUsuario)
                ELSE RTRIM(u.NombreUsuario) + IIF((SELECT TOP 1 InstitucionID FROM UsuariosInstituciones WHERE UsuarioId = u.UsuarioId) = 1, '@hotel.nose', '@hotel.taos')
            END,
            NormalizedUserName = CASE 
                WHEN u.NombreUsuario LIKE '%@%' THEN UPPER(RTRIM(u.NombreUsuario))
                ELSE UPPER(RTRIM(u.NombreUsuario) + IIF((SELECT TOP 1 InstitucionID FROM UsuariosInstituciones WHERE UsuarioId = u.UsuarioId) = 1, '@hotel.nose', '@hotel.taos'))
            END,
            Email = CASE 
                WHEN RTRIM(u.NombreUsuario) LIKE '%@%' THEN RTRIM(u.NombreUsuario)
                ELSE RTRIM(u.NombreUsuario) + IIF((SELECT TOP 1 InstitucionID FROM UsuariosInstituciones WHERE UsuarioId = u.UsuarioId) = 1, '@hotel.nose', '@hotel.taos')
            END,
            NormalizedEmail = CASE 
                WHEN u.NombreUsuario LIKE '%@%' THEN UPPER(RTRIM(u.NombreUsuario))
                ELSE UPPER(RTRIM(u.NombreUsuario) + IIF((SELECT TOP 1 InstitucionID FROM UsuariosInstituciones WHERE UsuarioId = u.UsuarioId) = 1, '@hotel.nose', '@hotel.taos'))
            END,
            PasswordHash = 'AQAAAAIAAYagAAAAEJ1KxN1CVU1AEahcUIrel+vlTVTQtPdyenkBqqrO8zwYjMp7xN4EIuDky+mFMQKQug==', -- Default password "Pass123" using Identity hashe
            SecurityStamp = CONVERT(NVARCHAR(36), NEWID()) -- Update security stamp
        FROM AspNetUsers au
        INNER JOIN Usuarios u ON au.LegacyUserId = u.UsuarioId
        WHERE u.UsuarioId = @UsuarioId;
        
        -- Update user roles if role changed
        DELETE FROM AspNetUserRoles WHERE UserId = @UserId;
        
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        SELECT 
            @UserId,
            ar.Id
        FROM Usuarios u
        INNER JOIN Roles r ON u.RolId = r.RolId
        INNER JOIN @RoleMappings rm ON UPPER(r.NombreRol) = UPPER(rm.LegacyRoleName)
        INNER JOIN AspNetRoles ar ON ar.Name = rm.IdentityRoleName
        WHERE u.UsuarioId = @UsuarioId;
        
        -- Update institution relationship
        UPDATE au SET
            InstitucionId = (SELECT TOP 1 ui.InstitucionID 
                           FROM UsuariosInstituciones ui 
                           WHERE ui.UsuarioId = @UsuarioId)
        FROM AspNetUsers au
        WHERE au.LegacyUserId = @UsuarioId;
    END
END;
GO

PRINT 'Synchronization procedures created.'

-- Summary of migration
DECLARE @UserCount INT, @RoleCount INT, @UserRoleCount INT;
SELECT @UserCount = COUNT(*) FROM AspNetUsers WHERE LegacyUserId IS NOT NULL;
SELECT @RoleCount = COUNT(*) FROM AspNetRoles;
SELECT @UserRoleCount = COUNT(*) FROM AspNetUserRoles 
    WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE LegacyUserId IS NOT NULL);

PRINT 'Migration Summary:'
PRINT '- Users migrated: ' + CAST(@UserCount AS NVARCHAR(10))
PRINT '- Roles available: ' + CAST(@RoleCount AS NVARCHAR(10))
PRINT '- User-role assignments: ' + CAST(@UserRoleCount AS NVARCHAR(10))

PRINT 'Identity tables created and legacy data migrated successfully!'
