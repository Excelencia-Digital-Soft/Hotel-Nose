-- =============================================
-- Script: 16-Migrate_Cierre_To_AspNetUsers.sql
-- Description: Migrates Cierre table to use AspNetUsers instead of legacy Usuarios
-- Version: 1.0
-- Date: 2025-01-26
-- Author: Claude AI Assistant
-- =============================================

USE [Hotel]
GO

-- =============================================
-- 1. ADD USERID COLUMN TO CIERRE TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Cierre') AND name = 'UserId')
BEGIN
    ALTER TABLE [dbo].[Cierre] ADD [UserId] nvarchar(450) NULL
    PRINT 'Column UserId added to Cierre table'
END
ELSE
BEGIN
    PRINT 'Column UserId already exists in Cierre table'
END
GO

-- =============================================
-- 2. CREATE FOREIGN KEY CONSTRAINT TO ASPNETUSERS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Cierre_AspNetUsers')
BEGIN
    -- Check if AspNetUsers table exists first
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
    BEGIN
        ALTER TABLE [dbo].[Cierre]
        ADD CONSTRAINT [FK_Cierre_AspNetUsers]
        FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AspNetUsers] ([Id])
        ON DELETE SET NULL
        
        PRINT 'Foreign key constraint FK_Cierre_AspNetUsers added successfully'
    END
    ELSE
    BEGIN
        PRINT 'WARNING: AspNetUsers table does not exist. Foreign key constraint not created.'
    END
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_Cierre_AspNetUsers already exists'
END
GO

-- =============================================
-- 3. CREATE INDEX FOR PERFORMANCE
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Cierre_UserId' 
    AND object_id = OBJECT_ID('Cierre')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Cierre_UserId]
    ON [dbo].[Cierre] ([UserId])
    INCLUDE ([CierreId], [InstitucionID], [FechaHoraCierre], [EstadoCierre])
    
    PRINT 'Index IX_Cierre_UserId created successfully'
END
ELSE
BEGIN
    PRINT 'Index IX_Cierre_UserId already exists'
END
GO

-- =============================================
-- 4. MIGRATE DATA USING LEGACYUSERID FIELD
-- =============================================

-- Update Cierre records to use AspNetUsers.Id based on LegacyUserId mapping
UPDATE c
SET UserId = u.Id
FROM [dbo].[Cierre] c
INNER JOIN [dbo].[AspNetUsers] u ON c.UsuarioId = u.LegacyUserId
WHERE c.UserId IS NULL
AND c.UsuarioId IS NOT NULL
AND u.LegacyUserId IS NOT NULL

DECLARE @MigratedCount INT = @@ROWCOUNT
PRINT 'Migrated ' + CAST(@MigratedCount AS NVARCHAR(10)) + ' Cierre records using LegacyUserId mapping'

-- Report on unmapped records
DECLARE @UnmappedCount INT
SELECT @UnmappedCount = COUNT(*)
FROM [dbo].[Cierre] c
WHERE c.UserId IS NULL 
AND c.UsuarioId IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM [dbo].[AspNetUsers] u 
    WHERE u.LegacyUserId = c.UsuarioId
)

IF @UnmappedCount > 0
BEGIN
    PRINT 'WARNING: ' + CAST(@UnmappedCount AS NVARCHAR(10)) + ' Cierre records could not be mapped to AspNetUsers'
    PRINT 'These records have UsuarioId values that do not exist in AspNetUsers.LegacyUserId'
    
    -- Optional: Set these to NULL or a default admin user
    -- Uncomment the following lines if you want to set unmapped records to NULL
    /*
    UPDATE [dbo].[Cierre] 
    SET UsuarioId = NULL
    WHERE UserId IS NULL 
    AND UsuarioId IS NOT NULL
    AND NOT EXISTS (
        SELECT 1 FROM [dbo].[AspNetUsers] u 
        WHERE u.LegacyUserId = UsuarioId
    )
    PRINT 'Set unmapped UsuarioId values to NULL'
    */
END
ELSE
BEGIN
    PRINT 'All Cierre records with UsuarioId have been successfully mapped to AspNetUsers'
END

-- =============================================
-- 5. ADD AUDIT FIELDS FOR FUTURE USE
-- =============================================

-- Add creation and modification audit fields if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Cierre') AND name = 'CreadoPorId')
BEGIN
    ALTER TABLE [dbo].[Cierre] ADD [CreadoPorId] nvarchar(450) NULL
    PRINT 'Column CreadoPorId added to Cierre table'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Cierre') AND name = 'FechaCreacion')
BEGIN
    ALTER TABLE [dbo].[Cierre] ADD [FechaCreacion] datetime2(7) NULL
    PRINT 'Column FechaCreacion added to Cierre table'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Cierre') AND name = 'ModificadoPorId')
BEGIN
    ALTER TABLE [dbo].[Cierre] ADD [ModificadoPorId] nvarchar(450) NULL
    PRINT 'Column ModificadoPorId added to Cierre table'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Cierre') AND name = 'FechaModificacion')
BEGIN
    ALTER TABLE [dbo].[Cierre] ADD [FechaModificacion] datetime2(7) NULL
    PRINT 'Column FechaModificacion added to Cierre table'
END
GO

-- Add foreign key constraints for audit fields
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Cierre_AspNetUsers_CreadoPor')
   AND EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
BEGIN
    ALTER TABLE [dbo].[Cierre]
    ADD CONSTRAINT [FK_Cierre_AspNetUsers_CreadoPor]
    FOREIGN KEY ([CreadoPorId]) 
    REFERENCES [dbo].[AspNetUsers] ([Id])
    
    PRINT 'Foreign key constraint FK_Cierre_AspNetUsers_CreadoPor added'
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Cierre_AspNetUsers_ModificadoPor')
   AND EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
BEGIN
    ALTER TABLE [dbo].[Cierre]
    ADD CONSTRAINT [FK_Cierre_AspNetUsers_ModificadoPor]
    FOREIGN KEY ([ModificadoPorId]) 
    REFERENCES [dbo].[AspNetUsers] ([Id])
    
    PRINT 'Foreign key constraint FK_Cierre_AspNetUsers_ModificadoPor added'
END
GO

-- =============================================
-- 6. UPDATE EXISTING RECORDS WITH CREATION DATE
-- =============================================

-- Set FechaCreacion to FechaHoraCierre for existing records that don't have it
UPDATE [dbo].[Cierre]
SET FechaCreacion = ISNULL(FechaHoraCierre, GETDATE())
WHERE FechaCreacion IS NULL

PRINT 'Updated FechaCreacion for existing Cierre records'

-- =============================================
-- COMPLETION MESSAGE
-- =============================================
PRINT '=========================================='
PRINT 'Cierre Migration to AspNetUsers Complete!'
PRINT '=========================================='
PRINT 'Changes made:'
PRINT '- Added UserId column (nvarchar(450))'
PRINT '- Added foreign key to AspNetUsers'
PRINT '- Added performance index IX_Cierre_UserId'
PRINT '- Added audit fields: CreadoPorId, FechaCreacion, ModificadoPorId, FechaModificacion'
PRINT '- Updated existing records with creation dates'
PRINT '- Migrated existing Cierre records using AspNetUsers.LegacyUserId mapping'
PRINT ''
PRINT 'Data migration uses AspNetUsers.LegacyUserId field to map'
PRINT 'legacy Usuarios.UsuarioId to AspNetUsers.Id automatically.'
PRINT '=========================================='