-- =============================================
-- Script: 12-Debug_Registros_Issue.sql
-- Description: Debug the Registros FK constraint issue
-- Author: Claude Code Assistant  
-- Date: 2025-01-11
-- =============================================

USE [Hotel]
GO

PRINT '======================================================'
PRINT 'DEBUGGING REGISTROS FK CONSTRAINT ISSUE'
PRINT '======================================================'

-- Check for orphaned Registros (ReservaID not in Reservas table)
PRINT 'Checking for orphaned Registros...'
SELECT 
    'Orphaned Registros' as Issue,
    COUNT(*) as Count,
    MIN(ReservaID) as MinReservaID,
    MAX(ReservaID) as MaxReservaID
FROM [dbo].[Registros] r 
WHERE r.[ReservaID] IS NOT NULL 
AND r.[ReservaID] NOT IN (SELECT [ReservaID] FROM [dbo].[Reservas])

-- Check if there are any Registros with ReservaID = 0
PRINT ''
PRINT 'Checking for Registros with ReservaID = 0...'
SELECT COUNT(*) as RegistrosWithZeroReservaID 
FROM [dbo].[Registros] 
WHERE [ReservaID] = 0

-- Check the current default constraint
PRINT ''
PRINT 'Current default constraints for ReservaID...'
SELECT 
    dc.name as ConstraintName,
    dc.definition as DefaultValue,
    c.name as ColumnName,
    t.name as TableName
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
INNER JOIN sys.tables t ON dc.parent_object_id = t.object_id
WHERE t.name = 'Registros' AND c.name = 'ReservaID'

-- Check recent Registros records
PRINT ''
PRINT 'Recent Registros records (last 10)...'
SELECT TOP 10
    RegistroID,
    ReservaID,
    Contenido,
    TipoRegistro,
    Modulo,
    FechaRegistro,
    InstitucionID
FROM [dbo].[Registros] 
ORDER BY FechaRegistro DESC

-- Check if there are any triggers on Registros table
PRINT ''
PRINT 'Checking for triggers on Registros table...'
SELECT 
    t.name as TriggerName,
    t.type_desc as TriggerType,
    OBJECT_NAME(t.parent_id) as TableName
FROM sys.triggers t
WHERE OBJECT_NAME(t.parent_id) = 'Registros'

-- Check if there are any triggers on Reservas table that might create Registros
PRINT ''
PRINT 'Checking for triggers on Reservas table...'
SELECT 
    t.name as TriggerName,
    t.type_desc as TriggerType,
    OBJECT_NAME(t.parent_id) as TableName
FROM sys.triggers t
WHERE OBJECT_NAME(t.parent_id) = 'Reservas'

-- Check the max ReservaID in Reservas table
PRINT ''
PRINT 'Max ReservaID in Reservas table...'
SELECT 
    'Max ReservaID' as Info,
    ISNULL(MAX(ReservaID), 0) as MaxReservaID,
    COUNT(*) as TotalReservas
FROM [dbo].[Reservas]

-- Check if there are any stored procedures that might create Registros
PRINT ''
PRINT 'Stored procedures that mention Registros...'
SELECT DISTINCT
    p.name as ProcedureName,
    p.type_desc as Type
FROM sys.procedures p
CROSS APPLY sys.dm_exec_describe_first_result_set_for_object(p.object_id, NULL) r
WHERE r.name LIKE '%Registro%' OR r.name LIKE '%ReservaID%'
OR EXISTS (
    SELECT 1 FROM sys.sql_modules m 
    WHERE m.object_id = p.object_id 
    AND (m.definition LIKE '%Registros%' OR m.definition LIKE '%ReservaID%')
)

PRINT ''
PRINT 'Investigation completed!'

GO