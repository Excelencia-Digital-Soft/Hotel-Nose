-- =============================================
-- Script: 11-Fix_Registros_ReservaID_Default.sql
-- Description: Fix ReservaID default value in Registros table
-- Author: Claude Code Assistant  
-- Date: 2025-01-11
-- Issue: ReservaID defaults to 0 but no reservation with ID=0 exists, causing FK constraint violations
-- =============================================

USE [Hotel]
GO

PRINT '======================================================'
PRINT 'FIXING REGISTROS RESERVAID DEFAULT VALUE'
PRINT '======================================================'

-- Check if the problematic default constraint exists
IF EXISTS (SELECT * FROM sys.default_constraints 
           WHERE parent_object_id = OBJECT_ID('dbo.Registros') 
           AND parent_column_id = (SELECT column_id FROM sys.columns 
                                   WHERE object_id = OBJECT_ID('dbo.Registros') 
                                   AND name = 'ReservaID'))
BEGIN
    DECLARE @constraint_name VARCHAR(256)
    SELECT @constraint_name = name 
    FROM sys.default_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.Registros') 
    AND parent_column_id = (SELECT column_id FROM sys.columns 
                           WHERE object_id = OBJECT_ID('dbo.Registros') 
                           AND name = 'ReservaID')

    PRINT 'Found default constraint: ' + @constraint_name
    
    -- Drop the existing default constraint
    EXEC('ALTER TABLE [dbo].[Registros] DROP CONSTRAINT [' + @constraint_name + ']')
    PRINT 'Dropped existing default constraint for ReservaID'
END
ELSE
BEGIN
    PRINT 'No existing default constraint found for ReservaID'
END

-- Update any existing records that have ReservaID = 0 to NULL
-- (only if they don't reference a valid reservation)
IF EXISTS (SELECT 1 FROM [dbo].[Registros] WHERE [ReservaID] = 0)
BEGIN
    DECLARE @records_to_update INT
    SELECT @records_to_update = COUNT(*) 
    FROM [dbo].[Registros] r 
    WHERE r.[ReservaID] = 0 
    AND NOT EXISTS (SELECT 1 FROM [dbo].[Reservas] res WHERE res.[ReservaID] = 0)
    
    PRINT 'Found ' + CAST(@records_to_update AS VARCHAR(10)) + ' records with ReservaID = 0 that need to be updated to NULL'
    
    IF @records_to_update > 0
    BEGIN
        UPDATE [dbo].[Registros] 
        SET [ReservaID] = NULL 
        WHERE [ReservaID] = 0 
        AND NOT EXISTS (SELECT 1 FROM [dbo].[Reservas] res WHERE res.[ReservaID] = 0)
        
        PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' records from ReservaID = 0 to NULL'
    END
END
ELSE
BEGIN
    PRINT 'No records found with ReservaID = 0'
END

-- Ensure ReservaID column allows NULL (it should already, but let's be sure)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Registros' 
           AND COLUMN_NAME = 'ReservaID' 
           AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE [dbo].[Registros] ALTER COLUMN [ReservaID] INT NULL
    PRINT 'Modified ReservaID column to allow NULL'
END
ELSE
BEGIN
    PRINT 'ReservaID column already allows NULL'
END

-- Add a new default constraint that sets ReservaID to NULL (which is the natural default for nullable columns)
-- Note: We don't actually need an explicit default constraint for NULL, 
-- but we're adding this for clarity and to ensure consistency
ALTER TABLE [dbo].[Registros] ADD CONSTRAINT [DF_Registros_ReservaID_NULL] DEFAULT (NULL) FOR [ReservaID]
PRINT 'Added new default constraint: ReservaID defaults to NULL'

-- Verify the fix
PRINT ''
PRINT 'Verification:'
SELECT 
    'Default Constraint' as Check_Type,
    CASE WHEN EXISTS (
        SELECT * FROM sys.default_constraints dc
        INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
        WHERE c.object_id = OBJECT_ID('dbo.Registros')
        AND c.name = 'ReservaID'
        AND dc.definition = '(NULL)'
    ) THEN 'PASS - ReservaID defaults to NULL' 
    ELSE 'FAIL - ReservaID does not default to NULL' 
    END as Status

UNION ALL

SELECT 
    'Column Nullable' as Check_Type,
    CASE WHEN EXISTS (
        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'Registros' 
        AND COLUMN_NAME = 'ReservaID' 
        AND IS_NULLABLE = 'YES'
    ) THEN 'PASS - ReservaID allows NULL'
    ELSE 'FAIL - ReservaID does not allow NULL'
    END as Status

UNION ALL

SELECT 
    'Orphaned Records' as Check_Type,
    CASE WHEN NOT EXISTS (
        SELECT 1 FROM [dbo].[Registros] r 
        WHERE r.[ReservaID] IS NOT NULL 
        AND r.[ReservaID] NOT IN (SELECT [ReservaID] FROM [dbo].[Reservas])
    ) THEN 'PASS - No orphaned ReservaID references'
    ELSE 'WARNING - Found orphaned ReservaID references'
    END as Status

PRINT ''
PRINT 'Fix completed successfully!'
PRINT 'ReservaID in Registros table now properly defaults to NULL instead of 0'
PRINT 'This should resolve the FK constraint violation issues.'

GO