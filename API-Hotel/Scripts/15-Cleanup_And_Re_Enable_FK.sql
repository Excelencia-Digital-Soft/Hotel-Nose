-- =============================================
-- Script: 15-Cleanup_And_Re_Enable_FK.sql
-- Description: Clean up orphaned Registros and re-enable FK constraint
-- Author: Claude Code Assistant  
-- Date: 2025-01-11
-- =============================================

USE [Hotel]
GO

PRINT '======================================================'
PRINT 'CLEANING UP ORPHANED REGISTROS AND RE-ENABLING FK'
PRINT '======================================================'

-- Step 1: Identify orphaned records
PRINT 'Step 1: Identifying orphaned Registros...'
SELECT 
    r.RegistroID,
    r.ReservaID,
    r.Contenido,
    r.FechaRegistro
FROM [dbo].[Registros] r 
WHERE r.[ReservaID] IS NOT NULL 
AND r.[ReservaID] NOT IN (SELECT [ReservaID] FROM [dbo].[Reservas])
ORDER BY r.FechaRegistro DESC

-- Step 2: Show details about what we're about to fix
DECLARE @OrphanCount INT
SELECT @OrphanCount = COUNT(*) 
FROM [dbo].[Registros] r 
WHERE r.[ReservaID] IS NOT NULL 
AND r.[ReservaID] NOT IN (SELECT [ReservaID] FROM [dbo].[Reservas])

PRINT ''
PRINT 'Found ' + CAST(@OrphanCount AS VARCHAR(10)) + ' orphaned Registros records'

IF @OrphanCount > 0
BEGIN
    PRINT ''
    PRINT 'Step 2: Fixing orphaned records by setting ReservaID to NULL...'
    
    -- Update orphaned records to have NULL ReservaID instead of invalid references
    UPDATE r
    SET ReservaID = NULL,
        Contenido = Contenido + ' [ReservaID reference fixed - was: ' + CAST(r.ReservaID AS VARCHAR(10)) + ']'
    FROM [dbo].[Registros] r 
    WHERE r.[ReservaID] IS NOT NULL 
    AND r.[ReservaID] NOT IN (SELECT [ReservaID] FROM [dbo].[Reservas])
    
    PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' orphaned records'
    PRINT 'ReservaID set to NULL and added note to Contenido field'
END
ELSE
BEGIN
    PRINT 'No orphaned records found to clean up'
END

-- Step 3: Verify no orphaned records remain
PRINT ''
PRINT 'Step 3: Verifying cleanup...'
SELECT 
    'Remaining Orphaned Records' as Status,
    COUNT(*) as Count
FROM [dbo].[Registros] r 
WHERE r.[ReservaID] IS NOT NULL 
AND r.[ReservaID] NOT IN (SELECT [ReservaID] FROM [dbo].[Reservas])

-- Step 4: Re-enable the foreign key constraint
PRINT ''
PRINT 'Step 4: Re-enabling FK constraint...'
ALTER TABLE [dbo].[Registros] WITH CHECK CHECK CONSTRAINT [FK_Registros_Reservas]
PRINT 'FK_Registros_Reservas constraint has been RE-ENABLED with data validation'

-- Step 5: Verify constraint status
PRINT ''
PRINT 'Step 5: Verifying constraint status...'
SELECT 
    'FK Constraint Status' as Info,
    CASE WHEN is_disabled = 1 THEN 'DISABLED' ELSE 'ENABLED' END as Status,
    name as ConstraintName
FROM sys.foreign_keys 
WHERE name = 'FK_Registros_Reservas'

PRINT ''
PRINT '======================================================'
PRINT 'CLEANUP COMPLETED SUCCESSFULLY!'
PRINT 'FK constraint is now properly enabled'
PRINT 'Orphaned records have been preserved with NULL ReservaID'
PRINT '======================================================'

GO