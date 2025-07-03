-- Safe migration script for adding missing image fields
-- This script checks if tables/columns exist before attempting to create them

BEGIN TRANSACTION;

-- Check if __EFMigrationsHistory table exists
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__EFMigrationsHistory' AND xtype='U')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT '__EFMigrationsHistory table created';
END

-- Mark InitialCreate migration as applied (since tables already exist)
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250701174952_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250701174952_InitialCreate', N'7.0.0');
    PRINT 'InitialCreate migration marked as applied';
END

-- Check if Origen column exists in Imagenes table before adding it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Imagenes' AND COLUMN_NAME = 'Origen')
BEGIN
    ALTER TABLE [Imagenes] ADD [Origen] nvarchar(500) NOT NULL DEFAULT N'';
    PRINT 'Origen column added to Imagenes table';
END
ELSE
BEGIN
    PRINT 'Origen column already exists in Imagenes table';
END

-- Mark AddMissingImageFields migration as applied
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250701182811_AddMissingImageFields')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250701182811_AddMissingImageFields', N'7.0.0');
    PRINT 'AddMissingImageFields migration marked as applied';
END

COMMIT TRANSACTION;

PRINT 'Migration completed successfully';