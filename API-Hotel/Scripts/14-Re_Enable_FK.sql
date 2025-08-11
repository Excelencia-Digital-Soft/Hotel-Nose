-- =============================================
-- Script: 14-Re_Enable_FK.sql
-- Description: Re-enable FK constraint after testing
-- Author: Claude Code Assistant  
-- Date: 2025-01-11
-- =============================================

USE [Hotel]
GO

PRINT '======================================================'
PRINT 'RE-ENABLING FK_Registros_Reservas CONSTRAINT'
PRINT '======================================================'

-- First check for any data integrity issues that might prevent re-enabling
PRINT 'Checking for data integrity issues...'
SELECT 
    'Orphaned Records Check' as Issue,
    COUNT(*) as Count
FROM [dbo].[Registros] r 
WHERE r.[ReservaID] IS NOT NULL 
AND r.[ReservaID] NOT IN (SELECT [ReservaID] FROM [dbo].[Reservas])

-- Re-enable the foreign key constraint with CHECK (validates existing data)
ALTER TABLE [dbo].[Registros] WITH CHECK CHECK CONSTRAINT [FK_Registros_Reservas]
PRINT 'FK_Registros_Reservas constraint has been RE-ENABLED with data validation'

-- Verify constraint status
SELECT 
    'FK Constraint Status' as Info,
    CASE WHEN is_disabled = 1 THEN 'DISABLED' ELSE 'ENABLED' END as Status,
    name as ConstraintName
FROM sys.foreign_keys 
WHERE name = 'FK_Registros_Reservas'

PRINT 'Foreign key constraint is now active again'

GO