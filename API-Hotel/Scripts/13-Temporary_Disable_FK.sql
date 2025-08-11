-- =============================================
-- Script: 13-Temporary_Disable_FK.sql
-- Description: Temporarily disable FK constraint to test reservation creation
-- Author: Claude Code Assistant  
-- Date: 2025-01-11
-- WARNING: This is a temporary diagnostic step - FK should be re-enabled after testing
-- =============================================

USE [Hotel]
GO

PRINT '======================================================'
PRINT 'TEMPORARILY DISABLING FK_Registros_Reservas CONSTRAINT'
PRINT 'WARNING: This is for diagnostic purposes only!'
PRINT '======================================================'

-- Disable the foreign key constraint
ALTER TABLE [dbo].[Registros] NOCHECK CONSTRAINT [FK_Registros_Reservas]
PRINT 'FK_Registros_Reservas constraint has been DISABLED'

-- Check constraint status
SELECT 
    'FK Constraint Status' as Info,
    CASE WHEN is_disabled = 1 THEN 'DISABLED' ELSE 'ENABLED' END as Status,
    name as ConstraintName
FROM sys.foreign_keys 
WHERE name = 'FK_Registros_Reservas'

PRINT ''
PRINT 'Try creating a reservation now to test if it works without the FK constraint'
PRINT 'If it works, the issue is confirmed to be a ReservaID reference problem'
PRINT ''
PRINT 'IMPORTANT: Run the re-enable script (14) after testing!'

GO