-- Script 15: Add UserId field to Empeño, Pagos, and Movimientos tables
-- Purpose: Add ASP.NET Identity UserId field to track which user performed operations
-- Date: 2025-08-11
-- Author: Claude AI Assistant

USE [Hotel]
GO

-- Add comment for script execution tracking
PRINT 'Script 15: Adding UserId field to Empeño, Pagos, and Movimientos tables...'
PRINT 'Date: ' + CAST(GETDATE() AS NVARCHAR(50))
GO

-- =====================================================================================
-- Add UserId to Empeño table
-- =====================================================================================
PRINT 'Checking and adding UserId to Empeño table...'

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Empeño') 
    AND name = 'UserId'
)
BEGIN
    PRINT 'Adding UserId column to Empeño table...'
    
    ALTER TABLE [dbo].[Empeño]
    ADD [UserId] NVARCHAR(450) NULL;
    
    -- Add foreign key constraint to AspNetUsers
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUsers')
    BEGIN
        ALTER TABLE [dbo].[Empeño]
        ADD CONSTRAINT [FK_Empeno_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
        ON DELETE SET NULL;
        
        PRINT 'Added foreign key constraint FK_Empeno_AspNetUsers_UserId'
    END
    ELSE
    BEGIN
        PRINT 'Warning: AspNetUsers table not found. Foreign key constraint not created.'
    END
    
    -- Add index for performance
    CREATE NONCLUSTERED INDEX [IX_Empeno_UserId] 
    ON [dbo].[Empeño] ([UserId]);
    
    PRINT 'Successfully added UserId column to Empeño table with index'
END
ELSE
BEGIN
    PRINT 'UserId column already exists in Empeño table - skipping'
END
GO

-- =====================================================================================
-- Add UserId to Pagos table  
-- =====================================================================================
PRINT 'Checking and adding UserId to Pagos table...'

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Pagos') 
    AND name = 'UserId'
)
BEGIN
    PRINT 'Adding UserId column to Pagos table...'
    
    ALTER TABLE [dbo].[Pagos]
    ADD [UserId] NVARCHAR(450) NULL;
    
    -- Add foreign key constraint to AspNetUsers
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUsers')
    BEGIN
        ALTER TABLE [dbo].[Pagos]
        ADD CONSTRAINT [FK_Pagos_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
        ON DELETE SET NULL;
        
        PRINT 'Added foreign key constraint FK_Pagos_AspNetUsers_UserId'
    END
    ELSE
    BEGIN
        PRINT 'Warning: AspNetUsers table not found. Foreign key constraint not created.'
    END
    
    -- Add index for performance
    CREATE NONCLUSTERED INDEX [IX_Pagos_UserId] 
    ON [dbo].[Pagos] ([UserId]);
    
    PRINT 'Successfully added UserId column to Pagos table with index'
END
ELSE
BEGIN
    PRINT 'UserId column already exists in Pagos table - skipping'
END
GO

-- =====================================================================================
-- Add UserId to Movimientos table
-- =====================================================================================
PRINT 'Checking and adding UserId to Movimientos table...'

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Movimientos') 
    AND name = 'UserId'
)
BEGIN
    PRINT 'Adding UserId column to Movimientos table...'
    
    ALTER TABLE [dbo].[Movimientos]
    ADD [UserId] NVARCHAR(450) NULL;
    
    -- Add foreign key constraint to AspNetUsers
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUsers')
    BEGIN
        ALTER TABLE [dbo].[Movimientos]
        ADD CONSTRAINT [FK_Movimientos_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
        ON DELETE SET NULL;
        
        PRINT 'Added foreign key constraint FK_Movimientos_AspNetUsers_UserId'
    END
    ELSE
    BEGIN
        PRINT 'Warning: AspNetUsers table not found. Foreign key constraint not created.'
    END
    
    -- Add index for performance
    CREATE NONCLUSTERED INDEX [IX_Movimientos_UserId] 
    ON [dbo].[Movimientos] ([UserId]);
    
    PRINT 'Successfully added UserId column to Movimientos table with index'
END
ELSE
BEGIN
    PRINT 'UserId column already exists in Movimientos table - skipping'
END
GO

-- =====================================================================================
-- Add comments to describe the new fields
-- =====================================================================================
PRINT 'Adding extended properties (comments) to document the new UserId fields...'

-- Add comments for Empeño.UserId
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Empeño') 
    AND name = 'UserId'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.extended_properties 
        WHERE major_id = OBJECT_ID('Empeño') 
        AND minor_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('Empeño') AND name = 'UserId')
        AND name = 'MS_Description'
    )
    BEGIN
        EXEC sys.sp_addextendedproperty 
            @name = N'MS_Description',
            @value = N'ASP.NET Identity User ID who created/managed this empeño. References AspNetUsers.Id',
            @level0type = N'SCHEMA', @level0name = N'dbo',
            @level1type = N'TABLE', @level1name = N'Empeño',
            @level2type = N'COLUMN', @level2name = N'UserId';
    END
END

-- Add comments for Pagos.UserId
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Pagos') 
    AND name = 'UserId'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.extended_properties 
        WHERE major_id = OBJECT_ID('Pagos') 
        AND minor_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('Pagos') AND name = 'UserId')
        AND name = 'MS_Description'
    )
    BEGIN
        EXEC sys.sp_addextendedproperty 
            @name = N'MS_Description',
            @value = N'ASP.NET Identity User ID who processed this payment. References AspNetUsers.Id',
            @level0type = N'SCHEMA', @level0name = N'dbo',
            @level1type = N'TABLE', @level1name = N'Pagos',
            @level2type = N'COLUMN', @level2name = N'UserId';
    END
END

-- Add comments for Movimientos.UserId
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Movimientos') 
    AND name = 'UserId'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.extended_properties 
        WHERE major_id = OBJECT_ID('Movimientos') 
        AND minor_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('Movimientos') AND name = 'UserId')
        AND name = 'MS_Description'
    )
    BEGIN
        EXEC sys.sp_addextendedproperty 
            @name = N'MS_Description',
            @value = N'ASP.NET Identity User ID who created this movement record. References AspNetUsers.Id',
            @level0type = N'SCHEMA', @level0name = N'dbo',
            @level1type = N'TABLE', @level1name = N'Movimientos',
            @level2type = N'COLUMN', @level2name = N'UserId';
    END
END
GO

-- =====================================================================================
-- Verification queries
-- =====================================================================================
PRINT 'Verification: Checking if all UserId columns were added successfully...'

-- Check Empeño table
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Empeño') 
    AND name = 'UserId'
)
    PRINT '✓ Empeño.UserId column exists'
ELSE
    PRINT '✗ ERROR: Empeño.UserId column missing'

-- Check Pagos table
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Pagos') 
    AND name = 'UserId'
)
    PRINT '✓ Pagos.UserId column exists'
ELSE
    PRINT '✗ ERROR: Pagos.UserId column missing'

-- Check Movimientos table
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Movimientos') 
    AND name = 'UserId'
)
    PRINT '✓ Movimientos.UserId column exists'
ELSE
    PRINT '✗ ERROR: Movimientos.UserId column missing'

-- Check foreign key constraints
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_Empeno_AspNetUsers_UserId'
)
    PRINT '✓ FK_Empeno_AspNetUsers_UserId constraint exists'
ELSE
    PRINT '⚠ FK_Empeno_AspNetUsers_UserId constraint missing (may be expected if AspNetUsers table not found)'

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_Pagos_AspNetUsers_UserId'
)
    PRINT '✓ FK_Pagos_AspNetUsers_UserId constraint exists'
ELSE
    PRINT '⚠ FK_Pagos_AspNetUsers_UserId constraint missing (may be expected if AspNetUsers table not found)'

IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_Movimientos_AspNetUsers_UserId'
)
    PRINT '✓ FK_Movimientos_AspNetUsers_UserId constraint exists'
ELSE
    PRINT '⚠ FK_Movimientos_AspNetUsers_UserId constraint missing (may be expected if AspNetUsers table not found)'

-- Summary
PRINT ''
PRINT 'Script 15 execution completed!'
PRINT 'Summary:'
PRINT '- Added UserId (NVARCHAR(450) NULL) to Empeño, Pagos, and Movimientos tables'
PRINT '- Added foreign key constraints to AspNetUsers table (if exists)'
PRINT '- Added performance indexes on UserId columns'
PRINT '- Added documentation comments to describe the fields'
PRINT '- All operations are idempotent - safe to run multiple times'
PRINT ''
PRINT 'Next steps:'
PRINT '1. Update Entity Framework models to include UserId properties'
PRINT '2. Update services to populate UserId from current user context'
PRINT '3. Consider data migration if existing records need UserId populated'
GO